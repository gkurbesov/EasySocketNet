using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocketNet.Arguments
{
    /// <summary>
    /// Connect or disconnect a server client
    /// </summary>
    public class ConnectionArgs
    {
        /// <summary>
        /// Client id with which the event is associated
        /// </summary>
        public int ClientId { get; }

        internal ConnectionArgs(int clientId)
        {
            ClientId = clientId;
        }
    }
}
