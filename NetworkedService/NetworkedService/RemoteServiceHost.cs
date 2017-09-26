using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Interfaces;
using NetworkedService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkedService
{
    public class RemoteServiceHost
    {
        private readonly Dictionary<ServiceHash, MethodDictionary> _methodDictionaries = new Dictionary<ServiceHash, MethodDictionary>();
        private readonly List<Type> _exposedInterfaces = new List<Type>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IRemoteProcedureListener _remoteProcedureListener;
        private readonly IRemoteProcedureSerializer _serializer;

        public RemoteServiceHost(IServiceProvider serviceProvider, IRemoteProcedureListener remoteProcedureListener, IRemoteProcedureSerializer serializer)
        {
            _serviceProvider = serviceProvider;
            _remoteProcedureListener = remoteProcedureListener;
            _serializer = serializer;
        }

        public void ExposeInterface<TInterface>()
            where TInterface: class
        {
            var methods = new MethodDictionary();
            methods.AddInterface<TInterface>();

            _methodDictionaries.Add(methods.GetServiceHash(), methods);
            _exposedInterfaces.Add(typeof(TInterface));
        }

        private object ConvertParameter(object value, Type returnType)
        {
            if (value == null)
                return value;

            if (value.GetType() == returnType)
                return value;

            if (returnType.IsEnum)
                return Enum.Parse(returnType, value.ToString());

            try
            {
                return Convert.ChangeType(value, returnType);
            }
            catch(Exception)
            {
                return Activator.CreateInstance(returnType, value);
            }
        }

        public RemoteResult ParseMessage(RemoteCommand remoteCommand)
        {
            var methods = _methodDictionaries.GetValueOrDefault(remoteCommand.RemoteProcedureDescriptor.ServiceHash);

            // Get the method to call on the service
            var lookup = methods.FindMethod(remoteCommand.RemoteProcedureDescriptor);

            var serviceType = methods.GetPrimaryInterface();
            var method = lookup.Item2;

            // Get the actual service instance
            var service = _serviceProvider.GetService(serviceType);

            // Convert the parameters from the RemoteCommand into correct types
            var parameterTypes = method.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
                
            var parameters = remoteCommand.Parameters
                .Zip(parameterTypes, (o, t) => ConvertParameter(_serializer.ConvertObject(o, t), t))
                .ToArray();

            try
            {
                var returnValue = method.Invoke(service, parameters);
                // Make the call

                var result = new RemoteResult
                {
                    RemoteSessionInformation = remoteCommand.RemoteSessionInformation,
                    Result = returnValue
                };

                var returnType = method.ReturnType;
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var task = ((Task)returnValue);
                    task.Wait();

                    var resultProperty = typeof(Task<>).MakeGenericType(returnType.GetGenericArguments()).GetProperty("Result");
                    result.Result = resultProperty.GetValue(task);
                }

                return result;
            } catch(TargetInvocationException e)
            {
                return new RemoteResult
                {
                    RemoteSessionInformation = remoteCommand.RemoteSessionInformation,
                    Exception = e.InnerException
                };
            } catch(Exception e)
            {
                return new RemoteResult
                {
                    RemoteSessionInformation = remoteCommand.RemoteSessionInformation,
                    Exception = e
                };
            }
        }

        public void Listen()
        {
            var activeActions = new List<Task>();

            while (true)
            {
                var session = _remoteProcedureListener.Receive();
                var remoteCommand = _serializer.DeserializeCommand(session.Message);
                var result = ParseMessage(remoteCommand);

                _remoteProcedureListener.Reply(session.Token, _serializer.SerializeResult(result));
            }
        }
    }
}
