using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkedService.Transport.Tcp
{
    public static class TcpHelper
    {
        public static void TryConnect(this TcpClient tcpClient, IPEndPoint endpoint, int timeoutRetry = 10)
        {
            do
            {
                try
                {
                    tcpClient.Connect(endpoint);
                    return;
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        --timeoutRetry;
                        continue;
                    }
                }
            } while (timeoutRetry > 0);
        }

        public static byte[] ReadFullPacket(this NetworkStream stream)
        {
            byte[] lengthBuffer = BitConverter.GetBytes(new int());
            stream.Read(lengthBuffer, 0, lengthBuffer.Length);

            var length = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                offset += stream.Read(buffer, offset, length - offset);
                //Console.WriteLine("Client: Received Bytes: {0}/{1}", offset, length);
            }

            return buffer;
        }

        public static void WriteFullPacket(this NetworkStream stream, byte[] buffer)
        {
            byte[] length = BitConverter.GetBytes(buffer.Length);
            stream.Write(length, 0, length.Length);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }
    }
}
