using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Interfaces
{
    public interface INetworkedScope
    {
        Guid GetScopeGuid();
        void AddClient(IRemoteService remoteService);
    }
}
