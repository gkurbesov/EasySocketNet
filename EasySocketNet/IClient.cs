using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasySocketNet
{
    public interface IClient
    {
        int ClientId { get; }
        EndPoint RemoteEndPoint { get; }
    }
}
