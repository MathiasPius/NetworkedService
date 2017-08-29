using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using NetMQ;
using NetMQ.Sockets;

using NetworkedService.Interfaces;
using NetworkedService.Models;

namespace NetworkedService.Transport.NetMQ
{
    public class Client : IRemoteProcedureCaller
    {
        private readonly Guid _identity = Guid.NewGuid();
        private readonly DealerSocket _requestSocket;
        private readonly ICommandSerializer _commandSerializer;

        public Client(string endpoint, ICommandSerializer commandSerializer)
        {
            _requestSocket = new DealerSocket(endpoint);
            _requestSocket.Options.Identity = _identity.ToByteArray();
            _commandSerializer = commandSerializer;
        }

        public RemoteResult CallMethod(RemoteCommand remoteCommand)
        {
            var msg = new NetMQMessage();
            msg.Append(_commandSerializer.SerializeCommand(remoteCommand));

            _requestSocket.SendMultipartMessage(msg);

            var reply = _requestSocket.ReceiveMultipartMessage(1);
            
            return _commandSerializer.DeserializeResult(reply[0].ToByteArray());
        }

        public static Func<IServiceProvider, Client> Factory(string endpoint)
        {
            return (serviceProvider) =>
            {
                var commandSerializer = serviceProvider.GetService<ICommandSerializer>();
                return new Client(endpoint, commandSerializer);
            };
        }

        public void ScopeDestroyed(INetworkedScope networkedScope)
        {
            // We destroy scopes by sending an "empty" RemoteCommand method to the server
            // specifying only the scope Guid to be destroyed
            var result = CallMethod(new RemoteCommand
            {
                MethodName = "@DestroyScope",
                Parameters = null,
                RemoteSessionInformation = new RemoteSessionInformation
                {
                    InstanceId = Guid.Empty,
                    ScopeId = networkedScope.GetScopeGuid(),
                    ActionId = Guid.Empty
                }
            });
        }

        public ICommandSerializer GetSerializer()
        {
            return _commandSerializer;
        }
    }
}
