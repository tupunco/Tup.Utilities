using System;
using System.Messaging;
using System.Threading;

using Tup.Utilities.Logging;

namespace Tup.Utilities.Msmq
{
    /// <summary>
    ///     MSMQ Process Config
    /// </summary>
    public class MsmqProcessConfig
    {
        /// <summary>
        ///     配置标识
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     接收 MQ 的路径
        /// </summary>
        public string ReceiveMQPath { get; set; }

        /// <summary>
        ///     发送 MQ 的路径
        /// </summary>
        public string SendMQPath { get; set; }

        /// <summary>
        ///     当前 MQ 处理是否需要停止
        /// </summary>
        public bool IsMQProcessStop { get; set; }

        /// <summary>
        ///     MQ 处理失败重启间隔
        ///     (默认10秒钟)
        /// </summary>
        public int MQFailScanInterval { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                string.Format(
                    "[MsmqProcessConfig Name={0}, ReceiveMQPath={1}, SendMQPath={2}, IsMQProcessStop={3}, MQFailScanInterval={4}]",
                    Name, ReceiveMQPath, SendMQPath, IsMQProcessStop, MQFailScanInterval);
        }
    }

    /// <summary>
    ///     MSMQ Utils
    /// </summary>
    public static class MsmqUtils
    {
        /// <summary>
        ///     Logger
        /// </summary>
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MsmqUtils));

        /// <summary>
        ///     单 MQ 处理
        /// </summary>
        /// <param name="config"></param>
        public static void RegisterSingleMqProcess<TMsmqHandlerContract, TMsmqHandlerImpl>(MsmqProcessConfig config)
            where TMsmqHandlerImpl : class, TMsmqHandlerContract, new()
        {
            ThrowHelper.ThrowIfNull(config, "config");

            var msmqBindObject = MsmqBindUtils.Register<TMsmqHandlerContract, TMsmqHandlerImpl>();
            RegisterSingleMqProcess(config, msg => { msmqBindObject.Execute(msg); });
        }

        /// <summary>
        ///     注册 单 MQ 处理
        /// </summary>
        /// <param name="config"></param>
        /// <param name="singleProcessFunc">消息处理动作</param>
        public static void RegisterSingleMqProcess(MsmqProcessConfig config, Action<string> singleProcessFunc)
        {
            ThrowHelper.ThrowIfNull(singleProcessFunc, "singleProcessFunc");

            ThrowHelper.ThrowIfNull(config, "config");
            ThrowHelper.ThrowIfNull(config.Name, "config.Name");
            ThrowHelper.ThrowIfNull(config.ReceiveMQPath, "config.ReceiveMQPath");
            ThrowHelper.ThrowIfNull(config.SendMQPath, "config.SendMQPath");

            var tag = config.Name;
            var mqFailScanInterval = config.MQFailScanInterval;
            if (mqFailScanInterval <= 0)
                mqFailScanInterval = 10;

            Logger.InfoFormat("...加载[{0} MQ 处理]配置...", tag);
            config.IsMQProcessStop = false;

            var mqPath = config.ReceiveMQPath;

            var mqProcessThread = new Thread(delegate ()
                                             {
                                                 #region 访问MQ

                                                 while (true)
                                                 {
                                                     if (config.IsMQProcessStop)
                                                         break;

                                                     try
                                                     {
                                                         if (!string.IsNullOrEmpty(mqPath))
                                                         {
                                                             //if (MessageQueue.Exists(mqPath))
                                                             //{
                                                             using (var mq = new MessageQueue(mqPath))
                                                             {
                                                                 mq.Formatter = new ActiveXMessageFormatter();
                                                                 Logger.InfoFormat("处理 [{0} MQ] 正常工作", tag);

                                                                 #region 处理MQ

                                                                 //循环调用取出MQ消息
                                                                 while (true)
                                                                 {
                                                                     if (config.IsMQProcessStop)
                                                                         break;

                                                                     try
                                                                     {
                                                                         #region 单条模式

                                                                         var msg = mq.Receive(); //接受消息
                                                                         if (msg != null)
                                                                         {
                                                                             var msgBody = msg.Body as string;
                                                                             if (msgBody != null) //处理订阅消息
                                                                                 singleProcessFunc(msgBody);
                                                                             else
                                                                                 Logger.ErrorFormat(
                                                                                     "处理 [{0} MQ] Message 返回消息为 NULL,MSG:{1}",
                                                                                     tag, msg);
                                                                         }

                                                                         #endregion
                                                                     }
                                                                     catch (MessageQueueException ex)
                                                                     {
                                                                         if (ex.MessageQueueErrorCode ==
                                                                             MessageQueueErrorCode.IOTimeout &&
                                                                             !config.IsMQProcessStop)
                                                                         {
                                                                             ex = null;
                                                                             Thread.Sleep(TimeSpan.FromSeconds(100));
                                                                             //如果获取队列超时, 停顿10秒继续获取队列
                                                                         }
                                                                         else
                                                                         {
                                                                             Logger.ErrorFormat(
                                                                                 "处理 [{0} MQ][Receive Message]发生MessageQueueException异常,Ex:{1}",
                                                                                 tag, ex);
                                                                             break; //如果抛出MessageQueueException异常, 停止监听
                                                                         }
                                                                     }
                                                                     catch (OutOfMemoryException ex)
                                                                     {
                                                                         //TODO 补发机制

                                                                         Logger.ErrorFormat(
                                                                             "处理 [{0} MQ] Message 发生OutOfMemoryException异常,Ex:{1}",
                                                                             tag, ex);
                                                                         Thread.Sleep(50);
                                                                     }
                                                                     catch (Exception ex)
                                                                     {
                                                                         //TODO 补发机制

                                                                         Logger.ErrorFormat(
                                                                             "处理 [{0} MQ] Message 发生异常,Ex:{1}", tag, ex);
                                                                     }
                                                                     Thread.Sleep(0);
                                                                 }

                                                                 #endregion
                                                             }
                                                             //}
                                                             //else
                                                             //Logger.ErrorFormat("AppConfig [{0} MQ 地址] 无法访问...");
                                                         }
                                                         else
                                                             Logger.InfoFormat("AppConfig [{0} MQ 地址] 必须配置...", tag);
                                                     }
                                                     catch (Exception ex)
                                                     {
                                                         Logger.InfoFormat("处理 [{0} MQ] 发生异常(MessageQueue),Ex:{1}", tag,
                                                             ex);
                                                     }
                                                     //间隔一段时间(默认10秒钟)重启MQ
                                                     Thread.Sleep(mqFailScanInterval * 1000);
                                                 }

                                                 #endregion
                                             });
            mqProcessThread.IsBackground = false; //前台线程, 处理完已有数据才能结束
            mqProcessThread.Start();

            Logger.InfoFormat("...加载[{0} MQ 处理]完成...", tag);
        }

        /// <summary>
        ///     发送与 COM 兼容的字符串 message
        /// </summary>
        /// <param name="strPath">队列路径</param>
        /// <param name="data">要发送消息数据</param>
        /// <param name="strTitle">消息标题</param>
        public static void SendComStr(string strPath, string data, string strTitle)
        {
            ThrowHelper.ThrowIfNull(strPath, "strPath");
            ThrowHelper.ThrowIfNull(data, "data");

            using (var q = new MessageQueue(strPath))
            {
                q.Formatter = new ActiveXMessageFormatter();
                q.Send(data, strTitle ?? string.Empty);
            }
        }
    }
}
