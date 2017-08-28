using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Models
{
    [Serializable]
    public class RemoteCommand
    {
        public RemoteSessionInformation RemoteSessionInformation { get; set; }
        public string InterfaceName { get; set; }
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
    }

    [Serializable]
    public class RemoteResult
    {
        public RemoteSessionInformation RemoteSessionInformation { get; set; }
        public object Result { get; set; }
    }
}
