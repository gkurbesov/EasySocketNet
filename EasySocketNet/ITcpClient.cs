using EasySocketNet.Arguments;
using System;
using System.Net;

namespace EasySocketNet
{
    public interface ITcpClient : IDisposable
    {
        event EventHandler OnConnected;
        event EventHandler OnDisconnected;
        event EventHandler<ReceivedArgs> OnReceived;

        object Tag { get; set; }
        bool NoDelay { get; set; }
        bool DualMode { get; set; }
        int ReceiveBufferSize { get; set; }
        int SendBufferSize { get; set; }
        bool ReuseAddress { get; set; }
        bool Connected { get; }
        EndPoint RemoteEndPoint { get; }


        void Connect(string host, int port);
        void Send(byte[] value);
        void Disconnect();
    }
}
