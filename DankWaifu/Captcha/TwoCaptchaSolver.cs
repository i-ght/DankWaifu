using System;
using System.Collections.Generic;

#if !DEBUG

using System.IO;

#endif

using System.Net;
using System.Text;
using System.Threading.Tasks;
using DankWaifu.Collections;
using DankWaifu.Net;

namespace DankWaifu.Captcha
{
    public class TwoCaptchaSolver
    {
        private readonly HttpWaifu _client;
        private readonly string _apiKey;

        /// <summary>
        /// Creates a new instance of a two captcha solver
        /// </summary>
        /// <param name="apiKey"></param>
        public TwoCaptchaSolver(string apiKey)
        {
            _apiKey = apiKey;

            WebProxy proxy = null;
#if !DEBUG
            if (File.Exists("2captcha_proxy.txt"))
            {
                var lines = File.ReadAllText("2captcha_proxy.txt");
                var split = lines.Split(Environment.NewLine.ToCharArray());
                proxy = new WebProxy(split[0]);
            }
#else
            proxy = new WebProxy("192.168.1.112:8887");
#endif

            var cfg = new HttpWaifuConfig
            {
                Proxy = proxy,
                UserAgent = "helloworld"
            };
            _client = new HttpWaifu(cfg);
        }

        /// <summary>
        /// Attempts to solve the captcha
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public async Task<string> SolveCaptchaAsync(byte[] image)
        {
            var postvars = new Dictionary<string, string>
            {
                ["method"] = "base64",
                ["key"] = _apiKey,
                ["body"] = Convert.ToBase64String(image)
            };

            var request = new HttpReq(HttpMethod.POST, "http://2captcha.com/in.php")
            {
                Accept = "application/json",
                ContentType = "application/x-www-form-urlencoded",
                ContentBody = postvars.ToUrlEncodedQueryString()
            };
            var response = await _client.SendRequestAsync(request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK || !response.ContentBody.Contains("OK"))
                throw new InvalidOperationException("2captcha returned non ok status code");

            var split = response.ContentBody.Split('|');
            if (split.Length < 2)
                throw new InvalidOperationException("2captcha response did not captcha id");

            var captchaId = split[1];
            return await PollSolutionAsync(captchaId, 60)
                .ConfigureAwait(false);
        }

        public async Task<int> SolveRotateCaptchaAsync(byte[] data, int timeout)
        {
            var boundary = Guid.NewGuid().ToString();
            var contentType = $"multipart/form-data; boundary={boundary}";

            var sb = new StringBuilder();
            sb.Append($"--{boundary}\r\n");
            sb.Append("Content-Disposition: form-data; name=\"key\"\r\n\r\n");
            sb.Append($"{_apiKey}\r\n");
            sb.Append($"--{boundary}\r\n");
            sb.Append("Content-Disposition: form-data; name=\"method\"\r\n\r\n");
            sb.Append("rotatecaptcha\r\n");
            sb.Append($"--{boundary}\r\n");
            sb.Append("Content-Disposition: form-data; name=\"angle\"\r\n\r\n");
            sb.Append("51\r\n");
            sb.Append($"--{boundary}\r\n");
            sb.Append("Content-Disposition: form-data; name=\"file_1\"; filename=\"helloworld.jpg\"\r\n");
            sb.Append("Content-Type: image/jpg\r\n\r\n");
            sb.Append("%IMAGE\r\n");
            sb.Append($"--{boundary}--\r\n");

            var contentData = HttpHelpers.MultipartData(sb.ToString(), data);
            var req = new HttpReq(HttpMethod.POST, "http://2captcha.com/in.php")
            {
                Accept = "application/json",
                ContentType = contentType,
                ContentData = contentData
            };

            var response = await _client.SendRequestAsync(req).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK ||
                !response.ContentBody.Contains("OK"))
            {
                if (response.ContentBody.Contains("ERROR_ZERO_BALANCE"))
                    throw new InvalidOperationException("twocaptcha returned: ERROR_ZERO_BALANCE");

                throw new InvalidOperationException("twocaptcha returned non ok initial status code");
            }

            var split = response.ContentBody.Split('|');
            if (split.Length < 2)
                throw new InvalidOperationException("2captcha response did not captcha id");

            var captchaId = split[1];
            var solutionStr = await PollSolutionAsync(captchaId, timeout)
                .ConfigureAwait(false);

            return int.Parse(solutionStr);
        }

        /// <summary>
        /// Polls the captcha id for solution
        /// </summary>
        /// <param name="captchaId"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<string> PollSolutionAsync(string captchaId, int timeout)
        {
            var request = new HttpReq(HttpMethod.GET, $"http://2captcha.com/res.php?key={_apiKey}&action=get&id={captchaId}")
            {
                Accept = "application/json"
            };

            for (var i = 0; i < timeout; i++)
            {
                var response = await _client.SendRequestAsync(request)
                    .ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(response.ContentBody))
                {
                    if (response.ContentBody.Contains("ERROR_KEY_DOES_NOT_EXIST"))
                        throw new InvalidOperationException("twocaptcha returned: ERROR_KEY_DOES_NOT_EXIST");

                    if (response.ContentBody.Contains("ERROR_WRONG_ID_FORMAT"))
                        throw new InvalidOperationException("twocaptcha returned: ERROR_WRONG_ID_FORMAT");

                    if (response.ContentBody.Contains("ERROR_CAPTCHA_UNSOLVABLE"))
                        throw new InvalidOperationException("twocaptcha returned: ERROR_CAPTCHA_UNSOLVABLE");

                    if (response.ContentBody.Contains("ERROR_WRONG_CAPTCHA_ID"))
                        throw new InvalidOperationException("twocaptcha returned: ERROR_WRONG_CAPTCHA_ID");
                }

                if (response.StatusCode != HttpStatusCode.OK ||
                    !response.ContentBody.Contains("OK"))
                {
                    await Task.Delay(1000)
                        .ConfigureAwait(false);
                    continue;
                }

                var split = response.ContentBody.Split('|');
                if (split.Length <= 1)
                    throw new InvalidOperationException("2captcha did not deliver solution");

                var solution = split[1];
                return solution;
            }

            throw new InvalidOperationException("2captcha did not deliver solution");
        }
    }
}