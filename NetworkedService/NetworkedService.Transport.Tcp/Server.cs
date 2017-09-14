using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System.Linq;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace NetworkedService.Transport.Tcp
{
    public class Server : IRemoteProcedureListener
    {
        private readonly Dictionary<SessionToken, Socket> _activeSessions;
        private readonly TcpListener _tcpListener;
        private readonly IPAddress _address;
        private readonly int _port;

        public Server(string hostname, int port)
            : this(
                  Dns.GetHostAddresses(hostname)
                    .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork), 
                  port
            ) { }

        public Server(IPAddress address, int port)
        {
            _activeSessions = new Dictionary<SessionToken, Socket>();
            _tcpListener = new TcpListener(address, port);
            _address = address;
            _port = port;

            _tcpListener.Start();
        }

        public Session Receive()
        {
            var socket = _tcpListener.AcceptSocket();
            socket.NoDelay = true;

            var session = new Session
            {
                Token = SessionToken.NewToken(),
                Message = socket.ReadFullPacket()
            };

            _activeSessions[session.Token] = socket;
            return session;
        }

        public void Reply(SessionToken sessionToken, byte[] reply)
        {
            _activeSessions.Remove(sessionToken, out Socket client);

            client.WriteFullPacket(reply);
            client.Close();

        }
    }
}
