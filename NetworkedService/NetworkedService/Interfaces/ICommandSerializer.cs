using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Interfaces
{
    public interface ICommandSerializer
    {
        byte[] SerializeCommand(RemoteCommand remoteCommand);
        RemoteResult DeserializeResult(byte[] remoteResult);
        object ConvertResult(object result, Type resultType);
    }
}
