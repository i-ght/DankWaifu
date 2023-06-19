using DankWaifu.Collections;
using DankWaifu.Tasks;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace DankWaifu.Net
{
    public static class ProxyHelpers
    {
        public static async Task FindWorkingProxyAsync(
            HttpWaifu client,
            string webPageToLoad,
            string expectedStringInResponse,
            ConcurrentQueue<WebProxy> proxies,
            int timeoutInMilliseconds)
        {
            await FindWorkingProxyAsync(
                    client,
                    webPageToLoad,
                    expectedStringInResponse,
                    proxies)
                .TimeoutAfter(timeoutInMilliseconds)
                .ConfigureAwait(false);
        }

        public static async Task FindWorkingProxyAsync(
            HttpWaifu client,
            string webPageToLoad,
            string expectedStringInResponse,
            ConcurrentQueue<WebProxy> proxies)
        {
            while (true)
            {
                var proxy = proxies.GetNext();
                if (proxy == null)
                    throw new InvalidOperationException("proxy must not be null");
                if (await TryTestProxyAsync(
                        client,
                        webPageToLoad,
                        expectedStringInResponse,
                        proxy)
                    .ConfigureAwait(false))
                {
                    break;
                }
            }
        }

        public static async Task<bool> TryTestProxyAsync(HttpWaifu client, string webPageToLoad, string expectedStringInResponse, WebProxy proxy)
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy), "proxy must not be null");

            client.Config.Proxy = proxy;
            var request = new HttpReq(HttpMethod.GET, webPageToLoad)
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                AcceptEncoding = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var response = await client.SendRequestAsync(request)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK ||
                !response.ContentBody.Contains(expectedStringInResponse))
            {
                return false;
            }

            return true;
        }
    }
}