using EasySocketNet.Arguments;
using System;
using System.Net;

namespace EasySocketNet
{
    public interface ITcpClient : IDisposable
    {
        event EventHandler<ClientStatusArgs> OnChangeStatus;
        event EventHandler<ReceivedArgs> OnReceived;

        object Tag { get; set; }
        int DefaultReceiveBufferSize { get; set; }
        int DefaultSendBufferSize { get; set; }
        bool NoDelay { get; set; }
        bool DualMode { get; set; }
        int ReceiveBufferSize { get; set; }
        int SendBufferSize { get; set; }
        bool ReuseAddress { get; set; }
        ClientStatusType Status { get; }
        EndPoint RemoteEndPoint { get; }


        void Connect(string host, int port);
        void Send(byte[] value);
        void Disconnect();
    }
}
