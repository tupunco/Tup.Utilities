using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace Tup.Utilities
{
    /// <summary>
    ///     HttpRequest 助手
    /// </summary>
    /// <remarks>
    ///     POST 功能没有详细测试
    /// </remarks>
    public static class RequestHelper
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly static Logging.ILogger Log = Logging.LogManager.GetLogger(typeof(RequestHelper));

        /// <summary>
        ///     GET 方式下载指定 URL 的 HTML 内容
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url)
        {
            return DownLoadHtml(url, null, null);
        }

        /// <summary>
        ///     POST 方式下载指定 URL 的 HTML 内容
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="isPost">是否 POST 方式下载页面</param>
        /// <param name="postData">POST 方式下载页面的参数</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url, bool isPost, IEnumerable<KeyValuePair<string, string>> postData)
        {
            return DownLoadHtml(url, null, isPost, postData, null);
        }

        /// <summary>
        ///     GET 方式下载指定 URL 的 HTML 内容, 本方法可指定待下载页面的引用页面和页面编码
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="pageEncoding">待下载页面的页面编码</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url, string headerReferer, Encoding pageEncoding)
        {
            return DownLoadHtml(url, headerReferer, pageEncoding, null);
        }

        /// <summary>
        ///     POST 方式下载指定 URL 的 HTML 内容, 本方法可指定待下载页面的引用页面
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="isPost">是否 POST 方式下载页面</param>
        /// <param name="postData">POST 方式下载页面的参数</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url, string headerReferer, bool isPost,
                                          IEnumerable<KeyValuePair<string, string>> postData)
        {
            return DownLoadHtml(url, headerReferer, isPost, postData, null, -1, null);
        }

        /// <summary>
        ///     POST 方式下载指定 URL 的 HTML 内容, 本方法可指定待下载页面的引用页面和页面编码
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="isPost">是否 POST 方式下载页面</param>
        /// <param name="postData">POST 方式下载页面的参数</param>
        /// <param name="pageEncoding">待下载页面的页面编码</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url, string headerReferer, bool isPost,
                                          IEnumerable<KeyValuePair<string, string>> postData, Encoding pageEncoding)
        {
            return DownLoadHtml(url, headerReferer, isPost, postData, pageEncoding, -1, null);
        }

        /// <summary>
        ///     GET 方式下载指定 URL 的 HTML 内容, 本方法可指定待下载页面的引用页面/页面编码/HTTP 代理
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="pageEncoding">待下载页面的页面编码</param>
        /// <param name="webProxy">当前下载操作使用的 HTTP 代理</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url, string headerReferer, Encoding pageEncoding, IWebProxy webProxy)
        {
            return DownLoadHtml(url, headerReferer, pageEncoding, -1, webProxy);
        }

        /// <summary>
        ///     GET 方式下载指定 URL 的 HTML 内容, 本方法可指定待下载页面的引用页面/页面编码/下载超时时间/HTTP 代理
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="pageEncoding">待下载页面的页面编码</param>
        /// <param name="timeout">下载页面的超时时间, -1 将忽略本项, 单位:毫秒</param>
        /// <param name="webProxy">当前下载操作使用的 HTTP 代理</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url, string headerReferer, Encoding pageEncoding, int timeout,
                                          IWebProxy webProxy)
        {
            return DownLoadHtml(url, headerReferer, false, null, pageEncoding, timeout, webProxy);
        }

        /// <summary>
        ///     POST 方式下载指定 URL 的 HTML 内容, 本方法可指定待下载页面的引用页面/页面编码/下载超时时间/HTTP 代理
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="isPost">是否 POST 方式下载页面</param>
        /// <param name="postData">POST 方式下载页面的参数</param>
        /// <param name="pageEncoding">待下载页面的页面编码</param>
        /// <param name="timeout">下载页面的超时时间, -1 将忽略本项, 单位:毫秒</param>
        /// <param name="webProxy">当前下载操作使用的 HTTP 代理</param>
        /// <returns>下载得到的 HTML, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static string DownLoadHtml(string url, string headerReferer, bool isPost,
                                          IEnumerable<KeyValuePair<string, string>> postData, Encoding pageEncoding,
                                          int timeout, IWebProxy webProxy)
        {
            if (pageEncoding == null)
                pageEncoding = Encoding.Default;

            #region 拼接 POST 方式下载的参数信息

            byte[] tPostData = null;
            if (isPost && postData != null)
            {
                var sb = new StringBuilder();
                foreach (var item in postData)
                {
                    if (!string.IsNullOrEmpty(item.Key)
                        && !string.IsNullOrEmpty(item.Value))
                    {
                        if (sb.Length != 0)
                            sb.Append("&");

                        sb.AppendFormat("{0}={1}", item.Key,
                            HttpUtility.UrlEncode(item.Value, pageEncoding));
                        //INFO UrlEncode, 字节使用 PageEncoding.GetBytes()得到
                    }
                }
                if (sb.Length != 0)
                    tPostData = pageEncoding.GetBytes(sb.ToString());
            }

            #endregion 拼接 POST 方式下载的参数信息

            var stream = DownLoadStream(url, headerReferer, isPost, tPostData, timeout, webProxy);

            if (stream != null)
            {
                using (var sr = new StreamReader(stream, pageEncoding))
                {
                    return sr.ReadToEnd();
                }
            }
            return null;
        }

        /// <summary>
        ///     GET 方式下载指定 URL 的流数据内容, 本方法可指定待下载页面的引用页面/下载超时时间/HTTP 代理
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="timeout">下载页面的超时时间, -1 将忽略本项, 单位:毫秒</param>
        /// <param name="webProxy">当前下载操作使用的 HTTP 代理</param>
        /// <returns>下载得到的页面流数据, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static Stream DownLoadStream(string url, string headerReferer, int timeout, IWebProxy webProxy)
        {
            return DownLoadStream(url, headerReferer, false, null, timeout, webProxy);
        }

        /// <summary>
        ///     PSOT 方式下载指定 URL 的流数据内容, 本方法可指定待下载页面的引用页面/页面编码/下载超时时间/HTTP 代理
        /// </summary>
        /// <param name="url">待下载 URL</param>
        /// <param name="headerReferer">待下载页面的引用页</param>
        /// <param name="isPost">是否 POST 方式下载页面</param>
        /// <param name="postData">POST 方式下载页面的参数</param>
        /// <param name="timeout">下载页面的超时时间, -1 将忽略本项, 单位:毫秒, 默认值为 100,000 毫秒</param>
        /// <param name="webProxy">当前下载操作使用的 HTTP 代理</param>
        /// <returns>下载得到的页面流数据, 如果下载失败返回 NULL</returns>
        /// <exception cref="ArgumentNullException">url is null</exception>
        public static Stream DownLoadStream(string url, string headerReferer, bool isPost, byte[] postData, int timeout,
                                            IWebProxy webProxy)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url", "[url] null...");

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                {
                    request.Accept = "*/*";

                    if (webProxy != null)
                        request.Proxy = webProxy;

                    if (!string.IsNullOrEmpty(headerReferer))
                        request.Referer = headerReferer;

                    if (timeout > 0)
                        request.Timeout = timeout;

                    // request.Headers["Cookie"] = "ASPSESSIONIDSATSACRA=FDAANHLDOGLMEDOMKGOEBHFK";

                    #region 拼接 POST 数据

                    if (isPost)
                    {
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";

                        if (postData != null && postData.Length != 0)
                        {
                            request.ContentLength = postData.Length;

                            using (var requestStream = request.GetRequestStream())
                            {
                                requestStream.Write(postData, 0, postData.Length);
                            }
                        }
                        else
                            request.ContentLength = 0L;
                    }

                    #endregion 拼接 POST 数据

                    request.Headers["Accept-Language"] = "zh-cn";
                    request.UserAgent =
                        "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1) ; InfoPath.2; .NET CLR 2.0.50727; CIBA; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E)";

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        var stream = new MemoryStream();
                        var resStream = response.ContentEncoding != "gzip" //解压某些WEB服务器强行响应 GZIP 数据
                            ? response.GetResponseStream()
                            : new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);

                        var buffer = new byte[1024];
                        var len = 0;
                        while ((len = resStream.Read(buffer, 0, 1024)) > 0)
                        {
                            stream.Write(buffer, 0, len);
                        }

                        stream.Seek(0, SeekOrigin.Begin); //INFO 下载的流数据把起始位置设到开始, 否则对流数据的接下来的操作会有莫名 BUG
                        return stream;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("url:{0}, headerReferer:{1}".Fmt(url, headerReferer), ex);
                ex = null;
            }
            return null;
        }
    }
}
