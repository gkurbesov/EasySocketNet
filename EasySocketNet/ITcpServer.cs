using System;
using System.Collections.Generic;
using System.Text;
using EasySocketNet.Arguments;
using EasySocketNet.Data;

namespace EasySocketNet
{
    public interface ITcpServer : IDisposable
    {
        event EventHandler<ServerStatusArgs> OnChangeStatus;
        event EventHandler<ReceivedArgs> OnReceive;
        event EventHandler<ConnectionArgs> OnClientConnect;
        event EventHandler<ConnectionArgs> OnClientDisconnect;

        object Tag { get; set; }
        int Online { get; }
        ServerStatusType Status { get; }
        int DefaultReceiveBufferSize { get; set; }
        int DefaultSendBufferSize { get; set; }

        void Start(int port);
        void Stop();
        void Send(int clientId, byte[] value);
        void Kick(int clientId);

        IClient GetClient(int clientId);
        IEnumerable<IClient> GetClients();
    }
}
