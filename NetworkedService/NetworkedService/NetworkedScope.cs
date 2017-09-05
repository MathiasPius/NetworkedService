using NetworkedService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService
{
    public class NetworkedScope : INetworkedScope, IDisposable
    {
        private Guid _scopeId { get; set; }
        private List<IRemoteService> _scopedClients;

        public NetworkedScope(IServiceProvider serviceProvider)
        {
            _scopedClients = new List<IRemoteService>();
            _scopeId = new Guid(serviceProvider.GetHashCode(), 0, 0, new byte[8]);

            Console.WriteLine("Client: Networked Scope Created: " + _scopeId);
        }

        public void AddClient(IRemoteService remoteService)
        {
            _scopedClients.Add(remoteService);
        }

        public Guid GetScopeGuid()
        {
            return _scopeId;
        }

        public void Dispose()
        {
            Console.WriteLine("Client: Networked Scope Destroyed: " + _scopeId);

            //foreach (var client in _scopedClients)
                //client.ScopeDestroyed(this);
        }
    }
}
