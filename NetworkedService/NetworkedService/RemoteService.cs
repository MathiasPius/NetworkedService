using System;
using System.Text;
using System.Collections.Generic;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace NetworkedService
{
    public class RemoteService<TInterface> : IRemoteService, IDisposable
    {
        private static Guid InstanceId = Guid.NewGuid();
        private readonly IRemoteProcedureCaller _remoteProcedureCaller;
        private readonly RemoteSessionInformation _remoteSessionInformation;
        //private readonly ConcurrentDictionary<Guid, Task> _activeActions;
        private readonly string _interfaceName;

        public RemoteService(INetworkedScope scope, IRemoteProcedureCaller remoteProcedurePoller, string interfaceName)
        {
            _remoteSessionInformation = new RemoteSessionInformation
            {
                InstanceId = InstanceId,
                ScopeId =  scope?.GetScopeGuid() ?? Guid.Empty,
                ActionId = Guid.Empty
            };

            _remoteProcedureCaller = remoteProcedurePoller;
            _interfaceName = interfaceName;
        }

        public void CallVoidMethod(string methodName, params object[] parameters)
        {
            if (methodName == "Dispose")
            {
                Console.WriteLine("Client: Ignoring call to {0}::Dispose()", _interfaceName);
                return;
            }

            if (_remoteProcedureCaller == null)
                throw new InvalidOperationException();

            var remoteCommand = new RemoteCommand
            {
                RemoteSessionInformation = new RemoteSessionInformation
                {
                    InstanceId = _remoteSessionInformation.InstanceId,
                    ScopeId = _remoteSessionInformation.ScopeId,
                    ActionId = Guid.NewGuid()
                },
                InterfaceName = _interfaceName,
                MethodName = methodName,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);
        }

        private TReturn ConvertResult<TReturn>(object value)
        {
            var returnType = typeof(TReturn);

            var converted = _remoteProcedureCaller.GetSerializer()
                .ConvertResult(value, returnType);

            if(converted.GetType() == returnType)
                return (TReturn)converted;

            if (returnType.IsEnum)
                return (TReturn)Enum.Parse(returnType, converted.ToString());

            return (TReturn)Convert.ChangeType(converted, typeof(TReturn));
        }

        public Task<TReturn> CallAsyncMethod<TReturn>(string methodName, params object[] parameters)
        {
            if (_remoteProcedureCaller == null)
                throw new InvalidOperationException();

            var remoteCommand = new RemoteCommand
            {
                RemoteSessionInformation = new RemoteSessionInformation
                {
                    InstanceId = _remoteSessionInformation.InstanceId,
                    ScopeId = _remoteSessionInformation.ScopeId,
                    ActionId = Guid.NewGuid()
                },
                InterfaceName = _interfaceName,
                MethodName = methodName,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);

            return Task.FromResult(ConvertResult<TReturn>(result.Result));
        }

        public TReturn CallMethod<TReturn>(string methodName, params object[] parameters)
        {
            if (_remoteProcedureCaller == null)
                throw new InvalidOperationException();

            var remoteCommand = new RemoteCommand
            {
                RemoteSessionInformation = new RemoteSessionInformation
                {
                    InstanceId = _remoteSessionInformation.InstanceId,
                    ScopeId = _remoteSessionInformation.ScopeId,
                    ActionId = Guid.NewGuid()
                },
                InterfaceName = _interfaceName,
                MethodName = methodName,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);
            return ConvertResult<TReturn>(result.Result);
        }

        public void ScopeDestroyed(INetworkedScope networkedScope)
        {
            // If this scope id doesn't belong to us, we don't give a shit
            if (_remoteSessionInformation.ScopeId != networkedScope.GetScopeGuid())
                return;
            
            // Send the command
            var result = _remoteProcedureCaller.CallMethod(new RemoteCommand
            {
                MethodName = RemoteServiceHost.DestroyScope,
                RemoteSessionInformation = new RemoteSessionInformation
                {
                    ScopeId = networkedScope.GetScopeGuid(),
                }
            });
        }

        public string GetInterfaceName()
        {
            return _interfaceName;
        }

        public void Dispose()
        {
            _remoteProcedureCaller.Dispose();
        }
    }
}
