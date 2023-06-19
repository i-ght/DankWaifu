using System;
using System.Net;
using System.Text;

namespace DankWaifu.Net
{
    public class HttpResp
    {
        private string _contentBody;

        public HttpResp()
        {
        }

        public HttpResp(
            HttpStatusCode statusCode,
            string statusDescription,
            Uri uri,
            WebHeaderCollection headers,
            CookieCollection cookies,
            byte[] contentData)
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
            Uri = uri;
            Headers = headers;
            Cookies = cookies;
            ContentData = contentData;
        }

        /// <summary>
        /// Status code of the HTTP response
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// HTTP status description
        /// </summary>
        public string StatusDescription { get; }

        /// <summary>
        /// URI of the http response
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Header collection of the HTTP response
        /// </summary>
        public WebHeaderCollection Headers { get; }

        /// <summary>
        /// Cookie collection of the HTTP response
        /// </summary>
        public CookieCollection Cookies { get; }

        /// <summary>
        /// Content data of the HTTP response
        /// </summary>
        public byte[] ContentData { get; }

        /// <summary>
        /// Content body of the HTTP response
        /// </summary>
        public string ContentBody
        {
            get
            {
                if (!string.IsNullOrEmpty(_contentBody))
                    return _contentBody;

                if (ContentData == null || ContentData.Length == 0)
                    return string.Empty;

                _contentBody = Encoding.UTF8.GetString(ContentData);
                return _contentBody;
            }
        }
    }
}