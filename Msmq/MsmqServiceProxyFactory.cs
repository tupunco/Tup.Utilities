using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Tup.Utilities.Msmq
{
    /// <summary>
    ///     创建自定义 MSMQ 服务代理工厂：MsmqServiceProxyFactory
    /// </summary>
    /// <remarks>
    ///     <example>
    ///         ITestMQService mq = MsmqServiceProxyFactory.Create
    ///         <ITestMQService>("FormatName:Direct=TCP:127.0.0.1\private$\TestMQService");
    ///     </example>
    /// </remarks>
    public static class MsmqServiceProxyFactory
    {
        /// <summary>
        ///     MSMQ JSON 消息发送格式
        ///     HandlerName|JsonMessage
        /// </summary>
        private static readonly string s_Msmq_JsonMsg_Format = "{0}|{1}";

        /// <summary>
        ///     创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msmqPath"></param>
        /// <returns></returns>
        public static T Create<T>(string msmqPath)
        {
            ThrowHelper.ThrowIfNull(msmqPath, "msmqPath");

            return (T)new MsmqServiceRealProxy<T>(msmqPath).GetTransparentProxy();
        }

        /// <summary>
        ///     发送与 COM 兼容的字符串 message
        /// </summary>
        /// <param name="strPath">队列路径</param>
        /// <param name="data">要发送消息数据</param>
        /// <param name="strTitle">消息标题</param>
        private static void SendComStr(string strPath, string methodName, object data, string strTitle)
        {
            ThrowHelper.ThrowIfNull(methodName, "methodName");
            ThrowHelper.ThrowIfNull(strPath, "strPath");
            ThrowHelper.ThrowIfNull(data, "data");

            var jsonData = JsonHelper.SerializeObject(data);
            ThrowHelper.ThrowIfNull(jsonData, "jsonData");

            MsmqUtils.SendComStr(strPath, s_Msmq_JsonMsg_Format.Fmt(methodName, jsonData), strTitle);
        }

        /// <summary>
        ///     创建 Msmq Contract RealProxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class MsmqServiceRealProxy<T> : RealProxy
        {
            private readonly string _msmqPath;

            public MsmqServiceRealProxy(string msmqPath)
                : base(typeof(T))
            {
                ThrowHelper.ThrowIfNull(msmqPath, "msmqPath");

                _msmqPath = msmqPath;
            }

            /// <summary>
            ///     捕获 Invoke 调用替换成调用 SendComStr
            /// </summary>
            /// <param name="msg"></param>
            /// <returns></returns>
            public override IMessage Invoke(IMessage msg)
            {
                var methodCall = (IMethodCallMessage)msg;
                var args = methodCall.Args;
                if (args == null || args.Length != 1)
                    throw new ArgumentException("methodCall.Args.Length != 1", "methodCall.Args");

                IMethodReturnMessage methodReturn = null;
                var copiedArgs = new object[args.Length];
                var method = methodCall.MethodName;
                methodCall.Args.CopyTo(copiedArgs, 0);
                try
                {
                    //序列化消息发送MQ
                    SendComStr(_msmqPath, method, args[0], method);

                    methodReturn = new ReturnMessage(null, copiedArgs, copiedArgs.Length, methodCall.LogicalCallContext,
                        methodCall);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        methodReturn = new ReturnMessage(ex.InnerException, methodCall);
                    else
                        methodReturn = new ReturnMessage(ex, methodCall);
                }

                return methodReturn;
            }
        }
    }
}
