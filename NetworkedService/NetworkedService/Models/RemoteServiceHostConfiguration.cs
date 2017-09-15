using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Models
{
    public class RemoteServiceHostConfiguration
    {
        private readonly RemoteServiceHost _remoteServiceHost;

        internal RemoteServiceHostConfiguration(RemoteServiceHost remoteServiceHost)
        {
            _remoteServiceHost = remoteServiceHost;
        }

        public RemoteServiceHostConfiguration Expose<TInterface>()
            where TInterface: class
        {
            _remoteServiceHost.ExposeInterface<TInterface>();
            return this;
        }

        public RemoteServiceHost GetServer()
        {
            return _remoteServiceHost;
        }
    }
}
