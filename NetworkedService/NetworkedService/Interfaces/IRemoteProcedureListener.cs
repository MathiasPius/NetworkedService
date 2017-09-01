using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetworkedService.Interfaces
{
    public interface IRemoteProcedureListener : IDisposable
    {
        RemoteCommand Receive();
        void Reply(RemoteResult remoteResult);
        ICommandDeserializer GetSerializer();
    }
}
