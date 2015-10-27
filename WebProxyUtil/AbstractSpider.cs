using System;
using System.Collections.Generic;
using System.IO;

namespace Tup.Utilities.WebProxyUtil
{
    /// <summary>
    /// 下载器抽象类
    /// </summary>
    public abstract class AbstractSpider
    {
        /// <summary>
        /// 抓取开始页码
        /// </summary>
        public int BeginPageNumber { set; get; }

        /// <summary>
        /// 抓取结束页码
        /// </summary>
        public int EndPageNumber { set; get; }

        /// <summary>
        /// URL 地址索引
        /// </summary>
        public int UrlFormatIndex { get; set; }

        public virtual void WriteToTxt(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "[fileName] NULL");

            if (ResultArticleList.Count > 0)
            {
                ////XmlSerializeHelper.SerializeToXml<List<ProxyIPNode>>("ProxyIPConfig.o.xml", oList);

                File.WriteAllLines(fileName,
                                    ResultArticleList.ConvertAll<string>(x => string.Format(@"""{0}"",""{1}""", x.IP, x.Port)).ToArray());
            }
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="msg"></param>
        public void WL(string msg) { Console.WriteLine(msg); }

        /// <summary>
        /// 处理
        /// </summary>
        protected abstract void Process();

        /// <summary>
        /// 分析
        /// </summary>
        /// <param name="html"></param>
        protected abstract void HtmlAnalyse(string html);

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected string DownLoadHtml(string url)
        {
            if (Site == null)
                throw new ArgumentNullException("Site", "[Site] null...");

            if (string.IsNullOrEmpty(url))
                return null;

            return RequestHelper.DownLoadHtml(url, null, Site.PageEncoding, null);
        }

        /// <summary>
        /// Try to string Trim
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected string TryTrim(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Trim();
        }

        private List<ProxyIPNode> _ResultArticleList;

        /// <summary>
        /// 结果数据
        /// </summary>
        public List<ProxyIPNode> ResultArticleList
        {
            get
            {
                if (_ResultArticleList == null)
                {
                    _ResultArticleList = new List<ProxyIPNode>();
                    Process();
                }
                return _ResultArticleList;
            }
            protected set
            {
                _ResultArticleList = value;
            }
        }

        /// <summary>
        /// 当前操作站点
        /// </summary>
        public SiteEntity Site { get; protected set; }

        private double _sleepTime = 0.1;

        /// <summary>
        /// 休眠秒数 默认 0.1
        /// </summary>
        protected double SleepTime
        {
            get { return _sleepTime; }
            set { _sleepTime = value; }
        }
    }

    /// <summary>
    /// 操作站点
    /// </summary>
    public class SiteEntity
    {
        /// <summary>
        /// 站点URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 站点名
        /// </summary>
        public string Name { set; get; }

        private System.Text.Encoding _PageEncoding;

        /// <summary>
        /// 站点页面编码
        /// </summary>
        public System.Text.Encoding PageEncoding
        {
            get
            {
                if (_PageEncoding == null)
                    _PageEncoding = System.Text.Encoding.Default;
                return _PageEncoding;
            }
            set
            {
                _PageEncoding = value;
            }
        }
    }
}