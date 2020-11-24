using EasySocketNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasySocketNet.Data
{
    public class ClientContainer : IClient
    {
        public int ClientId { get; private set; } = 0;
        public EndPoint RemoteEndPoint => socket?.RemoteEndPoint ?? null;
        internal Socket socket { get; set; } = null;
        internal byte[] ReadBuffer { get; set; } = new byte[0];
        private BufferCollector collector { get; set; } = new BufferCollector();

        internal ClientContainer() { }

        internal ClientContainer SetSocket(Socket value)
        {
            ClientId = value.Handle.ToInt32();
            socket = value;
            return this;
        }

        internal ClientContainer SetBufferSize(int value)
        {
            ReadBuffer = new byte[value];
            return this;
        }

        internal ClientContainer FlushBuffer(int size)
        {
            collector.Append(ReadBuffer, size);
            return this;
        }
        internal byte[] GetBuffer() => 
            collector.Data.ToArray();

        internal void ClearBuffer()
        {
            collector.Clear();
        }
    }
}
