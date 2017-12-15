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
        private readonly Dictionary<SessionToken, Socket> _activeSessions = new Dictionary<SessionToken, Socket>();
        private readonly TcpListener _tcpListener;
        private readonly IPEndPoint _endpoint;

        public Server(IPEndPoint endpoint)
        {
            _endpoint = endpoint;

            _tcpListener = new TcpListener(endpoint);
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

            if(session.Message == null)
            {
                socket.Close();
                return null;
            }

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
