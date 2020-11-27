using EasySocketNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasySocketNet.Data
{
    public class ClientContainer : IClient, IDisposable
    {

        public int ClientId { get; private set; } = 0;
        public EndPoint RemoteEndPoint => socket?.RemoteEndPoint ?? null;
        internal Socket socket { get; set; } = null;
        internal byte[] ReadBuffer { get; set; } = new byte[0];
        private BufferCollector collector { get; set; } = new BufferCollector();
        private bool _disposedValue = false;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ClientId = -1;
                    ReadBuffer = new byte[0];
                    collector.Clear();
                }
                try
                {
                    socket?.Dispose();
                }
                finally
                {
                    socket = null;
                    collector = null;
                    _disposedValue = true;
                }
            }
        }
        
        ~ClientContainer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
