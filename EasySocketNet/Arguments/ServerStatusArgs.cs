using EasySocketNet.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocketNet.Arguments
{
    public class ServerStatusArgs : EventArgs
    {
        public ServerStatusType Status { get; }
        internal ServerStatusArgs(ServerStatusType serverStatus)
        {
            Status = serverStatus;
        }
    }
}
