using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Interfaces;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace NetworkedService
{
    public class RemoteServiceFactory<TInterface>
    {
        public static Func<IServiceProvider, TInterface> Factory(IRemoteProcedureCaller remoteProcedureCaller)
        {
            return (IServiceProvider serviceProvider) =>
            {
                var scope = serviceProvider.GetService<INetworkedScope>();
                var instance = Activator.CreateInstance(CachedInterfaceWrapper.CachedType, scope, remoteProcedureCaller);

                if (scope != null)
                    scope.AddClient((IRemoteService)instance);

                return (TInterface)instance;
            };
        }

        public static Func<IServiceProvider, TInterface> Factory(Func<IServiceProvider, IRemoteProcedureCaller> rpcFactory)
        {
            return (IServiceProvider serviceProvider) =>
            {
                // Construct the IRPC from its own factory first, then call the TInterface constructor with the IRPC
                return Factory(rpcFactory(serviceProvider))(serviceProvider);
            };
        }

        private static class CachedInterfaceWrapper
        {
            internal static Type CachedType;

            // Get all methods declared by this interface, including those of 
            // nested interfaces

            static CachedInterfaceWrapper()
            {
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
                    new Type[] { typeof(INetworkedScope), typeof(IRemoteProcedureCaller) }
                );

                constructorBuilder.DefineParameter(1, ParameterAttributes.None, "scope");
                constructorBuilder.DefineParameter(2, ParameterAttributes.None, "remoteProcedureCaller");

                var baseConstructor = baseType.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new Type[] { typeof(INetworkedScope), typeof(IRemoteProcedureCaller) }, null
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
                {
                    Console.WriteLine("Client: Building implementation of " + method.DeclaringType.Name + ":" + method.Name);
                    AddMethodCall(typeBuilder, method);
                }

                CachedType = typeBuilder.CreateType();
            }

            private static void AddMethodCall(TypeBuilder typeBuilder, MethodInfo methodInfo)
            {
                // Get the CallMethod generic method from the RemoteService
                var baseType = typeof(RemoteService<TInterface>);

                MethodInfo callMethod;
                if (methodInfo.ReturnType == typeof(void))
                    callMethod = baseType.GetMethod("CallVoidMethod", new Type[] { typeof(string), typeof(object[]) });
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
                method.Emit(OpCodes.Ldstr, methodInfo.Name);

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

                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }
        }
    }
}
