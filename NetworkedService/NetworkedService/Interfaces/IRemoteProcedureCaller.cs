using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Interfaces
{
    public interface IRemoteProcedureCaller : IDisposable
    {
        RemoteResult CallMethod(RemoteCommand remoteCommand);
        ICommandSerializer GetSerializer();
    }
}
