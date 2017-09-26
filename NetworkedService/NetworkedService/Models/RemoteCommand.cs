using System;
using System.Collections.Generic;
using System.Text;
using NetworkedService.Models;

namespace NetworkedService.Models
{
    [Serializable]
    public class RemoteCommand
    {
        public RemoteSessionInformation RemoteSessionInformation { get; set; }
        public RemoteProcedureDescriptor RemoteProcedureDescriptor { get; set; }
        //public InterfaceHash TargetInterface { get; set; }
        public object[] Parameters { get; set; }
    }

    [Serializable]
    public class RemoteResult
    {
        public RemoteSessionInformation RemoteSessionInformation { get; set; }
        public object Exception { get; set; }
        public object Result { get; set; }
    }
}
