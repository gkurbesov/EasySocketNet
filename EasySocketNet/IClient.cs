using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasySocketNet
{
    /// <summary>
    /// Client connected to server
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// identifier
        /// </summary>
        int ClientId { get; }
        /// <summary>
        /// Identifies a network address of client
        /// </summary>
        EndPoint RemoteEndPoint { get; }
    }
}
