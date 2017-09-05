using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Interfaces
{
    public interface IRemoteService
    {
        void ScopeDestroyed(INetworkedScope networkedScope);
    }
}
