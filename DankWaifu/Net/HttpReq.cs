using System;
using System.Net;
using System.Text;

namespace DankWaifu.Net
{
    public class HttpReq
    {
        private TimeSpan _timeout;
        private string _contentBody;

        public HttpReq()
        {
            _contentBody = string.Empty;
            ProtocolVersion = HttpVersion.Version11;
            Accept = string.Empty;
            Origin = string.Empty;
            Referer = string.Empty;
            ContentType = string.Empty;
            ContentBody = string.Empty;
            ContentLength = -1;
            OverrideUserAgent = string.Empty;
            AcceptEncoding = DecompressionMethods.None;
            FollowRedirect = true;
            _timeout = TimeSpan.FromSeconds(-1);
            KeepAlive = true;
            AdditionalHeaders = new WebHeaderCollection();
            OverrideHeaders = new WebHeaderCollection();
        }

        public HttpReq(HttpMethod httpMethod, string url) : this()
        {
            if (httpMethod == HttpMethod.NONE)
                throw new ArgumentException("Invalid HttpMethod");

            HttpMethod = httpMethod;
            Uri = new Uri(url);
        }

        public HttpReq(HttpMethod httpMethod, Uri uri) : this()
        {
            if (httpMethod == HttpMethod.NONE)
                throw new ArgumentException("Invalid HttpMethod");

            HttpMethod = httpMethod;
            Uri = uri;
        }

        /// <summary>
        /// HTTP method
        /// </summary>
        public HttpMethod HttpMethod { get; }

        /// <summary>
        /// URI to send this request to.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// HTTP protocol version
        /// </summary>
        public Version ProtocolVersion { get; set; }

        /// <summary>
        /// HTTP Accept header
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// HTTP Origin header
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// HTTP Referer header
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// HTTP Content-Type header
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Content body of the HTTP request
        /// </summary>
        public byte[] ContentData { get; set; }

        /// <summary>
        /// If this property is set, then it overrides the client sending this request's user-agent header.
        /// </summary>
        public string OverrideUserAgent { get; set; }

        /// <summary>
        /// HTTP Accept-Encoding header
        /// </summary>
        public DecompressionMethods AcceptEncoding { get; set; }

        /// <summary>
        /// Follow redirect status codes
        /// </summary>
        public bool FollowRedirect { get; set; }

        /// <summary>
        /// Request times out after milliseconds
        /// </summary>
        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                if (value.TotalMilliseconds <= 0)
                {
                    throw new ArgumentOutOfRangeException
                    (nameof(value), "Timeout value must be > than 0."
                    );
                }

                _timeout = value;
            }
        }

        /// <summary>
        /// HTTP Content-Length header
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>
        /// HTTP Connection: keep-alive header
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// Additional headers to send with this HTTP request
        /// </summary>
        public WebHeaderCollection AdditionalHeaders { get; set; }

        /// <summary>
        /// If this property is set then it overrides the client sending this request's default headers.
        /// </summary>
        public WebHeaderCollection OverrideHeaders { get; set; }

        /// <summary>
        /// If this property is set then it overrides the client sending this request's proxy.
        /// </summary>
        public WebProxy OverrideProxy { get; set; }

        /// <summary>
        /// Throw if a WebException occurs instead of returning its response.
        /// </summary>
        public bool ThrowOnWebEx { get; set; }

        /// <summary>
        /// Throw if a TimeoutException or a OperationCanceledException exception occurs.
        /// </summary>
        public bool ThrowOnTimeoutEx { get; set; }

        /// <summary>
        /// Throw if an IOException occurs.
        /// </summary>
        public bool ThrowOnIOEx { get; set; }

        /// <summary>
        /// Content body of the HTTP request in string format
        /// </summary>
        public string ContentBody
        {
            get => _contentBody;
            set
            {
                var val = value ?? string.Empty;
                _contentBody = val;
                ContentData = Encoding.UTF8.GetBytes(_contentBody);
            }
        }
    }
}