using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tup.Utilities
{
    using Logging;

    /// <summary>
    /// 并行消费者队列
    /// </summary>
    /// <remarks>
    /// FROM: https://gist.github.com/tupunco/10440684
    ///
    /// <code>
    /// // 发送 MSMQ Single Queue
    /// s_MQSendQueue.Enqueue(new Pair[string, string](tag, msgMQ));
    ///
    /// /// <summary>
    /// /// MQ Buffer 发送队列
    /// /// </summary>
    /// private static MQBufferConsumerQueue s_MQBufferSendQueue = new MQBufferConsumerQueue();
    /// /// <summary>
    /// /// MQ Buffer 发送队列
    /// /// </summary>
    /// public class MQBufferConsumerQueue
    ///     : ConcurrentConsumerBufferQueue[Pair[string, string]]
    /// {
    ///     public MQBufferConsumerQueue() : base(10, 5000) { }
    ///
    ///     protected override void FlushProcessItem(IEnumerable[Pair[string, string]] dataBufferQueue)
    ///     {
    ///         if (dataBufferQueue == null)
    ///             return;
    ///
    ///         //TODO Buffer SendMQ
    ///         System.Console.WriteLine("MQBufferConsumerQueue-FlushProcessItem:\r\n{0}",
    ///                                         string.Join(",\r\nE", dataBufferQueue));
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public abstract class ConcurrentConsumerQueue<TData> : IDisposable
        where TData : class
    {
        /// <summary>
        /// 调试日志
        /// </summary>
        private static ILogger Log = LogManager.GetLogger<ConcurrentConsumerQueue<TData>>();

        private BlockingCollection<TData> _itemQueue = new BlockingCollection<TData>();

        /// <summary>
        ///
        /// </summary>
        public ConcurrentConsumerQueue()
        {
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        #region Dispose

        /// <summary>
        /// Flag: Has Dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            try
            {
                _itemQueue.CompleteAdding();
                Dispose(true);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0}-Dispose(CompleteAdding)-ex:{1}", this.GetType(), ex);
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>
        ///
        /// </summary>
        ~ConcurrentConsumerQueue()
        {
            Dispose(false);
        }

        #endregion

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        protected int Count
        {
            get
            {
                return _itemQueue.Count;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        public void Enqueue(TData dataItem)
        {
            try
            {
                if (_itemQueue.IsAddingCompleted)
                    return;

                _itemQueue.Add(dataItem);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0}-Enqueue(itemQueue.Add)-ex:{1}", this.GetType(), ex);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void ProcessQueue()
        {
            foreach (var dataItem in _itemQueue.GetConsumingEnumerable())
            {
                try
                {
                    Process(dataItem);

                    Thread.Sleep(0);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("{0}-ConcurrentConsumerQueue-DataItem:{1}-ex:{2}",
                                            this.GetType(), dataItem, ex);
                    ex = null;

                    Thread.Sleep(5);
                }
            }
        }

        /// <summary>
        /// Processes the queue.
        /// </summary>
        /// <param name="dataItem"></param>
        protected abstract void Process(TData dataItem);
    }

    /// <summary>
    /// 并行 缓冲 消费者队列
    /// </summary>
    /// <remarks>
    /// <code>
    /// //发送 MSMQ Buffer Queue
    /// s_MQBufferSendQueue.Enqueue(new Pair[string, string](tag, msgMQ));
    ///
    /// /// <summary>
    /// /// MQ Buffer 发送队列
    /// /// </summary>
    /// private static MQBufferConsumerQueue s_MQBufferSendQueue = new MQBufferConsumerQueue();
    /// /// <summary>
    /// /// MQ Buffer 发送队列
    /// /// </summary>
    /// public class MQBufferConsumerQueue
    ///     : ConcurrentConsumerBufferQueue[Pair[string, string]]
    /// {
    ///     public MQBufferConsumerQueue() : base(10, 5000) { }
    ///
    ///     protected override void FlushProcessItem(IEnumerable[Pair[string, string]] dataBufferQueue)
    ///     {
    ///         if (dataBufferQueue == null)
    ///             return;
    ///
    ///         //TODO Buffer SendMQ
    ///         System.Console.WriteLine("MQBufferConsumerQueue-FlushProcessItem:\r\n{0}",
    ///                                         string.Join(",\r\nE", dataBufferQueue));
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public abstract class ConcurrentConsumerBufferQueue<TData>
        : ConcurrentConsumerQueue<TData> where TData : class
    {
        /// <summary>
        /// 调试日志
        /// </summary>
        private static ILogger Log = LogManager.GetLogger<ConcurrentConsumerBufferQueue<TData>>();

        /// <summary>
        /// BufferQueue 长度
        /// </summary>
        private readonly static int s_MaxBufferQueueLen = 1000;

        /// <summary>
        /// BufferQueue 刷新时间间隔(单位:毫秒)-30 秒
        /// </summary>
        private readonly static int s_FlushTimerPeriod = 30 * 1000;

        /// <summary>
        /// 切换 CacheQueue LockObj
        /// </summary>
        private object m_LockObj = new object();

        /// <summary>
        /// 定时 FlushCache Timer
        /// </summary>
        private Timer m_FlushTimer = null;

        /// <summary>
        ///
        /// </summary>
        private Queue<TData> m_CacheQueue = null;

        /// <summary>
        ///
        /// </summary>
        private int m_bufferQueueLen = s_MaxBufferQueueLen;

        /// <summary>
        ///
        /// </summary>
        public ConcurrentConsumerBufferQueue() : this(s_MaxBufferQueueLen, s_FlushTimerPeriod) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bufferQueueLen">BufferQueue 长度</param>
        /// <remarks></remarks>
        public ConcurrentConsumerBufferQueue(int bufferQueueLen) : this(bufferQueueLen, s_FlushTimerPeriod) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bufferQueueLen">BufferQueue 长度</param>
        /// <param name="flushTimerPeriod">BufferQueue 刷新时间间隔(单位:毫秒)</param>
        /// <remarks></remarks>
        public ConcurrentConsumerBufferQueue(int bufferQueueLen, int flushTimerPeriod)
        {
            if (bufferQueueLen <= 0)
                bufferQueueLen = s_MaxBufferQueueLen;
            if (flushTimerPeriod <= 0)
                flushTimerPeriod = s_FlushTimerPeriod;

            m_bufferQueueLen = bufferQueueLen;
            m_CacheQueue = new Queue<TData>(m_bufferQueueLen);
            m_FlushTimer = new Timer(_ => ProcessFlushCache(), null, flushTimerPeriod, flushTimerPeriod);
        }

        #region Dispose

        /// <summary>
        /// Flag: Has Dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                m_FlushTimer.Dispose();
                m_FlushTimer = null;
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <param name="dataItem"></param>
        protected override void Process(TData dataItem)
        {
            if (dataItem == null)
                return;

            m_CacheQueue.Enqueue(dataItem);
            if (m_CacheQueue.Count >= m_bufferQueueLen)
                ProcessFlushCache();
        }

        /// <summary>
        /// Process FlushCache
        /// </summary>
        private void ProcessFlushCache()
        {
            try
            {
                FlushCache();

                Thread.Sleep(0);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("{0}-ProcessFlushCache-ex:{1}",
                                        this.GetType(), ex);
                ex = null;

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Flush Cache
        /// </summary>
        public void FlushCache()
        {
            if (m_CacheQueue == null || m_CacheQueue.Count <= 0)
                return;

            Queue<TData> oldCacheQueue = null;
            lock (m_LockObj)
            {
                if (m_CacheQueue == null || m_CacheQueue.Count <= 0)
                    return;

                oldCacheQueue = this.m_CacheQueue;
                this.m_CacheQueue = new Queue<TData>(m_bufferQueueLen);
            }
            FlushProcessItem(oldCacheQueue);
            oldCacheQueue = null;
        }

        /// <summary>
        /// Processes the buffer queue item.
        /// </summary>
        /// <param name="dataBufferQueue"></param>
        protected abstract void FlushProcessItem(IEnumerable<TData> dataBufferQueue);
    }
}

//namespace Tup.Utilities.Test
//{
//    public class TestClass
//    {
//        /// <summary>
//        /// 发送 MSMQ
//        /// </summary>
//        /// <param name="tag"></param>
//        /// <param name="msgMQ"></param>
//        public static void SendMQ(string tag, string msgMQ)
//        {
//            //Single Queue
//            s_MQSendQueue.Enqueue(new Pair<string, string>(tag, msgMQ));

//            //Buffer Queue
//            s_MQBufferSendQueue.Enqueue(new Pair<string, string>(tag, msgMQ));
//        }

//        /// <summary>
//        /// MQ 发送队列
//        /// </summary>
//        private static MQConsumerQueue s_MQSendQueue = new MQConsumerQueue();
//        /// <summary>
//        /// MQ 发送队列
//        /// </summary>
//        public class MQConsumerQueue
//            : ConcurrentConsumerQueue<Pair<string, string>>
//        {
//            protected override void Process(Pair<string, string> dataItem)
//            {
//                if (dataItem == null || dataItem.Key.IsEmpty() || dataItem.Value.IsEmpty())
//                    return;

//                //TODO SendMQ
//                System.Console.WriteLine("MQConsumerQueue-Process:{0}", dataItem);
//            }
//        }

//        /// <summary>
//        /// MQ Buffer 发送队列
//        /// </summary>
//        private static MQBufferConsumerQueue s_MQBufferSendQueue = new MQBufferConsumerQueue();
//        /// <summary>
//        /// MQ Buffer 发送队列
//        /// </summary>
//        public class MQBufferConsumerQueue
//            : ConcurrentConsumerBufferQueue<Pair<string, string>>
//        {
//            public MQBufferConsumerQueue() : base(10, 5000) { }

//            protected override void FlushProcessItem(IEnumerable<Pair<string, string>> dataBufferQueue)
//            {
//                if (dataBufferQueue == null)
//                    return;

//                //TODO Buffer SendMQ
//                System.Console.WriteLine("MQBufferConsumerQueue-FlushProcessItem:\r\n{0}",
//                                                string.Join(",\r\nE", dataBufferQueue));
//            }
//        }
//    }
//}