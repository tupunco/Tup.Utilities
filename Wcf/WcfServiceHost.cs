using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

using log4net;

namespace Tup.Utilities.Wcf
{
    /// <summary>
    ///     WCF ServiceHost
    /// </summary>
    /// <remarks>
    ///     TODO MQ 重试机制参考: https://msdn.microsoft.com/zh-cn/library/aa395218.aspx
    /// </remarks>
    internal static class WcfServiceHost
    {
        private static readonly ILog Logger = log4net.LogManager.GetLogger(typeof(WcfServiceHost));

        /// <summary>
        ///     Service Host List
        /// </summary>
        private static readonly List<ServiceHost> s_ServiceHostList = new List<ServiceHost>();

        /// <summary>
        ///     PSIServices Namespace Prefix
        /// </summary>
        private static readonly string s_PSIServices_NamespacePrefix = "Joyeon.Group.Psi2Services";

        /// <summary>
        ///     注册并运行所有 ServiceHost
        /// </summary>
        public static void RegisterAll()
        {
            //1.加载本应用域中所有继承自 IPsiService 的类型
            var assemblies = AppDomain.CurrentDomain
                                      .GetAssemblies()
                                      .Where(x => x.FullName.StartsWith(s_PSIServices_NamespacePrefix));

            var modules = new List<Type>();
            foreach (var asm in assemblies)
            {
                var query = from t in asm.GetTypes()
                            where
                                t.IsClass && t.GetInterface(typeof(IWcfServiceHost).FullName) != null && !t.IsAbstract
                            select t;

                modules.AddRange(query);
            }

            //2.作为 WCF Service Type 创建 ServiceHost
            var hostList = s_ServiceHostList;
            ServiceHost tHost = null;
            foreach (var item in modules)
            {
                tHost = new ServiceHost(item);

                ThrowHelper.ThrowIfFalse(!(tHost.Description == null || tHost.Description.Endpoints.Count <= 0),
                    "ServiceHost 配置不齐全:{0}".Fmt(item.FullName));

                tHost.Opening += (sender, e) => HandlerHostEvent(((ServiceHost)sender).Description, e, "Opening");
                tHost.Opened += (sender, e) => HandlerHostEvent(((ServiceHost)sender).Description, e, "Opened");
                tHost.Faulted += (sender, e) => HandlerHostEvent(((ServiceHost)sender).Description, e, "Faulted", true);
                tHost.Closing += (sender, e) => HandlerHostEvent(((ServiceHost)sender).Description, e, "Closing");
                tHost.Closed += (sender, e) => HandlerHostEvent(((ServiceHost)sender).Description, e, "Closed");

                tHost.Open();

                hostList.Add(tHost);
            }
        }

        /// <summary>
        ///     取消注册 ServiceHost
        /// </summary>
        public static void UnRegisterAll()
        {
            var hostList = s_ServiceHostList;
            foreach (var item in hostList)
            {
                item.Close();
            }
        }

        /// <summary>
        ///     Handler ServiceHost Event
        /// </summary>
        /// <param name="hostDescription"></param>
        /// <param name="e"></param>
        /// <param name="tag"></param>
        /// <param name="isError"></param>
        private static void HandlerHostEvent(ServiceDescription hostDescription, EventArgs e, string tag,
                                             bool isError = false)
        {
            if (isError)
                Logger.ErrorFormat("{0}-{1}-{2}-{3}", hostDescription.Name, hostDescription.ServiceType, e, tag);
            else
                Logger.InfoFormat("{0}-{1}-{2}-{3}", hostDescription.Name, hostDescription.ServiceType, e, tag);
        }
    }
}
