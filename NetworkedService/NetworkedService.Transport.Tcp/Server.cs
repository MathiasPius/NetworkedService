using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;

namespace NetworkedService.Transport.Tcp
{
    public class Server : IRemoteProcedureListener
    {
        private readonly ConcurrentDictionary<RemoteSessionInformation, TcpClient> _activeSessions;
        private readonly ICommandDeserializer _commandDeserializer;
        private readonly TcpListener _tcpListener;
        private readonly IPAddress _address;
        private readonly int _port;

        public Server(string hostname, int port, ICommandDeserializer commandDeserializer)
            : this(
                  Dns.GetHostAddresses(hostname)
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork), 
                 port, 
                 commandDeserializer
            ) { }

        public Server(IPAddress address, int port, ICommandDeserializer commandDeserializer)
        {
            _activeSessions = new ConcurrentDictionary<RemoteSessionInformation, TcpClient>();
            _commandDeserializer = commandDeserializer;
            _tcpListener = new TcpListener(address, port);
            _address = address;
            _port = port;

            _tcpListener.Start();
        }

        public RemoteCommand Receive()
        {
            var client = _tcpListener.AcceptTcpClient();
            client.NoDelay = true;

            var stream = client.GetStream();
            var message = stream.ReadFullPacket();
            var command = _commandDeserializer.DeserializeCommand(message);

            _activeSessions[command.RemoteSessionInformation] = client;

            return command;
            
        }

        public void Reply(RemoteResult remoteResult)
        {
            TcpClient client;
            _activeSessions.Remove(remoteResult.RemoteSessionInformation, out client);

            var reply = _commandDeserializer.SerializeResult(remoteResult);

            Console.WriteLine("Server: Writing {0} bytes of data", reply.Length);

            client.GetStream().WriteFullPacket(reply);
            client.Close();
        }

        public ICommandDeserializer GetSerializer()
        {
            return _commandDeserializer;
        }
    }
}
