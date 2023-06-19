using DankWaifu.Tasks;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DankWaifu.Net
{
    public class TcpClientWrapper : IDisposable
    {
        private bool _disposed;
        private bool _keepAlive;
        private SslStream _sslStream;

        private readonly TcpClient _client;

        /// <summary>
        /// Initializes a new instance of a <see cref="TcpClientWrapper"/> instance.
        /// </summary>
        public TcpClientWrapper()
        {
            _client = new TcpClient
            {
                SendBufferSize = 8192,
                ReceiveBufferSize = 8192
            };

            ConnectTimeout = TimeSpan.FromSeconds(30);
            ReceiveTimeout = TimeSpan.FromSeconds(30);
            SendTimeout = TimeSpan.FromSeconds(30);

            KeepAlive = true;
        }

        /// <summary>
        /// Returns the underlying socket of this instances <see cref="TcpClient"/>.
        /// </summary>
        public Socket Socket
        {
            get
            {
                if (_disposed)
                    return null;

                if (!_client.Connected)
                    return null;

                return _client.Client;
            }
        }

        /// <summary>
        /// Gets or sets the amount of time to wait before throwing a
        /// <see cref="TimeoutException"/> when connecting to a host.
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait before throwing a
        /// <see cref="TimeoutException"/> when attempting to receive data.
        /// </summary>
        public TimeSpan ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait before throwing a
        /// <see cref="TimeoutException"/> when attempting to send data.
        /// </summary>
        public TimeSpan SendTimeout { get; set; }

        /// <summary>
        /// Sets the keep alive property of the underlying <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        public bool KeepAlive
        {
            get => _keepAlive;
            set
            {
                ThrowIfDisposed();

                if (_keepAlive == value)
                    return;

                _keepAlive = value;
                _client.Client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.KeepAlive,
                    _keepAlive
                );
            }
        }

        /// <summary>
        /// Returns the amount of data available to read from the server.
        /// </summary>
        public int Available
        {
            get
            {
                if (_disposed)
                    return -1;

                if (!_client.Connected)
                    return -1;

                return _client.Available;
            }
        }

        /// <summary>
        /// Gets or sents the underlying <see cref="TcpClient"/>'s SendBufferSize property.
        /// </summary>
        public int SendBufferSize
        {
            get
            {
                if (_disposed)
                    return -1;

                return _client.SendBufferSize;
            }
            set
            {
                ThrowIfDisposed();

                if (_client.SendBufferSize == value)
                    return;

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"{nameof(value)} must be > 0."
                    );
                }
                _client.SendBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sents the underlying <see cref="TcpClient"/>'s ReceiveBufferSize property.
        /// </summary>
        public int ReceiveBufferSize
        {
            get
            {
                if (_disposed)
                    return -1;

                return _client.SendBufferSize;
            }
            set
            {
                ThrowIfDisposed();

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        $"{nameof(value)} must be > 0."
                    );
                }

                if (_client.ReceiveBufferSize == value)
                    return;

                _client.ReceiveBufferSize = value;
            }
        }

        /// <summary>
        /// Returns whether or not the underlying <see cref="TcpClient"/> is connected.
        /// </summary>
        public bool Connected
        {
            get
            {
                if (_disposed)
                    return false;

                return _client.Connected;
            }
        }

        /// <summary>
        /// Returns the <see cref="NetworkStream"/> of the underlying <see cref="TcpClient"/>.
        /// </summary>
        public Stream NetworkStream
        {
            get
            {
                if (_disposed)
                    return null;

                if (!_client.Connected)
                    return null;

                if (_sslStream != null)
                    return _sslStream;

                return _client.GetStream();
            }
        }

        /// <summary>
        /// Attempts to connect to specified host and port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string host, ushort port)
        {
            ThrowIfDisposed();
            ThrowIfInvalidConnectArgs(host, port);

            await InternalConnectAsync(host, port)
                .ConfigureAwait(false);
        }

        private async Task InternalConnectAsync(string host, ushort port)
        {
            Task connectTask;
            if (ConnectTimeout.Ticks > 0)
            {
                connectTask = _client.ConnectAsync(host, port)
                    .TimeoutAfter(ConnectTimeout);
            }
            else
            {
                connectTask = _client.ConnectAsync(host, port);
            }

            await connectTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to connect to specified host and port with a proxy tunnel.
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectWithProxyAsync(WebProxy proxy, string host, ushort port)
        {
            ThrowIfDisposed();

            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));

            ThrowIfInvalidConnectArgs(host, port);

            await InternalConnectAsync(
                proxy.Address.Host,
                (ushort)proxy.Address.Port
            ).ConfigureAwait(false);

            var sb = new StringBuilder();
            sb.Append($"CONNECT {host}:{port} HTTP/1.1\r\nProxy-Connection: Keep-Alive\r\n");

            if (proxy.Credentials is NetworkCredential creds)
            {
                var proxyCreds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{creds.UserName}:{creds.Password}"));
                sb.Append($"Proxy-Authorization: basic {proxyCreds}\r\n");
            }

            sb.Append("\r\n");

            var connectStr = sb.ToString();
            var connect = Encoding.UTF8.GetBytes(connectStr);

            await SendDataAsync(connect)
                .ConfigureAwait(false);

            var buffer = new byte[4096];
            var cnt = await ReceiveDataAsync(buffer, 0, buffer.Length);
            if (cnt <= 0)
                throw new InvalidOperationException("received <= 0");

            var response = Encoding.UTF8.GetString(buffer, 0, cnt);
            if (string.IsNullOrWhiteSpace(response) ||
                !response.ToLower().Contains("connection established"))
            {
                throw new InvalidOperationException(
                    "Did not receive OK after sending CONNECT to proxy server."
                );
            }
        }

        /// <summary>
        /// Initializes a <see cref="SslStream"/> with the underlying
        /// <see cref="TcpClient"/>'s <see cref="System.Net.Sockets.NetworkStream"/>.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public async Task InitSslStreamAsync(string host)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException(
                    $"{nameof(host)} must not be whitespace or empty string.",
                    nameof(host)
                );
            }

            if (_sslStream != null)
                throw new InvalidOperationException("SSL stream already initialized.");

            _sslStream = new SslStream(_client.GetStream(), false, delegate { return true; });
            await _sslStream.AuthenticateAsClientAsync(host)
                .TimeoutAfter(ConnectTimeout)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Receives any pending data into to da buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public async Task<int> ReceiveDataAsync(
            byte[] buffer,
            int index,
            int cnt)
        {
            return await ReceiveDataAsync(
                buffer,
                index,
                cnt,
                CancellationToken.None
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Receives any pending data into to da buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="cnt"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        public async Task<int> ReceiveDataAsync(
            byte[] buffer,
            int index,
            int cnt,
            CancellationToken cToken)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    $"{nameof(index)} must be >= 0."
                );
            }

            if (cnt < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cnt),
                    $"{nameof(cnt)} must be >= 0."
                );
            }

            Task<int> recvTask;
            if (ReceiveTimeout.Ticks > 0)
            {
                recvTask = NetworkStream.ReadAsync(buffer, index, cnt, cToken)
                    .TimeoutAfter(ReceiveTimeout);
            }
            else
            {
                recvTask = NetworkStream.ReadAsync(buffer, index, cnt, cToken);
            }

            return await recvTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async Task SendDataAsync(byte[] buffer)
        {
            await SendDataAsync(buffer, CancellationToken.None)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SendDataAsync(byte[] buffer, CancellationToken token)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            Task sendTask;
            if (SendTimeout.Ticks > 0)
            {
                sendTask = NetworkStream.WriteAsync(buffer, 0, buffer.Length, token).
                    TimeoutAfter(SendTimeout);
            }
            else
            {
                sendTask = NetworkStream.WriteAsync(buffer, 0, buffer.Length, token);
            }

            await sendTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Disconnects the underlying <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        public void Disconnect()
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            Socket.Disconnect(false);
        }

        private void ThrowIfNotConnected()
        {
            if (!_client.Connected)
                throw new InvalidOperationException("TcpClient is not currently connected.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_sslStream != null)
            {
                try { _sslStream.Close(); }
                catch { /**/ }
            }

            try { _client.Close(); }
            catch { /**/ }

            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }

        private static void ThrowIfInvalidConnectArgs(string host, ushort port)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException(
                    $"{nameof(host)} must not be whitespace or empty string.",
                    nameof(host)
                );
            }

            if (port == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(port),
                    $"{nameof(port)} must be > 0."
                );
            }
        }
    }
}