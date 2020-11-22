using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasySocketNet.Arguments
{
    public class ReceivedArgs : EventArgs
    {
        public EndPoint RemoteEndPoint { get; }
        public byte[] Data { get; }

        internal ReceivedArgs(EndPoint endPoint, byte[] data)
        {
            RemoteEndPoint = endPoint;
            Data = data;
        }
    }
}
