using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Tup.Utilities.WebProxyUtil
{
    /// <summary>
    /// Proxycn 代理中国 HTTP 代理列表 下载器
    /// 全文页面: 
    ///    首 http://www.proxycn.com/html_proxy/http-1.html
    ///    末 http://www.proxycn.com/html_proxy/http-62.html
    /// </summary>
    public class ProxycnSpider : AbstractSpider
    {
        string pageFormat = @"http://www.proxycn.com/html_proxy/http-{0}.html";
        /// <summary>
        /// 
        /// </summary>
        public ProxycnSpider()
        {
            Site = new SiteEntity()
            {
                Url = "www.proxycn.com",
                Name = "Proxycn-HTTP-代理列表",
                PageEncoding = System.Text.Encoding.GetEncoding("GB2312")
            };
        }
        /// <summary>
        /// 处理
        /// </summary>
        protected override void Process()
        {
            for (var i = BeginPageNumber; i <= EndPageNumber; i++)
            {
                var url = string.Format(pageFormat, i);

                Console.WriteLine("ProxycnSpider-Url:{0}", url);

                string downLoadHtml = DownLoadHtml(url);
                Thread.Sleep((int)(SleepTime * 1000));

                if (!string.IsNullOrEmpty(downLoadHtml))
                    HtmlAnalyse(downLoadHtml);
            }
        }
        /// <summary>
        /// 分析
        /// </summary>
        /// <param name="html"></param>
        protected override void HtmlAnalyse(string html)
        {
            if (string.IsNullOrEmpty(html))
                return;

            //处理列表部分
            Match match = Regex.Match(html, REGEX_PATTERN);
            while (match.Success)
            {
                int p = Convert.ToInt32(match.Groups["p"].Value);
                double t = Convert.ToDouble(match.Groups["t"].Value);
                string ip = match.Groups["ip"].Value.Trim();
                if (t <= 1)
                {
                    ProxyIPNode article = new ProxyIPNode()
                    {
                        Port = p,
                        IP = ip
                    };
                    ResultArticleList.Add(article);
                }
                match = match.NextMatch();
            }
        }
        const string REGEX_PATTERN = @"<TD\s+class=""list"">\s*(?<p>[^<]*)</TD>\s*<TD\s+class=""list"">[^<]*</TD>\s*<TD\s+class=""list"">[^<]*</TD>\s*<TD\s+class=""list"">[^<]*</TD>\s*<TD\s+class=""list"">\s*(?<t>[^<]*)</TD>\s*<TD\s+class=""list"">\s*<a\s+href=whois\.php\?whois=(?<ip>\d+\.\d+\.\d+\.\d+)\s+target=_blank>\s*whois\s*</TD>\s*</TR>";
    }
}
