using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using NetworkedService.Interfaces;
using NetMQ;
using NetMQ.Sockets;
using NetworkedService.Models;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace NetworkedService.Transport.NetMQ
{
    public class Server : IRemoteProcedureListener
    {
        private readonly ICommandDeserializer _commandDeserializer;
        private readonly RouterSocket _responseSocket;
        private readonly ConcurrentDictionary<RemoteSessionInformation, NetMQFrame> _activeSessions;

        public Server(string endpoint, ICommandDeserializer commandDeserializer)
        {
            _commandDeserializer = commandDeserializer;
            _responseSocket = new RouterSocket(endpoint);
            _activeSessions = new ConcurrentDictionary<RemoteSessionInformation, NetMQFrame>();
        }

        public RemoteCommand Receive()
        {
            var msg = _responseSocket.ReceiveMultipartMessage(3);
            var command = _commandDeserializer.DeserializeCommand(msg[2].Buffer);

            _activeSessions[command.RemoteSessionInformation] = msg[0];

            return command;
        }

        public void Reply(RemoteResult remoteResult)
        {
            NetMQFrame ident;
            _activeSessions.Remove(remoteResult.RemoteSessionInformation, out ident);

            var reply = new NetMQMessage(3);
            reply.Append(ident);
            reply.AppendEmptyFrame();
            reply.Append(_commandDeserializer.SerializeResult(remoteResult));

            _responseSocket.SendMultipartMessage(reply);
        }

        public ICommandDeserializer GetSerializer()
        {
            return _commandDeserializer;
        }

        public void Dispose()
        {
            _responseSocket.Dispose();
        }
    }
}
