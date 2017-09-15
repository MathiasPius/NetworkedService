using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkedService.Transport.Tcp
{
    public static class TcpHelper
    {
        internal static byte[] ReadFullPacket(this Socket stream)
        {
            byte[] lengthBuffer = BitConverter.GetBytes(new int());
            stream.Receive(lengthBuffer, lengthBuffer.Length, SocketFlags.None);

            var length = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[length];
            int offset = 0;

            while (offset < length)
                offset += stream.Receive(buffer, offset, length - offset, SocketFlags.None);

            return buffer;
        }

        internal static void WriteFullPacket(this Socket stream, byte[] buffer)
        {
            byte[] length = BitConverter.GetBytes(buffer.Length);
            stream.Send(length, length.Length, SocketFlags.None);
            stream.Send(buffer, buffer.Length, SocketFlags.None);
        }
    }
}
