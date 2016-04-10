using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;

namespace Tup.Utilities.Wcf
{
    /*
    * WCF技术剖析之三十：一个很有用的WCF调用编程技巧[下篇]
    * http://www.cnblogs.com/artech/archive/2010/01/08/1642607.html
    */

    /// <summary>
    ///     步骤三：创建自定义服务代理工厂：WcfServiceProxyFactory
    /// </summary>
    /// <remarks>
    ///     <example>
    ///         ICalculator calculator = WcfServiceProxyFactory.Create&lt;ICalculator&gt;("calculatorservice");
    ///     </example>
    /// </remarks>
    public static class WcfServiceProxyFactory
    {
        /// <summary>
        ///     创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static T Create<T>(string endpointName)
        {
            ThrowHelper.ThrowIfNull(endpointName, "endpointName");

            return (T)new ServiceRealProxy<T>(endpointName).GetTransparentProxy();
        }

        /// <summary>
        ///     步骤一：创建ChannalFactory<T>的静态工厂：ChannelFactoryCreator
        /// </summary>
        internal static class ChannelFactoryCreator
        {
            private static readonly Hashtable ChannelFactories = new Hashtable();

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="endpointName"></param>
            /// <returns></returns>
            public static ChannelFactory<T> Create<T>(string endpointName)
            {
                ThrowHelper.ThrowIfNull(endpointName, "endpointName");

                ChannelFactory<T> channelFactory = null;
                if (ChannelFactories.ContainsKey(endpointName))
                {
                    channelFactory = ChannelFactories[endpointName] as ChannelFactory<T>;
                }

                if (channelFactory == null)
                {
                    channelFactory = new ChannelFactory<T>(endpointName);
                    lock (ChannelFactories.SyncRoot)
                    {
                        ChannelFactories[endpointName] = channelFactory;
                    }
                }

                return channelFactory;
            }
        }

        /// <summary>
        ///     步骤二：创建自定义RealProxy：ServiceRealProxy<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class ServiceRealProxy<T> : RealProxy
        {
            private readonly string _endpointName;

            public ServiceRealProxy(string endpointName)
                : base(typeof(T))
            {
                ThrowHelper.ThrowIfNull(endpointName, "endpointName");

                _endpointName = endpointName;
            }

            public override IMessage Invoke(IMessage msg)
            {
                var channel = ChannelFactoryCreator.Create<T>(_endpointName).CreateChannel();
                var methodCall = (IMethodCallMessage)msg;
                IMethodReturnMessage methodReturn = null;
                //object[] copiedArgs = Array.CreateInstance(typeof(object), methodCall.Args.Length) as object[];
                var copiedArgs = new object[methodCall.Args.Length];
                methodCall.Args.CopyTo(copiedArgs, 0);
                try
                {
                    var returnValue = methodCall.MethodBase.Invoke(channel, copiedArgs);
                    methodReturn = new ReturnMessage(returnValue, copiedArgs, copiedArgs.Length,
                        methodCall.LogicalCallContext, methodCall);
                    (channel as ICommunicationObject).Close();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is CommunicationException || ex.InnerException is TimeoutException)
                    {
                        (channel as ICommunicationObject).Abort();
                    }

                    if (ex.InnerException != null)
                    {
                        methodReturn = new ReturnMessage(ex.InnerException, methodCall);
                    }
                    else
                    {
                        methodReturn = new ReturnMessage(ex, methodCall);
                    }
                }

                return methodReturn;
            }
        }
    }
}
