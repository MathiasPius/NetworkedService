using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Interfaces;
using NetworkedService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkedService
{
    public class RemoteServiceHost
    {
        public const string DestroyScope = @"@DestroyScope";

        private readonly ConcurrentDictionary<Guid, IServiceProvider> _scopes;
        private readonly ConcurrentDictionary<Type, IServiceProvider> _exposedInterfaces;
        private readonly IRemoteProcedureListener _remoteProcedureListener;

        public RemoteServiceHost(IRemoteProcedureListener remoteProcedureListener)
        {
            _scopes = new ConcurrentDictionary<Guid, IServiceProvider>();
            _exposedInterfaces = new ConcurrentDictionary<Type, IServiceProvider>();
            _remoteProcedureListener = remoteProcedureListener;
        }

        public void ExposeInterface<TInterface>(IServiceProvider serviceProvider)
            where TInterface: class
        {
            _exposedInterfaces.GetOrAdd(typeof(TInterface), serviceProvider);
        }

        public RemoteResult ParseMessage(RemoteCommand remoteCommand)
        {
            if (remoteCommand.MethodName == DestroyScope && remoteCommand.InterfaceName == null)
            {
                Console.WriteLine("Server: Destroying Networked Scope: " + remoteCommand.RemoteSessionInformation.ScopeId);

                IServiceProvider outValue;
                _scopes.Remove(remoteCommand.RemoteSessionInformation.ScopeId, out outValue);
                return new RemoteResult
                {
                    RemoteSessionInformation = remoteCommand.RemoteSessionInformation,
                    Result = true
                };
            }
            else
            {
                var servicePair = _exposedInterfaces
                    .FirstOrDefault(i => i.Key.Name == remoteCommand.InterfaceName);

                var serviceType = servicePair.Key;
                var serviceProvider = servicePair.Value;

                // Get the scope in which this action is to take place
                var serviceScopeId = remoteCommand.RemoteSessionInformation.ScopeId;

                
                var serviceScope = _scopes.GetOrAdd(serviceScopeId, serviceProvider.CreateScope().ServiceProvider);

                // Get the actual service instance
                var service = serviceScope.GetService(serviceType);

                // Get the method to call on the service
                // TODO: Perhaps cache these methods instead of looking up every time?
                var method = serviceType.GetNestedMethod(
                    remoteCommand.MethodName,
                    BindingFlags.Instance | BindingFlags.Public
                );


                // Convert the parameters from the RemoteCommand into correct types
                var parameterTypes = method.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                var parameters = _remoteProcedureListener
                    .GetSerializer()
                    .ConvertParameters(remoteCommand.Parameters, parameterTypes);

                // Make the call
                Console.WriteLine("Server: Calling " + remoteCommand.InterfaceName + "::" + remoteCommand.MethodName);
                var returnValue = method.Invoke(service, parameters);

                var result = new RemoteResult
                {
                    RemoteSessionInformation = remoteCommand.RemoteSessionInformation,
                    Result = returnValue
                };

                return result;
            }
        }

        public void Listen()
        {
            var activeActions = new List<Task>();

            activeActions.Add(Task.Run(() =>
            {
                while (true)
                {
                    var remoteCommand = _remoteProcedureListener.Receive();
                    activeActions.Add(Task.Run(() =>
                    {
                        _remoteProcedureListener.Reply(ParseMessage(remoteCommand));
                    }));
                }
            }));

            Parallel.ForEach(activeActions, (t) => t.Wait());
        }

        public async Task ListenAsync()
        {
            await Task.Run(() => Listen());
        }
    }
}
