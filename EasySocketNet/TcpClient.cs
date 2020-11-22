using EasySocketNet.Arguments;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasySocketNet
{
    public class TcpClient : ITcpClient
    {
        public object Tag { get; set; }
        public bool NoDelay { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool DualMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ReceiveBufferSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SendBufferSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool ReuseAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal Socket _socket;
        public bool Connected => throw new NotImplementedException();
        public EndPoint RemoteEndPoint => throw new NotImplementedException();

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<ReceivedArgs> OnReceived;

        public void Connect(string host, int port)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] value)
        {
            throw new NotImplementedException();
        }
    }
}
