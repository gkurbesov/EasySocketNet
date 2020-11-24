using EasySocketNet.Arguments;
using EasySocketNet.Data;
using System;
using System.Net;

namespace EasySocketNet
{
    public interface ITcpClient : IDisposable
    {
        event EventHandler<ClientStatusArgs> OnChangeStatus;
        event EventHandler<ReceivedArgs> OnReceive;

        object Tag { get; set; }
        int DefaultReceiveBufferSize { get; set; }
        int DefaultSendBufferSize { get; set; }
        ClientStatusType Status { get; }
        EndPoint RemoteEndPoint { get; }


        void Connect(string host, int port);
        void Send(byte[] value);
        void Disconnect();
    }
}
