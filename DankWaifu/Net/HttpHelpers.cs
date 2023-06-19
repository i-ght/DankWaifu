using DankWaifu.Sys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DankWaifu.Net
{
    public static class HttpHelpers
    {
        private const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        private static readonly string[] CommonDesktopUserAgents;

        static HttpHelpers()
        {
            CommonDesktopUserAgents = new[]
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.95 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.95 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_2) AppleWebKit/602.3.12 (KHTML, like Gecko) Version/10.0.2 Safari/602.3.12",
                "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0",
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36",
                "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:51.0) Gecko/20100101 Firefox/51.0",
                "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:50.0) Gecko/20100101 Firefox/50.0"
            };
        }

        /// <summary>
        /// Url encodes the input string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="spaceIsPlusSymbol"></param>
        /// <returns></returns>
        public static string UrlEncode(string data, bool spaceIsPlusSymbol = true)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var c in data)
            {
                if (UnreservedChars.IndexOf(c) != -1)
                {
                    sb.Append(c);
                    continue;
                }

                sb.Append($"%{(int)c:X2}");
            }

            var ret = sb.ToString();
            if (spaceIsPlusSymbol)
                ret = ret.Replace("%20", "+");

            return ret;
        }

        public static string RandomDesktopUserAgent()
        {
            return CommonDesktopUserAgents[RandomHelpers.RandomInt(CommonDesktopUserAgents.Length)];
        }

        public static byte[] MultipartData(string postData, byte[] photo)
        {
            var lst = new List<byte[]>();

            if (!postData.Contains("%IMAGE"))
                return new byte[0];

            var lines = Regex.Split(postData, "%IMAGE");
            if (lines.Length != 2)
                return new byte[0];

            string part1 = lines[0],
                   part2 = lines[1];

            lst.Add(Encoding.UTF8.GetBytes(part1));
            lst.Add(photo);
            lst.Add(Encoding.UTF8.GetBytes(part2));

            return CombineByteArrays(lst);
        }

        private static byte[] CombineByteArrays(IReadOnlyCollection<byte[]> byteQueue)
        {
            var ret = new byte[byteQueue.Sum(x => x.Length)];
            var offset = 0;
            foreach (var array in byteQueue)
            {
                Buffer.BlockCopy(array, 0, ret, offset, array.Length);
                offset += array.Length;
            }
            return ret;
        }

        public static bool TryFindCookieByName(CookieContainer cookies, string name, out Cookie cookie)
        {
            cookie = null;
            var table = CookieContainerDomainTable(cookies);
            foreach (var key in table.Keys)
            {
                var host = (string)key;
                if (host.StartsWith("."))
                    host = host.Substring(1);

                foreach (Cookie c in cookies.GetCookies(new Uri($"http://{host}/")))
                {
                    if (!string.Equals(c.Name,
                        name,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    cookie = c;
                    return true;
                }
                foreach (Cookie c in cookies.GetCookies(new Uri($"https://{host}/")))
                {
                    if (!string.Equals(c.Name,
                        name,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    cookie = c;
                    return true;
                }
            }

            return false;
        }

        public static HashSet<Cookie> AllCookies(CookieContainer cookies)
        {
            var ret = new HashSet<Cookie>();
            var table = CookieContainerDomainTable(cookies);

            foreach (var key in table.Keys)
            {
                var keyStr = key.ToString();
                if (keyStr.StartsWith("."))
                    keyStr = keyStr.Substring(1);

                if (!Uri.TryCreate($"http://{keyStr}/", UriKind.Absolute, out var uri))
                    continue;

                if (cookies.GetCookies(uri).Count > 0)
                {
                    foreach (Cookie cookie in cookies.GetCookies(uri))
                    {
                        ret.Add(cookie);
                    }
                }

                if (!Uri.TryCreate($"https://{keyStr}/", UriKind.Absolute, out uri))
                    continue;

                if (cookies.GetCookies(uri).Count <= 0)
                {
                    continue;
                }

                foreach (Cookie cookie in cookies.GetCookies(uri))
                {
                    ret.Add(cookie);
                }
            }

            return ret;
        }

        private static Hashtable CookieContainerDomainTable(CookieContainer cookies)
        {
            var ret = (Hashtable)cookies.GetType().InvokeMember("m_domainTable",
                BindingFlags.NonPublic |
                BindingFlags.GetField |
                BindingFlags.Instance,
                null,
                cookies,
                new object[] { });
            return ret;
        }
    }
}