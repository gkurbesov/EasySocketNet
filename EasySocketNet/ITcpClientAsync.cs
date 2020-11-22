using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasySocketNet
{
    public interface ITcpClientAsync : ITcpClient, IDisposable
    {
        Task<bool> ConnectAsync(string host, int port);
        Task<bool> SendAsync(byte[] value);
        Task<byte[]> ReceiveAsync();
    }
}
