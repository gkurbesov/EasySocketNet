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
using System.Threading.Tasks;

namespace EasySocketNet
{
    public class TcpClient : ITcpClient, IDisposable
    {

        public event EventHandler<ClientStatusArgs> OnChangeStatus;
        public event EventHandler<ReceivedArgs> OnReceive;

        public object Tag { get; set; } = null;
        public int DefaultReceiveBufferSize { get; set; } = 4096;
        public int DefaultSendBufferSize { get; set; } = 4096;
        public ClientStatusType Status => _connectedStatus;
        public EndPoint RemoteEndPoint => _socket?.RemoteEndPoint ?? null;

        private bool _showFail = false;
        private Socket _socket;
        private volatile ClientStatusType _connectedStatus = ClientStatusType.Disconnected;
        private byte[] _buffer { get; set; } = new byte[4096];
        private BufferCollector _bufferCollector { get; set; } = new BufferCollector();
        private bool _disposedValue = false;

        #region EventCallers
        private void CallReceive(byte[] value)
        {
            try
            {
                OnReceive?.Invoke(this, new ReceivedArgs(RemoteEndPoint, value));
            }
            catch(Exception ex)
            {
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
        }
        private void CallChangeStatus()
        {
            try
            {
                OnChangeStatus?.Invoke(this, new ClientStatusArgs(_connectedStatus));
            }
            catch (Exception ex)
            {
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region implemetation

        public void Connect(string host, int port)
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

                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        ReceiveBufferSize = DefaultReceiveBufferSize,
                        SendBufferSize = DefaultSendBufferSize,
                    };

                    _connectedStatus = ClientStatusType.Connection;
                    CallChangeStatus();

                    var result = _socket.BeginConnect(host, port, new AsyncCallback(ConnectCallback), null);
                    var success = result.AsyncWaitHandle.WaitOne(10000, true);
                    if (_socket != null && !_socket.Connected)
                    {
                        Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    DisconnectFinalize();
                }
            }
        }

        public void Disconnect()
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
                            _socket?.BeginDisconnect(false, DisconnectCallback, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Disconnect Exception");
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
            finally
            {
                DisconnectFinalize();
            }
        }

        public void Send(byte[] value)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentNullException(nameof(value));

            if (_socket != null && _socket.Connected)
            {
                try
                {
                    _socket?.BeginSend(value, 0, value.Length, SocketFlags.None,
                        new AsyncCallback(SendCallback), null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Send Exception");
                    if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                    Disconnect();
                }
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
                _buffer = new byte[0];
                _bufferCollector.Clear();
            }
            if (_connectedStatus != ClientStatusType.Disconnected)
            {
                _connectedStatus = ClientStatusType.Disconnected;
                CallChangeStatus();
            }
        }
        #endregion


        #region callbacks
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                if (_socket != null)
                {
                    _socket.EndConnect(result);
                    _buffer = new byte[DefaultReceiveBufferSize];
                    _connectedStatus = ClientStatusType.Connected;
                    CallChangeStatus();
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ConnectCallback Exception");
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                Disconnect();
            }
        }
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    int readSize = _socket.EndReceive(result);
                    if (readSize > 0)
                    {
                        _bufferCollector.Append(_buffer, readSize);
                        if (_socket.Available <= 0)
                        {
                            CallReceive(_bufferCollector.Data.ToArray());
                            _bufferCollector.Clear();
                        }
                        _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None,
                            new AsyncCallback(ReceiveCallback), null);
                    }
                    else
                    {
                        Debug.WriteLine("ReceiveCallback: read size = " + readSize.ToString());
                        DisconnectFinalize();
                    }
                }
                else
                {
                    Debug.WriteLine("ReceiveCallback: _socket not connected");
                    DisconnectFinalize();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReceiveCallback Exception");
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                DisconnectFinalize();
            }
        }
        private void SendCallback(IAsyncResult result)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                    _socket.EndSend(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SendCallback Exception");
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
                Disconnect();
            }
        }
        private void DisconnectCallback(IAsyncResult ar)
        {
            try
            {
                _socket?.EndDisconnect(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DisconnectCallback Exception");
                if (_showFail) Debug.Fail(ex.Message, ex.StackTrace);
            }
            finally
            {
                DisconnectFinalize();
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

        ~TcpClient()
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
