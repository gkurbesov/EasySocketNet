using EasySocketNet.Arguments;
using EasySocketNet.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasySocketNet
{
    public interface ITcpClientAsync : IDisposable
    {
        event EventHandler<ClientStatusArgs> OnChangeStatus;


        object Tag { get; set; }
        int DefaultReceiveBufferSize { get; set; }
        int DefaultSendBufferSize { get; set; }
        int DefaultReceiveTimeout { get; set; }
        int DefaultSendTimeout { get; set; }
        ClientStatusType Status { get; }
        EndPoint RemoteEndPoint { get; }

        Task<bool> ConnectAsync(string host, int port, CancellationToken cancellationToken);
        Task<bool> SendAsync(byte[] value);
        Task<byte[]> ReceiveAsync(CancellationToken cancellationToken);
        Task DisconnectAsync();
    }
}
