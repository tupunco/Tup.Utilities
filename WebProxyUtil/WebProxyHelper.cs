using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Tup.Utilities.WebProxyUtil
{
    /// <summary>
    /// HTTP 代理操作 助手
    /// </summary>
    public static class WebProxyHelper
    {
        /// <summary>
        /// 代理配置文件保存文件
        /// </summary>
        private static readonly string s_Config_ProxyIPConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProxyIPConfig.xml");
        /// <summary>
        /// 
        /// </summary>
        private static List<ProxyIPNode> _webProxyIPList;
        /// <summary>
        /// 
        /// </summary>
        readonly static Random _sysRandom = new Random();
        /// <summary>
        /// 
        /// </summary>
        static WebProxyHelper()
        {
            if (File.Exists(s_Config_ProxyIPConfigPath))
                _webProxyIPList = new HashSet<ProxyIPNode>(XmlSerializeHelper.DeserializeFromXml<List<ProxyIPNode>>(s_Config_ProxyIPConfigPath)).ToList();
            else
                _webProxyIPList = null;
        }
        /// <summary>
        /// 强制过期某代理
        /// </summary>
        /// <param name="cProxyNode"></param>
        public static void ForcedExpiration(WebProxy cProxy)
        {
            if (cProxy == null) 
                return;

            var cProxyNode = new ProxyIPNode()
            {
                IP = cProxy.Address.Host,
                Port = cProxy.Address.Port
            };
            cProxyNode.CanUse = false;
            _webProxyIPList.Remove(cProxyNode);
        }
        /// <summary>
        /// 获得一个Web代理
        /// </summary>
        /// <returns></returns>
        public static WebProxy GetWebProxy()
        {
        L1:
            if (_webProxyIPList == null || _webProxyIPList.Count == 0)
            {
                LogManager.Instance.Info("代理列表为空,无法使用代理.");
                return null;
            }

            var cProxyIndex = _sysRandom.Next(0, _webProxyIPList.Count);
            var cProxyNode = _webProxyIPList[cProxyIndex];

            if (cProxyNode.CheckExpired)
                cProxyNode.CanUse = CheckWebProxy(cProxyNode);

            if (!cProxyNode.CanUse)
            {
                _webProxyIPList.Remove(cProxyNode);

                goto L1;
            }

            return new WebProxy(cProxyNode.IP, cProxyNode.Port);
        }

        /// <summary>
        /// 判断当前代理的健康性
        /// </summary>
        /// <param name="cProxyNode">当前代理节点</param>
        /// <returns></returns>
        /// <remarks>
        /// 判断代理是否可用下载'新浪网公司'简介页面,并判断是否有'新浪网'字样
        /// </remarks>
        public static bool CheckWebProxy(ProxyIPNode cProxyNode)
        {
            if (cProxyNode == null)
                return false;

            if (!cProxyNode.CheckExpired)
                return cProxyNode.CanUse;

            lock (cProxyNode)
            {
                if (!cProxyNode.CheckExpired)
                    return cProxyNode.CanUse;

                try
                {
                    WebProxy cProxy = new WebProxy(cProxyNode.IP, cProxyNode.Port);
                    //INFO 判断当前代理健康, 去下载新浪网站一个页面
                    var checkHtml = RequestHelper.DownLoadHtml("http://corp.sina.com.cn/chn/sina_index.html", null, null, 500, cProxy);
                    cProxyNode.LastCheckDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(checkHtml)
                            && checkHtml.IndexOf("新浪") > -1)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error("CheckWebProxy-ProxyIPNode:{0}-ex:{1}", cProxyNode, ex);
                    ex = null;
                    return false;
                }
            }
            return cProxyNode.CanUse;
        }

        /// <summary>
        /// 判断指定的代理列表是否有效
        /// </summary>
        /// <param name="webProxyIPList"></param>
        /// <returns>
        /// 可以用的代理列表
        /// </returns>
        public static List<ProxyIPNode> CheckAllWebProxy(List<ProxyIPNode> webProxyIPList)
        {
            if (webProxyIPList == null)
                return null;

            List<ProxyIPNode> oList = new List<ProxyIPNode>();
            foreach (var item in webProxyIPList)
            {
                Console.WriteLine("{0}", item);
                if (CheckWebProxy(item))
                {
                    item.CanUse = true;
                    oList.Add(item);
                    //Console.WriteLine("CheckAllWebProxy-O:{0}", item);
                }

                Thread.Sleep(_sysRandom.Next(1, 50));
            }
            return oList.Count == 0 ? null : oList;
        }
        /// <summary>
        /// 重新下载 代理列表
        /// </summary>
        /// <remarks>
        /// 调用程序定期调用本方法下载最新的代理列表
        /// </remarks>
        public static void ReDownLoadWebProxyList()
        {
            AbstractSpider spider = new ProxycnSpider()
            {
                BeginPageNumber = 1,
                EndPageNumber = 30 //下载30页
            };
            var nList = spider.ResultArticleList;

            //spider.WriteToTxt(Path.Combine(Directory.GetCurrentDirectory(),
            //                                   string.Format("{0}-{1}.txt", spider.Site.Name, DateTime.Now.ToShortDateString())));

            if (nList == null || nList.Count == 0)
            {
                LogManager.Instance.Info("ReDownLoadWebProxyList-NULL");
                return;
            }
            var cList = CheckAllWebProxy(nList);
            if (cList.Count > 0)
            {
                XmlSerializeHelper.SerializeToXml<List<ProxyIPNode>>(s_Config_ProxyIPConfigPath, cList);
                _webProxyIPList = cList;
                LogManager.Instance.Info("ReDownLoadWebProxyList-Checked-UPDATE-Num:{0}", cList.Count);
            }
            else
                LogManager.Instance.Info("ReDownLoadWebProxyList-Checked-NULL");
        }
    }
}
