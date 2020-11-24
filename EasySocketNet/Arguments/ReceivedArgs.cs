using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasySocketNet.Arguments
{
    public class ReceivedArgs : EventArgs
    {
        public int ClientId { get; }
        public EndPoint RemoteEndPoint { get; }
        public byte[] Data { get; }

        internal ReceivedArgs(EndPoint endPoint, byte[] data)
        {
            ClientId = -1;
            RemoteEndPoint = endPoint;
            Data = data;
        }

        internal ReceivedArgs(int clientId, EndPoint endPoint, byte[] data)
        {
            ClientId = clientId;
            RemoteEndPoint = endPoint;
            Data = data;
        }
    }
}
