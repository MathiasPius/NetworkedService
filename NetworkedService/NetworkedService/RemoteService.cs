using System;
using System.Text;
using System.Collections.Generic;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

namespace NetworkedService
{
    public class RemoteService<TInterface>
    {
        private static Guid InstanceId = Guid.NewGuid();
        private readonly IRemoteProcedureCaller _remoteProcedureCaller;
        private readonly IRemoteProcedureSerializer _remoteProcedureSerializer;
        private readonly RemoteSessionInformation _remoteSessionInformation;

        private readonly MethodDictionary _methodDictionary;

        public RemoteService(IRemoteProcedureCaller remoteProcedureCaller, IRemoteProcedureSerializer remoteProcedureSerializer, MethodDictionary methodDictionary)
        {
            _remoteProcedureSerializer = remoteProcedureSerializer;
            _methodDictionary = methodDictionary;

            _remoteSessionInformation = new RemoteSessionInformation
            {
                InstanceId = InstanceId,
                ScopeId =  Guid.Empty,
                ActionId = Guid.Empty
            };

            _remoteProcedureCaller = remoteProcedureCaller;
        }

        public void CallVoidMethod(RemoteProcedureDescriptor descriptor, params object[] parameters)
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
                RemoteProcedureDescriptor = descriptor,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);
            if(result.Exception != null)
            {
                throw new RemoteException("Remote Service threw an exception", ConvertResult<Exception>(result.Exception));
            }
        }

        public void CallVoidMethod(string descriptor, params object[] parameters)
            => CallVoidMethod(new RemoteProcedureDescriptor(Guid.Parse(descriptor)), parameters);

        private TReturn ConvertResult<TReturn>(object value)
        {
            if (value == null)
                return default(TReturn);

            var returnType = typeof(TReturn);

            var converted = _remoteProcedureSerializer
                .ConvertObject(value, returnType);

            if(converted.GetType() == returnType)
                return (TReturn)converted;

            if (returnType.IsEnum)
                return (TReturn)Enum.Parse(returnType, converted.ToString());

            if (returnType.IsAssignableFrom(converted.GetType()))
                return (TReturn)converted;

            return (TReturn)Convert.ChangeType(converted, typeof(TReturn));
        }

        public Task<TReturn> CallAsyncMethod<TReturn>(RemoteProcedureDescriptor descriptor, params object[] parameters)
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
                RemoteProcedureDescriptor = descriptor,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);

            return Task.FromResult(ConvertResult<TReturn>(result.Result));
        }

        public Task<TReturn> CallAsyncMethod<TReturn>(string descriptor, params object[] parameters)
            => CallAsyncMethod<TReturn>(new RemoteProcedureDescriptor(Guid.Parse(descriptor)), parameters);

        public TReturn CallMethod<TReturn>(RemoteProcedureDescriptor descriptor, params object[] parameters)
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
                RemoteProcedureDescriptor = descriptor,
                Parameters = parameters
            };

            var result = _remoteProcedureCaller.CallMethod(remoteCommand);

            if (result.Exception != null)
            {
                throw new RemoteException("Remote Service threw an exception", ConvertResult<Exception>(result.Exception));
            }
            else
            {
                return ConvertResult<TReturn>(result.Result);
            }
        }

        public TReturn CallMethod<TReturn>(string descriptor, params object[] parameters)
            => CallMethod<TReturn>(new RemoteProcedureDescriptor(Guid.Parse(descriptor)), parameters);
    }
}
