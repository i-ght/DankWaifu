using DankWaifu.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DankWaifu.Net
{
    public class TwoCaptchaWaifu
    {
        private readonly HttpWaifu _client;
        private readonly string _apiKey;

        /// <summary>
        /// Creates a new instance of a two captcha solver
        /// </summary>
        /// <param name="apiKey"></param>
        public TwoCaptchaWaifu(string apiKey)
        {
            _apiKey = apiKey;

            WebProxy proxy = null;
            if (File.Exists("2captcha_proxy.txt"))
            {
                var lines = File.ReadAllText("2captcha_proxy.txt");
                var split = lines.Split(Environment.NewLine.ToCharArray());
                proxy = new WebProxy(split[0]);
            }

#if DEBUG
            proxy = new WebProxy("192.168.0.22:8887");
#endif

            var cfg = new HttpWaifuConfig
            {
                Proxy = proxy,
                UserAgent = "helloworld"
            };
            _client = new HttpWaifu(cfg);
        }

        ///// <summary>
        ///// Attempts to solve the captcha
        ///// </summary>
        ///// <param name="image"></param>
        ///// <returns></returns>
        //public string SolveCaptcha(byte[] image)
        //{
        //    var postvars = new Dictionary<string, string>
        //    {
        //        ["method"] = "base64",
        //        ["key"] = _apiKey,
        //        ["body"] = Convert.ToBase64String(image)
        //    };

        //    var request = new HttpReq(HttpMethod.Post, "http://2captcha.com/in.php")
        //    {
        //        Accept = "application/json",
        //        ContentType = "application/x-www-form-urlencoded",
        //        ContentBody = postvars.ToUrlEncodedQueryString()()
        //    };
        //    var response = _client.SendRequest(request);
        //    if (!response.IsExpected("OK"))
        //        return string.Empty;

        //    var split = response.ContentBody.Split('|');
        //    if (split.Length < 2)
        //        return string.Empty;

        //    var captchaId = split[1];
        //    return PollSolution(captchaId);
        //}

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
                throw new InvalidOperationException("2captcha returned unexpected response");

            var split = response.ContentBody.Split('|');
            if (split.Length < 2)
                throw new InvalidOperationException("2captcha response did not contain a captcha ID");

            var captchaId = split[1];
            return await PollSolutionAsync(captchaId, 60).ConfigureAwait(false);
        }

        //public int SolveRotateCaptcha(byte[] data)
        //{
        //    var boundary = Guid.NewGuid().ToString();
        //    var contentType = $"multipart/form-data; boundary={boundary}";

        //    var sb = new StringBuilder();
        //    sb.Append($"--{boundary}\r\n");
        //    sb.Append("Content-Disposition: form-data; name=\"key\"\r\n\r\n");
        //    sb.Append($"{_apiKey}\r\n");
        //    sb.Append($"--{boundary}\r\n");
        //    sb.Append("Content-Disposition: form-data; name=\"method\"\r\n\r\n");
        //    sb.Append("rotatecaptcha\r\n");
        //    sb.Append($"--{boundary}\r\n");
        //    sb.Append("Content-Disposition: form-data; name=\"file_1\"; filename=\"helloworld.jpg\"\r\n");
        //    sb.Append("Content-Type: image/jpg\r\n\r\n");
        //    sb.Append("%IMAGE\r\n");
        //    sb.Append($"--{boundary}--\r\n");

        //    var contentData = HttpHelpers.MultipartData(sb.ToString(), data);
        //    var req = new HttpReq(HttpMethod.POST, "http://2captcha.com/in.php")
        //    {
        //        Accept = "application/json",
        //        ContentType = contentType,
        //        ContentData = contentData
        //    };

        //    var response = _client.SendRequest(req);
        //    if (!response.IsExpected("OK"))
        //        return 0;

        //    var split = response.ContentBody.Split('|');
        //    if (split.Length < 2)
        //        return 0;

        //    var captchaId = split[1];
        //    var solutionStr = PollSolution(captchaId);

        //    int ret;
        //    if (int.TryParse(solutionStr, out ret))
        //        return ret;

        //    return 0;
        //}

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

            var response = await _client.SendRequestAsync(req)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK || !response.ContentBody.Contains("OK"))
                return 0;

            var split = response.ContentBody.Split('|');
            if (split.Length < 2)
                return 0;

            var captchaId = split[1];
            var solutionStr = await PollSolutionAsync(captchaId, timeout)
                .ConfigureAwait(false);

            int ret;
            if (int.TryParse(solutionStr, out ret))
                return ret;

            return 0;
        }

        ///// <summary>
        ///// Polls the captcha id for solution
        ///// </summary>
        ///// <param name="captchaId"></param>
        ///// <returns></returns>
        //private string PollSolution(string captchaId)
        //{
        //    var request = new HttpReq(HttpMethod.Get, $"http://2captcha.com/res.php?key={_apiKey}&action=get&id={captchaId}")
        //    {
        //        Accept = "application/json"
        //    };

        //    for (var i = 0; i < 30; i++)
        //    {
        //        var response = _client.SendRequest(request);

        //        if (!string.IsNullOrWhiteSpace(response.ContentBody))
        //        {
        //            if (response.ContentBody.Contains("ERROR_KEY_DOES_NOT_EXIST"))
        //                return string.Empty;

        //            if (response.ContentBody.Contains("ERROR_WRONG_ID_FORMAT"))
        //                return string.Empty;

        //            if (response.ContentBody.Contains("ERROR_CAPTCHA_UNSOLVABLE"))
        //                return string.Empty;
        //        }

        //        if (!response.IsExpected("OK"))
        //        {
        //            Thread.Sleep(1000);
        //            continue;
        //        }

        //        var split = response.ContentBody.Split('|');
        //        if (split.Length <= 1)
        //            return string.Empty;

        //        var solution = split[1];
        //        return solution;
        //    }

        //    return string.Empty;
        //}

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
                var response = await _client.SendRequestAsync(request).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(response.ContentBody))
                {
                    if (response.ContentBody.Contains("ERROR_KEY_DOES_NOT_EXIST"))
                        throw new InvalidOperationException("2captcha returned ERROR_KEY_DOES_NOT_EXIST");

                    if (response.ContentBody.Contains("ERROR_WRONG_ID_FORMAT"))
                        throw new InvalidOperationException("2captcha returned ERROR_WRONG_ID_FORMAT");

                    if (response.ContentBody.Contains("ERROR_CAPTCHA_UNSOLVABLE"))
                        throw new InvalidOperationException("2captcha returned ERROR_CAPTCHA_UNSOLVABLE");

                    if (response.ContentBody.Contains("ERROR_WRONG_CAPTCHA_ID"))
                        throw new InvalidOperationException("2captcha returned ERROR_WRONG_CAPTCHA_ID");
                }

                if (response.StatusCode != HttpStatusCode.OK || !response.ContentBody.Contains("OK"))
                {
                    await Task.Delay(1000)
                        .ConfigureAwait(false);
                    continue;
                }

                var split = response.ContentBody.Split('|');
                if (split.Length <= 1)
                    throw new InvalidOperationException("2captcha poll returned unexpected response");

                var solution = split[1];
                if (string.IsNullOrWhiteSpace(solution))
                    throw new InvalidOperationException("2captcha returned empty string");
                return solution;
            }

            throw new TimeoutException("2captcha timed out");
        }
    }
}