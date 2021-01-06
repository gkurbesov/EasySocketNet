using EasySocketNet.Arguments;
using EasySocketNet.Data;
using System;
using System.Net;

namespace EasySocketNet
{
    public interface ITcpClient : IDisposable
    {
        /// <summary>
        /// Client status change event
        /// </summary>
        event EventHandler<ClientStatusArgs> OnChangeStatus;
        /// <summary>
        /// Data receive Event
        /// </summary>
        event EventHandler<ReceivedArgs> OnReceive;

        /// <summary>
        /// Gets or sets an object that contains information about the control.
        /// </summary>
        object Tag { get; set; }
        int DefaultReceiveBufferSize { get; set; }
        int DefaultSendBufferSize { get; set; }
        int DefaultReceiveTimeout { get; set; }
        int DefaultSendTimeout { get; set; }
        /// <summary>
        /// Client connection status
        /// </summary>
        ClientStatusType Status { get; }
        /// <summary>
        /// Client to server connection point
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        void Connect(string host, int port);
        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="value"></param>
        void Send(byte[] value);
        /// <summary>
        /// Disconnect from server
        /// </summary>
        void Disconnect();
    }
}
