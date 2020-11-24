using EasySocketNet.Arguments;
using EasySocketNet.Data;
using EasySocketNet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasySocketNet
{
    public class TcpClientAsync : ITcpClientAsync, IDisposable
    {
        public event EventHandler<ClientStatusArgs> OnChangeStatus;

        public object Tag { get; set; } = null;
        public int DefaultReceiveBufferSize { get; set; } = 4096;
        public int DefaultSendBufferSize { get; set; } = 4096;
        public ClientStatusType Status => _connectedStatus;
        public EndPoint RemoteEndPoint => _socket?.RemoteEndPoint ?? null;

        private bool showFail = false;
        private Socket _socket;
        private volatile ClientStatusType _connectedStatus = ClientStatusType.Disconnected;
        private bool _disposedValue = false;

        #region EventCallers
        private void CallChangeStatus()
        {
            Task.Run(() =>
            {
                OnChangeStatus?.Invoke(this, new ClientStatusArgs(_connectedStatus));
            }).ConfigureAwait(false);
        }
        #endregion

        #region interface methods
        public async Task<bool> ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentNullException(nameof(host));
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (_connectedStatus == ClientStatusType.Disconnected)
            {
                try
                {
                    try
                    {
                        _socket?.Dispose();
                    }
                    finally
                    {
                        _socket = null;
                    }

                    using (cancellationToken.Register(() => DisconnectFinalize()))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            ReceiveBufferSize = DefaultReceiveBufferSize,
                            SendBufferSize = DefaultSendBufferSize,
                        };

                        _connectedStatus = ClientStatusType.Connection;
                        CallChangeStatus();

                        await Task.WhenAny(
                            _socket.ConnectAsync(host, port),
                            Task.Delay(10000, cancellationToken))
                            .ConfigureAwait(false);


                        if (_socket.Connected)
                        {
                            _connectedStatus = ClientStatusType.Connection;
                            CallChangeStatus();
                            return true;
                        }
                        else
                        {
                            DisconnectFinalize();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ConnectAsync Exception");
                    if (showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    DisconnectFinalize();
                }
            }
            return false;
        }

        public async Task<bool> SendAsync(byte[] value)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentNullException(nameof(value));

            if (_socket != null && _socket.Connected)
            {
                try
                {
                    var result = await _socket.SendAsync(new ArraySegment<byte>(value), SocketFlags.None)
                        .ConfigureAwait(false);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SendAsync Exception");
                    if (showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    DisconnectFinalize();
                }
            }
            return false;
        }

        public async Task<byte[]> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_socket == null || !_socket.Connected)
                return null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var collector = new BufferCollector();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var buffer = new byte[DefaultReceiveBufferSize];
                    var readSize = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None).ConfigureAwait(false);
                    if (readSize > 0)
                    {
                        collector.Append(buffer, readSize);
                        if (_socket.Available <= 0)
                            break;
                    }
                    else
                    {
                        DisconnectFinalize();
                        return new byte[0];
                    }

                }
                return collector.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReceiveAsync Exception");
                if (showFail) Debug.Fail(ex.Message, ex.StackTrace);
                DisconnectFinalize();
                return new byte[0];
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_connectedStatus != ClientStatusType.Disconnection && _connectedStatus != ClientStatusType.Disconnected)
                {
                    _connectedStatus = ClientStatusType.Disconnection;
                    CallChangeStatus();
                    if (_socket != null)
                    {
                        LingerOption lingerOption = new LingerOption(true, 1);
                        _socket.LingerState = lingerOption;
                        if (_socket.Connected)
                        {
                            _socket?.Shutdown(SocketShutdown.Both);
                            await Task.Factory.FromAsync(_socket.BeginDisconnect, _socket.EndDisconnect, false, null).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DisconnectAsync Exception");
                if (showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
            finally
            {
                DisconnectFinalize();
            }
        }

        private void DisconnectFinalize()
        {
            try
            {
                _socket?.Dispose();
            }
            finally
            {
                _socket = null;
            }
            if (_connectedStatus != ClientStatusType.Disconnected)
            {
                _connectedStatus = ClientStatusType.Disconnected;
                CallChangeStatus();
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
                    Tag = null;
                    _connectedStatus = ClientStatusType.Disconnected;
                }

                try
                {
                    _socket?.Dispose();
                }
                finally
                {
                    _socket = null;
                }

                _disposedValue = true;
            }
        }

        ~TcpClientAsync()
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
