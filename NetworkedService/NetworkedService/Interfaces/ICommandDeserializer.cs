using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Interfaces
{
    public interface ICommandDeserializer
    {
        RemoteCommand DeserializeCommand(byte[] rawData);
        byte[] SerializeResult(RemoteResult remoteResult);
    }
}
