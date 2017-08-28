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

namespace NetworkedService.Transport.NetMQ
{
    public class Server : IRemoteProcedureListener
    {
        private readonly ICommandDeserializer _commandDeserializer;
        private readonly ResponseSocket _responseSocket;

        public Server(string endpoint, ICommandDeserializer commandDeserializer)
        {
            _commandDeserializer = commandDeserializer;
            _responseSocket = new ResponseSocket(endpoint);
        }

        public RemoteCommand Receive()
        {
            var data = _responseSocket.ReceiveFrameBytes();
            return _commandDeserializer.DeserializeCommand(data);
        }

        public void Reply(RemoteResult remoteResult)
        {
            _responseSocket.SendFrame(_commandDeserializer.SerializeResult(remoteResult));
        }

        public ICommandDeserializer GetSerializer()
        {
            return _commandDeserializer;
        }
    }
}
