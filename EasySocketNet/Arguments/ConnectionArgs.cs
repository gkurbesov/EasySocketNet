using System;
using System.Collections.Generic;
using System.Text;

namespace EasySocketNet.Arguments
{
    public class ConnectionArgs
    {
        public int ClientId { get; }

        internal ConnectionArgs(int clientId)
        {
            ClientId = clientId;
        }
    }
}
