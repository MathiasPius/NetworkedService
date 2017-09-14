using System;
using Microsoft.Extensions.DependencyInjection;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Threading;

namespace NetworkedService.Transport.Tcp
{
    public class Client : IRemoteProcedureCaller
    {
        private readonly Guid _identity = Guid.NewGuid();
        private readonly IRemoteProcedureSerializer _commandSerializer;
        private readonly IPEndPoint _address;

        public Client(string hostname, int port, IRemoteProcedureSerializer commandSerializer)
        {
            _commandSerializer = commandSerializer;

            // Find the first IPv4 address
            var address = Dns.GetHostAddresses(hostname)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            _address = new IPEndPoint(address, port);
        }

        public RemoteResult CallMethod(RemoteCommand remoteCommand)
        {
            var client = TcpHelper.TryConnect(_address);

            var msg = _commandSerializer.SerializeCommand(remoteCommand);

            client.WriteFullPacket(msg);
            var reply = client.ReadFullPacket();
            return _commandSerializer.DeserializeResult(reply);
        }

        public static Func<IServiceProvider, Client> Factory(string hostname, int port)
        {
            return (serviceProvider) =>
            {
                var commandSerializer = serviceProvider.GetService<IRemoteProcedureSerializer>();
                return new Client(hostname, port, commandSerializer);
            };
        }

        public IRemoteProcedureSerializer GetSerializer()
        {
            return _commandSerializer;
        }
    }
}
