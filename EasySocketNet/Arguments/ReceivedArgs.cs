using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasySocketNet.Arguments
{
    /// <summary>
    /// Receiving data
    /// </summary>
    public class ReceivedArgs : EventArgs
    {
        /// <summary>
        /// The ID of the client who sent the data. Used only in server events
        /// </summary>
        public int ClientId { get; }
        public EndPoint RemoteEndPoint { get; }
        /// <summary>
        /// Received data
        /// </summary>
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
