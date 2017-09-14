using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Interfaces
{
    public interface IRemoteProcedureSerializer
    {
        RemoteCommand DeserializeCommand(byte[] remoteCommand);
        RemoteResult DeserializeResult(byte[] remoteResult);
        byte[] SerializeCommand(RemoteCommand remoteCommand);
        byte[] SerializeResult(RemoteResult remoteResult);
        object ConvertObject(object result, Type resultType);
    }
}
