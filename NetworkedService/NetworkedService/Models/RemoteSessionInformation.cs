using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Models
{
    [Serializable]
    public struct RemoteSessionInformation
    {
        public Guid InstanceId { get; set; }
        public Guid ScopeId { get; set; }
        public Guid ActionId { get; set; }
    }
}
