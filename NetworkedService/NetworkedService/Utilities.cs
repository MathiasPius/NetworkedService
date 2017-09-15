using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkedService
{
    public static class Utilities
    {
        public static IPAddress Resolve(string hostname, AddressFamily addressFamily = AddressFamily.InterNetwork)
        {
            var hosts = Dns.GetHostEntry(hostname);
            return hosts.AddressList.FirstOrDefault(i => addressFamily == AddressFamily.Unspecified || i.AddressFamily == addressFamily);
        }
    }
}
