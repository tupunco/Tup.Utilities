using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using Tup.Utilities.Wcf;

namespace Tup.Utilities.Msmq
{
    /// <summary>
    ///     MSMQ Bind Utils
    ///     解析 MSMQ 消息内容绑定到指定的调用接口上
    /// </summary>
    public static class MsmqBindUtils
    {
        /// <summary>
        ///     注册
        /// </summary>
        /// <typeparam name="TMsmqHandlerContract">MSMQ 接口契约</typeparam>
        /// <typeparam name="TMsmqHandlerImpl">MSMQ 接口实现</typeparam>
        /// <returns></returns>
        /// <remarks>
        /// <example>
        /// var msmqBind = MsmqBindUtils.Register&lt;ITestMQService, TestMQService&gt;();
        ///     msmqBind.Execute(new ITestMQService(), new MsmqMessageJsonObject()
        ///     {
        ///         HandlerName = "TestFunc",
        ///         JsonMessage = JsonHelper.SerializeObject(new Contract.Models.TestFuncMessage())
        ///     });
        /// </example>
        /// </remarks>
        public static MsmqBindObject<TMsmqHandlerContract, TMsmqHandlerImpl> Register
            <TMsmqHandlerContract, TMsmqHandlerImpl>()
            where TMsmqHandlerImpl : class, TMsmqHandlerContract, new()
        {
            return new MsmqBindObject<TMsmqHandlerContract, TMsmqHandlerImpl>();
        }

        /// <summary>
        ///     创建 THandler 创建工厂
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        internal static Func<THandler> GetNewFactory<THandler>()
            where THandler : new()
        {
            //Expression<Func<THandler>> func = Expression.Lambda<Func<THandler>>(Expression.New(typeof(THandler)), new ParameterExpression[0]);
            Expression<Func<THandler>> func = () => new THandler();
            return func.Compile();
        }
    }

    /// <summary>
    ///     MSMQ Bind Object
    /// </summary>
    /// <typeparam name="TMsmqHandlerContract">MSMQ 处理器接口契约</typeparam>
    /// <typeparam name="TMsmqHandlerImpl">MSMQ 处理器实现</typeparam>
    public class MsmqBindObject<TMsmqHandlerContract, TMsmqHandlerImpl>
        where TMsmqHandlerImpl : class, TMsmqHandlerContract, new()
    {
        /// <summary>
        ///     Msmq Contract Handler Cache
        /// </summary>
        private readonly ConcurrentDictionary<string, MethodInfo> m_MsmqContract_Cache
            = new ConcurrentDictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// </summary>
        public MsmqBindObject()
        {
            InitContractCache();
        }

        /// <summary>
        ///     MsmqHandler Factory
        /// </summary>
        public Func<TMsmqHandlerImpl> MsmqHandlerFactory { get; private set; }

        /// <summary>
        ///     初始化 契约缓存
        /// </summary>
        public void InitContractCache()
        {
            var cType = typeof(TMsmqHandlerContract);
            ParameterInfo tParameterInfo = null;
            var methods = cType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(x => x.MemberType == MemberTypes.Method //实例方法
                                           && x.ReturnType == typeof(void) //方法无返回值
                                           && x.GetParameters().Length == 1 //一个参数
                                           && (tParameterInfo = x.GetParameters()[0])
                                               .ParameterType.IsSubclassOf(typeof(MessageBase))) //参数继承自 MessageBase
                               .ToDictionary(x => x.Name, y => y);

            m_MsmqContract_Cache.AddRange(methods);

            MsmqHandlerFactory = MsmqBindUtils.GetNewFactory<TMsmqHandlerImpl>();
        }

        /// <summary>
        ///     Execute Binding Method
        /// </summary>
        /// <param name="msmqMsg">待处理 MSMQ 消息</param>
        public void Execute(string msmqMsg)
        {
            ThrowHelper.ThrowIfNull(MsmqHandlerFactory, "m_MsmqHandlerFactory");

            var mqHandler = MsmqHandlerFactory();
            Execute(mqHandler, msmqMsg);
        }

        /// <summary>
        ///     Execute Binding Method
        /// </summary>
        /// <param name="instance">MSMQ 处理器实现</param>
        /// <param name="msmqMsg">待处理 MSMQ 消息</param>
        public void Execute(TMsmqHandlerImpl instance, string msmqMsg)
        {
            ThrowHelper.ThrowIfNull(instance, "instance");
            ThrowHelper.ThrowIfNull(msmqMsg, "msmqMsg");

            var resObj = MsmqMessageJsonObject.Parse(msmqMsg);
            Execute(instance, resObj);
        }

        /// <summary>
        ///     Execute Binding Method
        /// </summary>
        /// <param name="instance">MSMQ 处理器实现</param>
        /// <param name="msg">待处理消息</param>
        internal void Execute(TMsmqHandlerImpl instance, MsmqMessageJsonObject msg)
        {
            ThrowHelper.ThrowIfNull(instance, "instance");
            ThrowHelper.ThrowIfNull(msg, "msg");
            ThrowHelper.ThrowIfNull(msg.HandlerName, "msg.HandlerName");
            ThrowHelper.ThrowIfNull(msg.JsonMessage, "msg.JsonMessage");

            var handlerMethod = m_MsmqContract_Cache.GetValue(msg.HandlerName);
            ThrowHelper.ThrowIfNull(handlerMethod, "handlerMethod");

            var msgType = handlerMethod.GetParameters()[0].ParameterType;
            var msgObj = JsonHelper.DeserializeObject(msg.JsonMessage, msgType);
            ThrowHelper.ThrowIfNull(msgObj, "msgObj");

            //handlerMethod.FastInvoke(instance, new object[] { msgObj });
            handlerMethod.Invoke(instance, new[] { msgObj });
        }
    }

    /// <summary>
    ///     Msmq Message JsonObject
    /// </summary>
    internal class MsmqMessageJsonObject
    {
        /// <summary>
        ///     消息格式正则
        ///     HandlerName|JsonMessage
        /// </summary>
        private static readonly Regex s_Msg_Parse_Reg = new Regex(@"^\s*(?<name>[\w\d]+)\s*\|\s*(?<jsonmsg>[\s\S]*)$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        ///     Handler Name
        /// </summary>
        public string HandlerName { get; set; }

        /// <summary>
        ///     JSON 格式 Message 内容
        /// </summary>
        public string JsonMessage { get; set; }

        /// <summary>
        ///     MSMQ msg string
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static MsmqMessageJsonObject Parse(string msg)
        {
            if (msg == null)
                return null;

            var m = s_Msg_Parse_Reg.Match(msg);
            if (m.Success)
            {
                return new MsmqMessageJsonObject
                {
                    HandlerName = m.Groups["name"].Value,
                    JsonMessage = m.Groups["jsonmsg"].Value
                };
            }

            return null;
        }
    }
}
