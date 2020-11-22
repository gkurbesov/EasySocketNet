using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocketNet.Arguments
{
    public class ClientStatusArgs : EventArgs
    {
        public ClientStatusType Status { get; }
        internal ClientStatusArgs(ClientStatusType clientStatus)
        {
            Status = clientStatus;
        }
    }
}
