using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService
{
    public class RemoteException : InvalidOperationException
    {
        public RemoteException()
        {
        }

        public RemoteException(string message) 
            : base(message)
        {
        }

        public RemoteException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
