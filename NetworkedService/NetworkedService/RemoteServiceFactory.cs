using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Interfaces;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using NetworkedService.Models;

namespace NetworkedService
{
    public static class RemoteServiceFactory<TInterface>
        where TInterface : class
    {
        public static Func<IServiceProvider, TInterface> Factory(IRemoteProcedureCaller remoteProcedureCaller)
        {
            StaticInterfaceDescriptor<TInterface>.Build();

            return (IServiceProvider serviceProvider) =>
            {
                var type = StaticInterfaceDescriptor<TInterface>.CachedType;
                var dictionary = StaticInterfaceDescriptor<TInterface>.MethodDictionary;
                var instance = Activator.CreateInstance(type, remoteProcedureCaller, dictionary);

                return (TInterface)instance;
            };
        }

        public static Func<IServiceProvider, TInterface> Factory(IRemoteProcedureCaller remoteProcedureCaller, InterfaceHash interfaceHash)
        {
            StaticInterfaceDescriptor<TInterface>.Build();

            return (IServiceProvider serviceProvider) =>
            {
                var type = StaticInterfaceDescriptor<TInterface>.CachedType;
                var dictionary = StaticInterfaceDescriptor<TInterface>.MethodDictionary;
                var instance = Activator.CreateInstance(type, remoteProcedureCaller, dictionary);

                return (TInterface)instance;
            };
        }

        public static Func<IServiceProvider, object> Factory(Func<IServiceProvider, IRemoteProcedureCaller> rpcFactory)
        {
            return (IServiceProvider serviceProvider) =>
            {
                // Construct the IRPC from its own factory first, then call the TInterface constructor with the IRPC
                return Factory(rpcFactory(serviceProvider))(serviceProvider);
            };
        }

        public static Func<IServiceProvider, object> Factory(Func<IServiceProvider, IRemoteProcedureCaller> rpcFactory, InterfaceHash interfaceHash)
        {
            return (IServiceProvider serviceProvider) =>
            {
                // Construct the IRPC from its own factory first, then call the TInterface constructor with the IRPC
                return Factory(rpcFactory(serviceProvider), interfaceHash)(serviceProvider);
            };
        }
    }

    public static class StaticInterfaceDescriptor<TInterface>
        where TInterface: class
    {
        public static Type CachedType;
        public static MethodDictionary MethodDictionary;

        static StaticInterfaceDescriptor()
        {
            MethodDictionary = new MethodDictionary();
            MethodDictionary.AddInterface<TInterface>();

            var iface = typeof(TInterface);
            var ifaceName = iface.Name;
            var implName = ifaceName.Substring(1);
            var baseType = typeof(RemoteService<TInterface>);

            // Create the hosting assembly
            var assemblyName = new AssemblyName("Implementation." + iface.Name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            // Create module
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Main");

            // Create the implementation
            var typeBuilder = moduleBuilder
                .DefineType("Remote" + implName + "Service", 0
                | TypeAttributes.Public
                | TypeAttributes.Class
                | TypeAttributes.AutoLayout
                | TypeAttributes.Sealed
                , baseType
                , new Type[] { iface });

            // Create the constructor
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                new Type[] { typeof(IRemoteProcedureCaller), typeof(MethodDictionary) }
            );

            constructorBuilder.DefineParameter(1, ParameterAttributes.None, "remoteProcedureCaller");
            constructorBuilder.DefineParameter(2, ParameterAttributes.None, "remoteProcedureDescriptor");

            var baseConstructor = baseType.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance, null,
                new Type[] { typeof(IRemoteProcedureCaller), typeof(MethodDictionary) }, null
            );

            // Tell our constructor to call the RemoteService<TInterface> constructor
            var constructor = constructorBuilder.GetILGenerator();
            constructor.Emit(OpCodes.Ldarg_0);
            constructor.Emit(OpCodes.Ldarg_1);
            constructor.Emit(OpCodes.Ldarg_2);
            constructor.Emit(OpCodes.Call, baseConstructor);
            constructor.Emit(OpCodes.Nop);
            constructor.Emit(OpCodes.Nop);
            constructor.Emit(OpCodes.Ret);


            var methods = iface.GetAllMethods(BindingFlags.Public | BindingFlags.Instance);
            // Implement the functions of the interface
            foreach (var method in methods)
                AddMethodCall(typeBuilder, method);

            CachedType = typeBuilder.CreateType();
        }

        public static void Build()
        {
            // This does nothing, but ensure that the constructor has been called
        }

        private static void AddMethodCall(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            // Get the CallMethod generic method from the RemoteService
            var baseType = typeof(RemoteService<TInterface>);

            MethodInfo callMethod;
            if (methodInfo.ReturnType == typeof(void))
                callMethod = baseType.GetMethod("CallVoidMethod", new Type[] { typeof(string), typeof(object[]) });
            else if (methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                callMethod = baseType.GetMethod("CallAsyncMethod", new Type[] { typeof(string), typeof(object[]) })
                    .MakeGenericMethod(methodInfo.ReturnType.GetGenericArguments()[0]);
            else
                callMethod = baseType.GetMethod("CallMethod", new Type[] { typeof(string), typeof(object[]) })
                    .MakeGenericMethod(methodInfo.ReturnType);


            var methodBuilder = typeBuilder.DefineMethod(
                methodInfo.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                methodInfo.ReturnType,
                methodInfo.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray()
            );

            var method = methodBuilder.GetILGenerator();
            method.Emit(OpCodes.Ldarg_0);
            // TODO: Find a way to load the binary RemoteProcedureDescriptor here instead of
            // making it into a string first, but I can't find a good way to do that yet.
            method.Emit(OpCodes.Ldstr, MethodDictionary.FindDescriptor(methodInfo).ToGuid().ToString());

            var methodParams = methodInfo.GetParameters();
            method.DeclareLocal(typeof(object[]));
            method.Emit(OpCodes.Ldc_I4, methodParams.Count());
            method.Emit(OpCodes.Newarr, typeof(object));
            foreach (var param in methodParams)
            {
                method.Emit(OpCodes.Dup);
                method.Emit(OpCodes.Ldc_I4, param.Position);
                method.Emit(OpCodes.Ldarg, param.Position + 1);
                method.Emit(OpCodes.Box, param.ParameterType);
                method.Emit(OpCodes.Stelem_Ref);
            }
            method.Emit(OpCodes.Call, callMethod);
            method.Emit(OpCodes.Ret);
        }
    }
}
