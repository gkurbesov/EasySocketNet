using EasySocketNet.Arguments;
using EasySocketNet.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasySocketNet
{
    public class TcpServer : ITcpServer, IDisposable
    {
        public event EventHandler<ServerStatusArgs> OnChangeStatus;
        public event EventHandler<ReceivedArgs> OnReceive;
        public event EventHandler<ConnectionArgs> OnClientConnect;
        public event EventHandler<ConnectionArgs> OnClientDisconnect;

        public object Tag { get; set; } = null;
        public int Online => _clients.Count;
        public ServerStatusType Status => _listenningStatus;
        public int DefaultReceiveBufferSize { get; set; } = 4096;
        public int DefaultSendBufferSize { get; set; } = 4096;


        private bool _showFail = true;
        private Socket _soket = null;
        private bool _disposedValue = false;
        private volatile ServerStatusType _listenningStatus = ServerStatusType.None;
        private List<ClientContainer> _clients = new List<ClientContainer>();
        private object syncObjectClients = new object();

        #region EventCallers
        private void CallClientConnect(int clientId)
        {
            try
            {
                OnClientConnect?.Invoke(this, new ConnectionArgs(clientId));
            }
            catch(Exception ex)
            {
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
        }

        private void CallClientDisconnect(int clientId)
        {
            try
            {
                OnClientDisconnect?.Invoke(this, new ConnectionArgs(clientId));
            }
            catch (Exception ex)
            {
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
        }

        private void CallReceive(ClientContainer client, byte[] value)
        {
            try
            {
                OnReceive?.Invoke(this, new ReceivedArgs(client.ClientId, client.RemoteEndPoint, value));
            }
            catch (Exception ex)
            {
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
        }

        private void CallChangeStatus()
        {
            try
            {
                OnChangeStatus?.Invoke(this, new ServerStatusArgs(_listenningStatus));
            }
            catch (Exception ex)
            {
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
        }
        #endregion


        #region implemetation 
        public void Start(int port)
        {
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (_listenningStatus == ServerStatusType.None)
            {
                try
                {
                    _soket?.Dispose();
                }
                finally
                {
                    _soket = null;
                }
                _soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveBufferSize = DefaultReceiveBufferSize,
                    SendBufferSize = DefaultSendBufferSize
                };

                try
                {
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
                    _soket.Bind(localEndPoint);
                    _soket.Listen((int)SocketOptionName.MaxConnections);
                    _soket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                    _listenningStatus = ServerStatusType.Listening;
                }
                catch (Exception ex)
                {
                    if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    _listenningStatus = ServerStatusType.PortError;
                }
                finally
                {
                    CallChangeStatus();
                }

            }
        }

        public void Stop()
        {
            if (_listenningStatus != ServerStatusType.None)
            {
                if (_soket != null)
                {
                    try
                    {
                        _soket.Close();
                    }
                    finally
                    {
                        foreach (var client in _clients.ToArray())
                            ClientDisconnect(client);
                    }
                }
                StopFinalize();
            }
        }

        public void Send(int clientId, byte[] value)
        {
            if (GetClient(clientId) is ClientContainer client)
            {
                try
                {
                    var socket = client.socket;
                    if (socket != null && socket.Connected)
                    {
                        socket.BeginSend(value, 0, value.Length, SocketFlags.None,
                          new AsyncCallback(SendCallback), client);
                    }
                }
                catch (Exception ex)
                {
                    if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    ClientDisconnect(client);
                }
            }
        }

        public void Kick(int clientId)
        {
            ClientDisconnect(clientId);
        }

        public IClient GetClient(int clientId)
        {
            lock (syncObjectClients)
            {
                return _clients.FirstOrDefault(o => o.ClientId == clientId);
            }
        }

        public IEnumerable<IClient> GetClients()
        {
            lock (syncObjectClients)
            {
                return _clients.ToArray();
            }
        }

        internal void StopFinalize()
        {
            lock (syncObjectClients) _clients.Clear();
            try
            {
                _soket?.Dispose();
            }
            finally
            {
                _soket = null;
                if (_listenningStatus != ServerStatusType.None)
                {
                    _listenningStatus = ServerStatusType.None;
                    CallChangeStatus();
                }
            }
        }

        internal void ClientDisconnect(int clientId)
        {
            var client = GetClient(clientId);
            if (client != null)
                ClientDisconnect((ClientContainer)client);
        }

        internal void ClientDisconnect(ClientContainer client)
        {
            if (client != null)
            {
                lock (syncObjectClients) _clients.Remove(client);
                var socket = client.socket;
                if (socket != null)
                {
                    LingerOption lingerOption = new LingerOption(true, 1);
                    socket.LingerState = lingerOption;
                    try
                    {
                        if (socket.Connected)
                        {
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    }
                }
                if (client.ClientId > 0)
                    CallClientDisconnect(client.ClientId);
                client.Dispose();
            }
        }

        #endregion

        #region callback
        private void AcceptCallback(IAsyncResult result)
        {
            if (_listenningStatus == ServerStatusType.Listening)
            {
                var client = new ClientContainer();
                try
                {
                    var socket = _soket.EndAccept(result);
                    socket.ReceiveBufferSize = DefaultReceiveBufferSize;
                    socket.SendBufferSize = DefaultSendBufferSize;

                    client.SetSocket(socket)
                        .SetBufferSize(DefaultReceiveBufferSize);

                    lock (syncObjectClients) _clients.Add(client);

                    socket.BeginReceive(client.ReadBuffer,
                          0, client.ReadBuffer.Length, SocketFlags.None,
                          new AsyncCallback(ReceiveCallback),
                          client);

                    CallClientConnect(client.ClientId);
                }
                catch (Exception ex)
                {
                    if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    ClientDisconnect(client);
                }
                finally
                {
                    try
                    {
                        if (_soket != null && _soket.IsBound)
                            _soket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                    }
                    catch (Exception ex)
                    {
                        if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            if (result.AsyncState is ClientContainer client && GetClient(client.ClientId) != null)
            {
                var socket = client.socket;

                if (socket != null && socket.Connected)
                {
                    try
                    {
                        int readSize = socket.EndReceive(result);
                        if (readSize > 0)
                        {
                            client.FlushBuffer(readSize);
                            if (socket.Available <= 0)
                            {
                                CallReceive(client, client.GetBuffer());
                                client.ClearBuffer();
                            }
                            socket.BeginReceive(client.ReadBuffer, 0,
                                client.ReadBuffer.Length, SocketFlags.None,
                                new AsyncCallback(ReceiveCallback),
                                client);
                        }
                        else
                        {
                            ClientDisconnect(client);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                        ClientDisconnect(client);
                    }
                }
                else
                {
                    ClientDisconnect(client);
                }
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            if (result.AsyncState is ClientContainer client && GetClient(client.ClientId) != null)
            {
                var socket = client.socket;
                try
                {
                    if (socket != null && socket.Connected)
                        socket.EndSend(result);
                    else
                        ClientDisconnect(client);
                }
                catch (Exception ex)
                {
                    if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    ClientDisconnect(client);
                }
            }
        }
        #endregion

        #region dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                _disposedValue = true;
            }
        }

        ~TcpServer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}
