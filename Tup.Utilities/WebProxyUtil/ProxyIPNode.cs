using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Tup.Utilities.WebProxyUtil
{
    /// <summary>
    ///     HTTP 代理 IP 节点实体
    /// </summary>
    [Serializable]
    public class ProxyIPNode
    {
        /// <summary>
        ///     当前代理 IP
        /// </summary>
        [XmlAttribute]
        public string IP { get; set; }

        /// <summary>
        ///     当前代理 端口
        /// </summary>
        [XmlAttribute]
        public int Port { get; set; }

        /// <summary>
        ///     是否可用
        /// </summary>
        [XmlIgnore]
        public bool CanUse { get; set; }

        /// <summary>
        ///     探测代理可用性, 最后判断时间
        /// </summary>
        [XmlIgnore]
        public DateTime LastCheckDate { get; set; }

        /// <summary>
        ///     探测代理可用性, 上次判断已经过期, 需要重新判断
        /// </summary>
        [XmlIgnore]
        public bool CheckExpired
        {
            get
            {
                return (DateTime.Now - LastCheckDate).TotalMinutes > 10; //10 分钟过期
            }
        }

        /// <summary>
        ///     ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ProxyIPNode IP:{0}, Port:{1}, CanUse:{2}, CheckExpired:{3}, LastCheckDate:{4}]", IP,
                Port, CanUse, CheckExpired, LastCheckDate);
        }
    }

    /// <summary>
    ///     ProxyIPNode 比较器
    /// </summary>
    public class ProxyIPNodeEqualityComparer : IEqualityComparer<ProxyIPNode>
    {
        #region IEqualityComparer<ProxyIPNode> 成员

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(ProxyIPNode x, ProxyIPNode y)
        {
            if (x == null || y == null)
                return false;
            return x.IP == y.IP && x.Port == x.Port;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(ProxyIPNode obj)
        {
            if (obj == null)
                return 0;
            var hashCode = 0;
            unchecked
            {
                if (obj.IP != null)
                    hashCode += 1000000007 * obj.IP.GetHashCode();
                hashCode += 1000000009 * obj.Port.GetHashCode();
            }
            return hashCode;
        }

        #endregion IEqualityComparer<ProxyIPNode> 成员
    }
}
