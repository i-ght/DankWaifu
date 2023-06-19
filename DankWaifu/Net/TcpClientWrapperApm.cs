using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;

namespace DankWaifu.Net
{
    public class TcpClientWrapperApm : IDisposable
    {
        private readonly TcpClient _client;
        private SslStream _sslStream;
        private bool _disposed;

        public event EventHandler ConnectionTimedOutEventHandler;

        public event EventHandler WriteOperationTimedOutEventHandler;

        public event EventHandler ReadOperationTimedOutEventHandler;

        public event EventHandler SSLAuthenticateAsClientTimedOutEventHandler;

        public TcpClientWrapperApm()
        {
            _client = new TcpClient
            {
                ReceiveTimeout = 60000,
                SendTimeout = 60000,
                SendBufferSize = 8192,
                ReceiveBufferSize = 8192
            };
            ConnectTimeout = 60000;
            ReadTimeout = 60000;
            WriteTimeout = 60000;
            SSLAuthenticateTimeout = 60000;
            PassThisAsStateByDefault = true;
        }

        public int ConnectTimeout { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public int SSLAuthenticateTimeout { get; set; }
        public object Obj { get; set; }
        public bool PassThisAsStateByDefault { get; set; }
        public Socket Socket => _client.Client;

        public int Available
        {
            get
            {
                if (_disposed)
                    return -1;

                return _client.Available;
            }
        }

        public bool Connected
        {
            get
            {
                if (_disposed)
                    return false;

                return _client.Connected;
            }
        }

        public Stream NetworkStream
        {
            get
            {
                if (_disposed)
                    return null;

                if (_sslStream != null)
                    return _sslStream;

                return _client.GetStream();
            }
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback callback, object state = null)
        {
            ThrowIfDisposed();

            if (state == null && PassThisAsStateByDefault)
                state = this;

            var result = _client.BeginConnect(host, port, callback, state);

            if (ConnectTimeout <= 0)
                return result;

            var waitHandleState = new WaitHandlesState(result.AsyncWaitHandle);
            waitHandleState.RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, ConnectionTimedout, waitHandleState, ConnectTimeout, true);

            return result;
        }

        public void EndConnect(IAsyncResult ar)
        {
            ThrowIfDisposed();
            _client.EndConnect(ar);
        }

        public IAsyncResult BeginRead(byte[] buffer, int index, int len, AsyncCallback callback, object state = null)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            if (state == null && PassThisAsStateByDefault)
                state = this;

            var result = NetworkStream.BeginRead(buffer, index, len, callback, state);

            if (ReadTimeout <= 0)
                return result;

            var waitHandleState = new WaitHandlesState(result.AsyncWaitHandle);
            waitHandleState.RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, ReadTimedout, waitHandleState, ConnectTimeout, true);

            return result;
        }

        public int EndRead(IAsyncResult ar)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            return NetworkStream.EndRead(ar);
        }

        public IAsyncResult BeginWrite(byte[] buffer, AsyncCallback callback, object state = null)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            if (state == null && PassThisAsStateByDefault)
                state = this;

            var result = NetworkStream.BeginWrite(buffer, 0, buffer.Length, callback, state);

            if (WriteTimeout <= 0)
                return result;

            var waitHandleState = new WaitHandlesState(result.AsyncWaitHandle);
            waitHandleState.RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, WriteTimedout, waitHandleState, ConnectTimeout, true);

            return result;
        }

        public void EndWrite(IAsyncResult ar)
        {
            ThrowIfDisposed();
            ThrowIfNotConnected();

            NetworkStream.EndWrite(ar);
        }

        private void WriteTimedout(object state, bool timedout)
        {
            var stateObj = (WaitHandlesState)state;
            while (stateObj.RegisteredWaitHandle == null)
                Thread.Sleep(100);

            stateObj.RegisteredWaitHandle.Unregister(stateObj.AsyncWaitHandle);

            if (timedout)
                WriteOperationTimedOutEventHandler?.Invoke(this, null);
        }

        private void ReadTimedout(object state, bool timedout)
        {
            var stateObj = (WaitHandlesState)state;
            while (stateObj.RegisteredWaitHandle == null)
                Thread.Sleep(100);

            stateObj.RegisteredWaitHandle.Unregister(stateObj.AsyncWaitHandle);

            if (timedout)
                ReadOperationTimedOutEventHandler?.Invoke(this, null);
        }

        private void ConnectionTimedout(object state, bool timedout)
        {
            var stateObj = (WaitHandlesState)state;
            while (stateObj.RegisteredWaitHandle == null)
                Thread.Sleep(100);

            stateObj.RegisteredWaitHandle.Unregister(stateObj.AsyncWaitHandle);

            if (timedout)
                ConnectionTimedOutEventHandler?.Invoke(this, null);
        }

        private void OnSSLAuthenticationTimedout(object state, bool timedout)
        {
            var stateObj = (WaitHandlesState)state;
            while (stateObj.RegisteredWaitHandle == null)
                Thread.Sleep(100);

            stateObj.RegisteredWaitHandle.Unregister(stateObj.AsyncWaitHandle);

            if (timedout)
                SSLAuthenticateAsClientTimedOutEventHandler?.Invoke(this, null);
        }

        /// <summary>
        /// Initializes the SSL stream
        /// </summary>
        /// <param name="host"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IAsyncResult SSLBeginAuthenticateAsClient(string host, AsyncCallback callback)
        {
            ThrowIfNotConnected();

            if (_sslStream != null)
                throw new InvalidOperationException("SSL stream already initialized");

            _sslStream = new SslStream(_client.GetStream(), false, delegate { return true; });
            var result = _sslStream.BeginAuthenticateAsClient(host, callback, this);

            var waitHandleState = new WaitHandlesState(result.AsyncWaitHandle);
            waitHandleState.RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, OnSSLAuthenticationTimedout, waitHandleState, ConnectTimeout, true);

            return result;
        }

        public void SSLAuthenticateAsClient(string host)
        {
            _sslStream = new SslStream(_client.GetStream(), false, delegate { return true; });
            _sslStream.AuthenticateAsClient(host);
        }

        public void SSLEndAuthenticateAsClient(IAsyncResult ar)
        {
            _sslStream.EndAuthenticateAsClient(ar);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (!disposing)
                return;

            try { _sslStream?.Dispose(); }
            catch { /*ignored*/ }

            try { _client?.Close(); }
            catch { /*ignored*/ }

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void ThrowIfNotConnected()
        {
            if (!_client.Connected)
                throw new InvalidOperationException("TcpClient is not connected");
        }

        ~TcpClientWrapperApm()
        {
            Dispose(false);
        }
    }
}