using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace NetworkedService.Transport.Tcp
{
    public class Client : IRemoteProcedureCaller
    {
        private readonly Guid _identity = Guid.NewGuid();
        //private readonly DealerSocket _requestSocket;
        private readonly ICommandSerializer _commandSerializer;
        private readonly IPEndPoint _address;

        public Client(string hostname, int port, ICommandSerializer commandSerializer)
        {
            //_requestSocket = new DealerSocket(endpoint);
            //_requestSocket.Options.Identity = _identity.ToByteArray();
            _commandSerializer = commandSerializer;

            // Find the first IPv4 address
            var address = Dns.GetHostAddresses(hostname)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            _address = new IPEndPoint(address, port);
        }

        public RemoteResult CallMethod(RemoteCommand remoteCommand)
        {
            var client = new TcpClient();
            client.NoDelay = true;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    client.Connect(_address);
                    break;
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.TimedOut)
                        throw e;
                }
            }

            var msg = _commandSerializer.SerializeCommand(remoteCommand);
            Console.WriteLine("Client: Transmitting {0} bytes of data", msg.Length);

            using (var stream = client.GetStream())
            {
                stream.WriteFullPacket(msg);
                var reply = stream.ReadFullPacket();

                return _commandSerializer.DeserializeResult(reply);
            }
        }

        public static Func<IServiceProvider, Client> Factory(string hostname, int port)
        {
            return (serviceProvider) =>
            {
                var commandSerializer = serviceProvider.GetService<ICommandSerializer>();
                return new Client(hostname, port, commandSerializer);
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
