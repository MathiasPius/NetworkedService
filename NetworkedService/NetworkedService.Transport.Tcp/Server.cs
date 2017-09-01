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
            var socket = _tcpListener.AcceptTcpClient();
            socket.NoDelay = true;

            var stream = socket.GetStream();

            // Read the incoming packet size first
            var lengthBuffer = BitConverter.GetBytes(new Int32());
            stream.Read(lengthBuffer, 0, lengthBuffer.Length);

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var buffer = new byte[length];

            Console.WriteLine("Server: Receiving {0} bytes of data", length);

            int offset = 0;
            while (offset < length)
            {
                offset += stream.Read(buffer, offset, length - offset);
                Console.WriteLine("Server: Received Bytes: {0}/{1}", offset, length);
            }

            var command = _commandDeserializer.DeserializeCommand(buffer);

            _activeSessions[command.RemoteSessionInformation] = socket;

            return command;
        }

        public void Reply(RemoteResult remoteResult)
        {
            TcpClient client;
            _activeSessions.Remove(remoteResult.RemoteSessionInformation, out client);

            var reply = _commandDeserializer.SerializeResult(remoteResult);

            Console.WriteLine("Server: Writing {0} bytes of data", reply.Length);

            var stream = client.GetStream();
            byte[] length = BitConverter.GetBytes(reply.Length);
            // Prepend the packet with the length of our packet
            stream.Write(length, 0, length.Length);
            stream.Write(reply, 0, reply.Length);
            stream.Flush();
            stream.Close();
            client.Close();
        }

        public ICommandDeserializer GetSerializer()
        {
            return _commandDeserializer;
        }
    }
}
