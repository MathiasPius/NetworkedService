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
            _address = new IPEndPoint(address, port);
            //_requestSocket = new DealerSocket(endpoint);
            //_requestSocket.Options.Identity = _identity.ToByteArray();
            _commandSerializer = commandSerializer;

            // Find the first IPv4 address
            var address = Dns.GetHostAddresses(hostname)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }

        public RemoteResult CallMethod(RemoteCommand remoteCommand)
        {
            var client = new TcpClient();
            client.Connect(_address);
            client.NoDelay = true;

            //var msg = new NetMQMessage();
            var msg = _commandSerializer.SerializeCommand(remoteCommand);
            var stream = client.GetStream();

            byte[] length = BitConverter.GetBytes(msg.Length);
            // Prepend the packet with the length of our packet
            stream.Write(length, 0, length.Length);
            stream.Write(msg, 0, msg.Length);
            stream.Flush();
            
            // Read the actual packet
            stream.Read(length, 0, length.Length);
            var replyLength = BitConverter.ToInt32(length, 0);
            var reply = new byte[replyLength];
            stream.Read(reply, 0, replyLength);

            stream.Close();
            client.Close();
            return _commandSerializer.DeserializeResult(reply);
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

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
