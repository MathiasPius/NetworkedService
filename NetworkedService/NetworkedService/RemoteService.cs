using System;
using System.Text;
using System.Collections.Generic;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System.ComponentModel;

namespace NetworkedService
{
    public class RemoteService<TInterface> : IRemoteService
    {
        private static Guid InstanceId = Guid.NewGuid();
        private readonly IRemoteProcedureCaller _remoteProcedureCaller;
        private readonly RemoteSessionInformation _remoteSessionInformation;

        public RemoteService(INetworkedScope scope, IRemoteProcedureCaller remoteProcedurePoller)
        {
            _remoteSessionInformation = new RemoteSessionInformation
            {
                InstanceId = InstanceId,
                ScopeId =  scope?.GetScopeGuid() ?? Guid.Empty,
                ActionId = Guid.Empty
            };

            _remoteProcedureCaller = remoteProcedurePoller;
        }

        public void CallVoidMethod(string methodName, params object[] parameters)
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
                InterfaceName = typeof(TInterface).Name,
                MethodName = methodName,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);
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
                InterfaceName = typeof(TInterface).Name,
                MethodName = methodName,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);

            return (TReturn)_remoteProcedureCaller.GetSerializer()
                .ConvertResult(result.Result, typeof(TReturn));
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
    }
}
