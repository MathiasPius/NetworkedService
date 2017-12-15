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
        private readonly string _hostname;

        public Client(string hostname, int port, IRemoteProcedureSerializer commandSerializer)
        {
            _commandSerializer = commandSerializer;

            // Find the first IPv4 address
            var address = Dns.GetHostAddresses(hostname)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            _address = new IPEndPoint(address, port);
            _hostname = hostname;
        }

        public RemoteResult CallMethod(RemoteCommand remoteCommand)
        {
            try
            {
                var client = TryConnect(_address);

                var msg = _commandSerializer.SerializeCommand(remoteCommand);

                client.WriteFullPacket(msg);
                var reply = client.ReadFullPacket();

                if(reply == null)
                    throw new InvalidOperationException("Reply message contained invalid signature");

                return _commandSerializer.DeserializeResult(reply);
            } catch(SocketException se)
            {
                throw new InvalidOperationException(string.Format("Failed to call remote method on {0}: {1}:{2}", _hostname, _address.Address.ToString(), _address.Port), se);
            }
        }

        public static Func<IServiceProvider, Client> Factory(string hostname, int port)
        {
            return (serviceProvider) =>
            {
                var commandSerializer = serviceProvider.GetService<IRemoteProcedureSerializer>();
                return new Client(hostname, port, commandSerializer);
            };
        }

        private Socket TryConnect(IPEndPoint endpoint, int timeoutRetry = 10)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            do
            {
                try
                {
                    if (socket.ConnectAsync(endpoint).Wait(1000))
                        return socket;
                }
                catch(SocketException)
                {
                    --timeoutRetry;
                    Thread.Sleep(500);
                }
                catch (AggregateException ae)
                {
                    ae.Handle(x =>
                    {
                        if (x is SocketException)
                        {
                            --timeoutRetry;
                            Thread.Sleep(500);
                            return true;
                        }

                        return false;
                    });
                }
            } while (timeoutRetry >= 0);

            return socket;
        }
    }
}
