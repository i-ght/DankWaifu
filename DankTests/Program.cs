using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DankWaifu.Net;

namespace DankTests
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var cfg = new HttpWaifuConfig
            {
                UserAgent = HttpHelpers.RandomDesktopUserAgent(),
                Proxy = new WebProxy("192.168.0.22:8887")
            };
            var client = new HttpWaifu(cfg);

            var request = new HttpReq(HttpMethod.GET, "https://httpbin.org/get")
            {
                Accept = "application/json"
            };

            var tasks = new List<Task>();
            for (int i = 0; i < 300; i++)
            {
                tasks.Add(client.SendRequestAsync(request));
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);
        }
    }
}
