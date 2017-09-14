using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Interfaces
{
    public interface IRemoteProcedureCaller
    {
        RemoteResult CallMethod(RemoteCommand remoteCommand);
        IRemoteProcedureSerializer GetSerializer();
    }
}
