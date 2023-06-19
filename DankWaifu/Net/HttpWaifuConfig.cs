using System;
using System.Net;

namespace DankWaifu.Net
{
    public class HttpWaifuConfig
    {
        public HttpWaifuConfig()
        {
            DefaultHeaders = new WebHeaderCollection();
            UserAgent = string.Empty;
            UseCookies = true;
            ProxyRequired = true;
            DefaultTimeout = TimeSpan.FromSeconds(90);
        }

        /// <summary>
        /// Default request headers
        /// </summary>
        public WebHeaderCollection DefaultHeaders { get; set; }

        /// <summary>
        /// Proxy for all web requests
        /// </summary>
        public WebProxy Proxy { get; set; }

        public bool ProxyRequired { get; set; }

        /// <summary>
        /// User-Agent for all web requests
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Add cookies from HTTP responses to CookieContainer, make requests with CookieContainer.
        /// </summary>
        public bool UseCookies { get; set; }

        public TimeSpan DefaultTimeout { get; set; }

        public bool ThrowOnTimeoutEx { get; set; }

        public bool ThrowOnIOEx { get; set; }

        public bool ThrowOnWebEx { get; set; }
    }
}