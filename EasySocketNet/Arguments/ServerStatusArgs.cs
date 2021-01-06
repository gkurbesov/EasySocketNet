using EasySocketNet.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocketNet.Arguments
{
    /// <summary>
    /// Server status change
    /// </summary>
    public class ServerStatusArgs : EventArgs
    {
        /// <summary>
        /// New server status
        /// </summary>
        public ServerStatusType Status { get; }
        internal ServerStatusArgs(ServerStatusType serverStatus)
        {
            Status = serverStatus;
        }
    }
}
