using DankWaifu.Collections;
using DankWaifu.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DankWaifu.Captcha
{
    public class ThreeCaptcha5Me
    {
        private readonly string _2CaptchaAPIKey;
        private readonly HttpWaifu _client;

        public ThreeCaptcha5Me(string twoCaptchaAPIKey)
        {
            if (twoCaptchaAPIKey == null)
                throw new ArgumentNullException(nameof(twoCaptchaAPIKey));

            if (string.IsNullOrWhiteSpace(twoCaptchaAPIKey))
                throw new ArgumentException($"{nameof(twoCaptchaAPIKey)} is whitespace", nameof(twoCaptchaAPIKey));

            _2CaptchaAPIKey = twoCaptchaAPIKey;

            WebProxy proxy = null;
            if (File.Exists("2captcha_proxy.txt"))
            {
                var fileContents = File.ReadAllText("2captcha_proxy.txt")
                    .Split(Environment.NewLine.ToCharArray())[0];
                proxy = new WebProxy(fileContents);
            }

#if DEBUG
            proxy = new WebProxy("192.168.1.112:8887");
#endif

            var cfg = new HttpWaifuConfig
            {
                Proxy = proxy,
                ProxyRequired = false
            };

            _client = new HttpWaifu(cfg);
        }

        public async Task<string> REEEEEEECaptcha(string dateSiteKey, string pageUrl, TimeSpan timeout)
        {
            if (dateSiteKey == null)
                throw new ArgumentNullException(nameof(dateSiteKey));

            if (string.IsNullOrWhiteSpace(dateSiteKey))
                throw new ArgumentException($"{nameof(dateSiteKey)} can not be whitespace", nameof(dateSiteKey));

            if (pageUrl == null)
                throw new ArgumentNullException(nameof(pageUrl));

            if (string.IsNullOrWhiteSpace(pageUrl))
                throw new ArgumentException($"{nameof(pageUrl)} can not be whitespace", nameof(pageUrl));

            var postParams = new Dictionary<string, string>
            {
                ["key"] = _2CaptchaAPIKey,
                ["method"] = "userrecaptcha",
                ["googlekey"] = dateSiteKey,
                //["proxy"] = proxyStr,
                ["proxytype"] = "HTTP",
                ["pageurl"] = pageUrl
            };

            WebProxy proxy = null;
            if (File.Exists("debug_proxy.txt"))
            {
                var lines = File.ReadAllLines("debug_proxy.txt");
                if (lines.Length > 0)
                {
                    NetHelpers.TryParseProxy(lines[0], out proxy);
                }
            }

            var request = new HttpReq(HttpMethod.POST, "http://2captcha.com/in.php")
            {
                Accept = "application/json",
                ContentType = "application/x-www-form-urlencoded",
                ContentBody = postParams.ToUrlEncodedQueryString(),
                OverrideProxy = proxy
            };

            var response = await _client.SendRequestAsync(request)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK ||
                !response.ContentBody.Contains("OK|"))
                throw new InvalidOperationException($"2captcha POST returned unexpected response {response.StatusCode}");

            var split = response.ContentBody.Split('|');
            if (split.Length < 2)
                throw new InvalidOperationException($"2captcha POST returned unexpected response {response.StatusCode}");

            var captchaID = split[1];
            if (string.IsNullOrWhiteSpace(captchaID))
                throw new InvalidOperationException("2captcha POST empty captchaID");

            return await PollSolutionAsync(captchaID, timeout, proxy)
                .ConfigureAwait(false);
        }

        private async Task<string> PollSolutionAsync(string captchaID, TimeSpan timeout, WebProxy proxy = null)
        {
            var request = new HttpReq(HttpMethod.GET,
                $"http://2captcha.com/res.php?key={_2CaptchaAPIKey}&action=get&id={captchaID}")
            {
                Accept = "application/json",
                OverrideProxy = proxy
            };

            using (var c = new CancellationTokenSource(timeout))
            {
                while (!c.IsCancellationRequested)
                {
                    try
                    {
                        var response = await _client.SendRequestAsync(request)
                            .ConfigureAwait(false);
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            await Task.Delay(950, c.Token)
                                .ConfigureAwait(false);
                            continue;
                        }

                        if (response.ContentBody.Contains("ERROR_WRONG_USER_KEY"))
                            throw new InvalidOperationException("2captcha: ERROR_WRONG_USER_KEY");

                        if (response.ContentBody.Contains("ERROR_KEY_DOES_NOT_EXIST"))
                            throw new InvalidOperationException("2captcha: ERROR_KEY_DOES_NOT_EXIST");

                        if (response.ContentBody.Contains("ERROR_ZERO_CAPTCHA_FILESIZE"))
                            throw new InvalidOperationException("2captcha: ERROR_ZERO_CAPTCHA_FILESIZE");

                        if (response.ContentBody.Contains("ERROR_TOO_BIG_CAPTCHA_FILESIZE"))
                            throw new InvalidOperationException("2captcha: ERROR_TOO_BIG_CAPTCHA_FILESIZE");

                        if (response.ContentBody.Contains("ERROR_WRONG_FILE_EXTENSION"))
                            throw new InvalidOperationException("2captcha: ERROR_WRONG_FILE_EXTENSION");

                        if (response.ContentBody.Contains("ERROR_IMAGE_TYPE_NOT_SUPPORTED"))
                            throw new InvalidOperationException("2captcha: ERROR_IMAGE_TYPE_NOT_SUPPORTED");

                        if (response.ContentBody.Contains("ERROR_IP_NOT_ALLOWED"))
                            throw new InvalidOperationException("2captcha: ERROR_IP_NOT_ALLOWED");

                        if (response.ContentBody.Contains("IP_BANNED"))
                            throw new InvalidOperationException("2captcha: IP_BANNED");

                        if (response.ContentBody.Contains("ERROR_CAPTCHAIMAGE_BLOCKED"))
                            throw new InvalidOperationException("2captcha: ERROR_CAPTCHAIMAGE_BLOCKED");

                        if (response.ContentBody.Contains("ERROR_CAPTCHA_UNSOLVABLE"))
                            throw new InvalidOperationException("2captcha: ERROR_CAPTCHA_UNSOLVABLE");

                        if (response.ContentBody.StartsWith("OK"))
                        {
                            var split = response.ContentBody.Split('|');
                            if (split.Length < 2)
                                throw new InvalidOperationException($"2captcha poll returned unexpected response body ({response.StatusCode})");

                            return split[1];
                        }

                        await Task.Delay(950, c.Token)
                            .ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) {/**/}
                }
            }

            throw new TimeoutException("2captcha poll solution timed out");
        }
    }
}