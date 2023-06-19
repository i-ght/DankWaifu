using DankWaifu.Tasks;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DankWaifu.Net
{
    public class HttpWaifu
    {
        static HttpWaifu()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 222;
        }

        /// <summary>
        /// Creates a new instance of an HttpWaifu HTTP client
        /// </summary>
        /// <param name="cfg"></param>
        public HttpWaifu(HttpWaifuConfig cfg)
        {
            Config = cfg ?? throw new ArgumentNullException(nameof(cfg));
            Cookies = new CookieContainer();
        }

        /// <summary>
        /// Configuration for this client.
        /// </summary>
        public HttpWaifuConfig Config { get; }

        /// <summary>
        /// Cookie container for this client.
        /// </summary>
        public CookieContainer Cookies { get; set; }

        private HttpWebRequest CreateHttpWebRequest(HttpReq httpReq)
        {
            if (Config.ProxyRequired && Config.Proxy == null && httpReq.OverrideProxy == null)
            {
                throw new InvalidOperationException("proxy is required to send this request.");
            }

            var httpWebRequest = WebRequest.CreateHttp(httpReq.Uri);

            httpWebRequest.Proxy = httpReq.OverrideProxy ?? Config.Proxy;
            httpWebRequest.Method = httpReq.HttpMethod.ToString();
            httpWebRequest.ProtocolVersion = httpReq.ProtocolVersion;

            if (httpReq.Timeout == TimeSpan.FromSeconds(-1))
                httpWebRequest.ReadWriteTimeout = (int)Config.DefaultTimeout.TotalMilliseconds;
            else
                httpWebRequest.ReadWriteTimeout = (int)httpReq.Timeout.TotalMilliseconds;

            if (httpReq.Timeout == TimeSpan.FromSeconds(-1))
                httpWebRequest.Timeout = (int)Config.DefaultTimeout.TotalMilliseconds;
            else
                httpWebRequest.Timeout = (int)httpReq.Timeout.TotalMilliseconds;

            httpWebRequest.ServicePoint.ReceiveBufferSize = 8192;
            httpWebRequest.ServicePoint.Expect100Continue = false;
            //httpWebRequest.ServicePoint.ConnectionLeaseTimeout = 60000

#if DEBUG
            httpWebRequest.ServerCertificateValidationCallback = delegate { return true; };
#endif

            httpWebRequest.Headers = CustomHeadersForRequest(httpReq);

            if (!string.IsNullOrWhiteSpace(httpReq.Accept))
                httpWebRequest.Accept = httpReq.Accept;

            if (!string.IsNullOrWhiteSpace(httpReq.Origin))
                httpWebRequest.Headers.Add("Origin", httpReq.Origin);

            if (!string.IsNullOrWhiteSpace(httpReq.Referer))
                httpWebRequest.Referer = httpReq.Referer;

            if (!string.IsNullOrWhiteSpace(httpReq.ContentType))
                httpWebRequest.ContentType = httpReq.ContentType;

            if (!string.IsNullOrWhiteSpace(Config.UserAgent) &&
                string.IsNullOrWhiteSpace(httpReq.OverrideUserAgent))
            {
                httpWebRequest.UserAgent = Config.UserAgent;
            }
            else if (!string.IsNullOrWhiteSpace(httpReq.OverrideUserAgent))
                httpWebRequest.UserAgent = httpReq.OverrideUserAgent;

            httpWebRequest.AllowAutoRedirect = httpReq.FollowRedirect;
            httpWebRequest.AutomaticDecompression = httpReq.AcceptEncoding;
            httpWebRequest.KeepAlive = httpReq.KeepAlive;

            if (Config.UseCookies)
                httpWebRequest.CookieContainer = Cookies;

            return httpWebRequest;
        }

        private WebHeaderCollection CustomHeadersForRequest(HttpReq httpReq)
        {
            var headers = new WebHeaderCollection();

            if (Config.DefaultHeaders.Count > 0)
            {
                for (var i = 0; i < Config.DefaultHeaders.Count; i++)
                {
                    var key = Config.DefaultHeaders.GetKey(i);
                    var values = Config.DefaultHeaders.GetValues(key);
                    if (values == null)
                        continue;

                    foreach (var val in values)
                        headers.Add(key, val);
                }
            }
            else if (httpReq.OverrideHeaders.Count > 0)
            {
                for (var i = 0; i < httpReq.OverrideHeaders.Count; i++)
                {
                    var key = httpReq.OverrideHeaders.GetKey(i);
                    var values = httpReq.OverrideHeaders.GetValues(key);
                    if (values == null)
                        continue;

                    foreach (var val in values)
                        headers.Add(key, val);
                }
            }

            if (httpReq.AdditionalHeaders.Count <= 0)
                return headers;

            for (var i = 0; i < httpReq.AdditionalHeaders.Count; i++)
            {
                var key = httpReq.AdditionalHeaders.GetKey(i);
                var values = httpReq.AdditionalHeaders.GetValues(i);
                if (values == null)
                    continue;

                foreach (var val in values)
                    headers.Add(key, val);
            }

            return headers;
        }

        /// <summary>
        /// Sends the http web request.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="WebException"></exception>
        /// <param name="httpReq"></param>
        /// <returns></returns>
        public async Task<HttpResp> SendRequestAsync(HttpReq httpReq)
        {
            if (httpReq == null)
                throw new ArgumentNullException(nameof(httpReq));

            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = CreateHttpWebRequest(httpReq);

                var timeout =
                    httpReq.Timeout == TimeSpan.FromSeconds(-1) ?
                    Config.DefaultTimeout :
                    httpReq.Timeout;

                switch (httpReq.HttpMethod)
                {
                    case HttpMethod.NONE:
                        throw new InvalidOperationException("invalid http method");
                    case HttpMethod.GET:
                        if (httpReq.ContentLength > -1)
                            httpWebRequest.ContentLength = httpReq.ContentLength;
                        break;

                    case HttpMethod.PUT:
                    case HttpMethod.POST:
                        if (httpReq.ContentData.Length == 0)
                        {
                            httpWebRequest.ContentLength = 0;
                            break;
                        }

                        await WriteHttpRequestContentBodyAsync(
                            httpWebRequest,
                            timeout,
                            httpReq.ContentData,
                            httpReq.ContentData.Length
                        ).ConfigureAwait(false);
                        break;
                }

                using (var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync()
                    .TimeoutAfter(timeout)
                    .ConfigureAwait(false))
                {
                    var ret = await ReadHttpResponseAsync(response, timeout)
                        .ConfigureAwait(false);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case WebException w:
                        if (httpReq.ThrowOnWebEx ||
                            Config.ThrowOnWebEx)
                        {
                            throw;
                        }

                        return await ReadHttpResponseFromWebExAsync(
                            w,
                            TimeSpan.FromSeconds(30)
                        ).ConfigureAwait(false);

                    case InvalidOperationException _:
                    case IOException _:
                        httpWebRequest?.Abort();

                        if (httpReq.ThrowOnIOEx ||
                            Config.ThrowOnIOEx)
                        {
                            throw;
                        }

                        return new HttpResp();

                    case TimeoutException _:
                    case OperationCanceledException _:
                        httpWebRequest?.Abort();

                        if (httpReq.ThrowOnTimeoutEx ||
                            Config.ThrowOnTimeoutEx)
                        {
                            throw;
                        }

                        return new HttpResp();

                    default:
                        httpWebRequest?.Abort();
                        throw;
                }
            }
        }

        private static async Task WriteHttpRequestContentBodyAsync(WebRequest httpWebRequest, TimeSpan timeout, byte[] data, int contentLength)
        {
            httpWebRequest.ContentLength = contentLength;

            using (var reqStream = await httpWebRequest.GetRequestStreamAsync()
                .TimeoutAfter(timeout)
                .ConfigureAwait(false))
            {
                using (var memoryStream = new MemoryStream(data))
                {
                    await memoryStream.CopyToAsync(
                        reqStream,
                        8192
                    ).TimeoutAfter(timeout).ConfigureAwait(false);
                }
            }
        }

        private async Task<HttpResp> ReadHttpResponseAsync(HttpWebResponse response, TimeSpan timeout)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var contentBody = await ReadHttpResponseContentBodyAsync(
                response,
                timeout
            ).ConfigureAwait(false);
            var statusCode = response.StatusCode;
            var statusDescription = response.StatusDescription;
            var headers = response.Headers;
            var cookies = response.Cookies;
            var uri = response.ResponseUri;

            if (Config.UseCookies)
                Cookies.Add(response.Cookies);

            var ret = new HttpResp(statusCode, statusDescription, uri, headers, cookies, contentBody);
            return ret;
        }

        private async Task<byte[]> ReadHttpResponseContentBodyAsync(HttpWebResponse response, TimeSpan timeout)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                {
                    throw new InvalidOperationException(
                        "GetResponseStream returned null"
                    );
                }

                using (var memoryStream = new MemoryStream())
                {
                    await responseStream.CopyToAsync(
                        memoryStream,
                        8192
                    ).TimeoutAfter(timeout).ConfigureAwait(false);
                    return memoryStream.ToArray();
                }
            }
        }

        private async Task<HttpResp> ReadHttpResponseFromWebExAsync(WebException webEx, TimeSpan timeout)
        {
            if (webEx.Response == null)
                return new HttpResp();

            try
            {
                using (var response = (HttpWebResponse)webEx.Response)
                {
                    return await ReadHttpResponseAsync(
                        response,
                        timeout
                    ).ConfigureAwait(false);
                }
            }
            catch
            {
                return new HttpResp();
            }
        }
    }
}