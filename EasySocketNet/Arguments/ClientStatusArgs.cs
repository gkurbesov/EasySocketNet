using EasySocketNet.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocketNet.Arguments
{
    /// <summary>
    /// Change of client status
    /// </summary>
    public class ClientStatusArgs : EventArgs
    {
        /// <summary>
        /// New client status
        /// </summary>
        public ClientStatusType Status { get; }
        internal ClientStatusArgs(ClientStatusType clientStatus)
        {
            Status = clientStatus;
        }
    }
}
