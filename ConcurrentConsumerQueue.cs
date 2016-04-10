using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tup.Utilities
{
    /// <summary>
    ///     并行消费者队列
    /// </summary>
    /// <remarks>
    ///     FROM: https://gist.github.com/tupunco/10440684
    /// </remarks>
    public abstract class ConcurrentConsumerQueue<TData> : IDisposable
        where TData : class
    {
        private readonly BlockingCollection<TData> _itemQueue = new BlockingCollection<TData>();

        /// <summary>
        /// </summary>
        protected ConcurrentConsumerQueue()
        {
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>The count.</value>
        protected int Count
        {
            get { return _itemQueue.Count; }
        }

        /// <summary>
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
                LogManager.Instance.Error("{0}-Enqueue(itemQueue.Add)-ex:{1}", GetType(), ex);
            }
        }

        /// <summary>
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
                    LogManager.Instance.Error("{0}-ConcurrentConsumerQueue-DataItem:{1}-ex:{2}",
                        GetType(), dataItem, ex);
                    ex = null;

                    Thread.Sleep(5);
                }
            }
        }

        /// <summary>
        ///     Processes the queue.
        /// </summary>
        /// <param name="dataItem"></param>
        protected abstract void Process(TData dataItem);

        #region Dispose

        /// <summary>
        ///     Flag: Has Dispose already been called?
        /// </summary>
        private bool _disposed;

        /// <summary>
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
                LogManager.Instance.Error("{0}-Dispose(CompleteAdding)-ex:{1}", GetType(), ex);
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
        }

        /// <summary>
        /// </summary>
        ~ConcurrentConsumerQueue()
        {
            Dispose(false);
        }

        #endregion Dispose
    }

    /// <summary>
    ///     并行缓冲消费者队列
    /// </summary>
    public abstract class ConcurrentConsumerBufferQueue<TData>
        : ConcurrentConsumerQueue<TData> where TData : class
    {
        /// <summary>
        ///     BufferQueue 长度
        /// </summary>
        private static readonly int s_MaxBufferQueueLen = 1000;

        /// <summary>
        ///     BufferQueue 刷新时间间隔(单位:毫秒)-30 秒
        /// </summary>
        private static readonly int s_FlushTimerPeriod = 30 * 1000;

        /// <summary>
        /// </summary>
        private readonly int m_bufferQueueLen = s_MaxBufferQueueLen;

        /// <summary>
        /// </summary>
        private Queue<TData> m_CacheQueue;

        /// <summary>
        ///     定时 FlushCache Timer
        /// </summary>
        private Timer m_FlushTimer;

        /// <summary>
        ///     切换 CacheQueue LockObj
        /// </summary>
        private readonly object m_LockObj = new object();

        /// <summary>
        /// </summary>
        protected ConcurrentConsumerBufferQueue()
            : this(s_MaxBufferQueueLen, s_FlushTimerPeriod)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="bufferQueueLen">BufferQueue 长度</param>
        /// <remarks></remarks>
        protected ConcurrentConsumerBufferQueue(int bufferQueueLen)
            : this(bufferQueueLen, s_FlushTimerPeriod)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="bufferQueueLen">BufferQueue 长度</param>
        /// <param name="flushTimerPeriod">BufferQueue 刷新时间间隔(单位:毫秒)</param>
        /// <remarks></remarks>
        protected ConcurrentConsumerBufferQueue(int bufferQueueLen, int flushTimerPeriod)
        {
            if (bufferQueueLen <= 0)
                bufferQueueLen = s_MaxBufferQueueLen;
            if (flushTimerPeriod <= 0)
                flushTimerPeriod = s_FlushTimerPeriod;

            m_bufferQueueLen = bufferQueueLen;
            m_CacheQueue = new Queue<TData>(m_bufferQueueLen);
            m_FlushTimer = new Timer(_ => FlushCache(), null, flushTimerPeriod, flushTimerPeriod);
        }

        /// <summary>
        /// </summary>
        /// <param name="dataItem"></param>
        protected override void Process(TData dataItem)
        {
            if (dataItem == null)
                return;

            m_CacheQueue.Enqueue(dataItem);
            if (m_CacheQueue.Count >= m_bufferQueueLen)
                FlushCache();
        }

        /// <summary>
        ///     Flush Cache
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

                oldCacheQueue = m_CacheQueue;
                m_CacheQueue = new Queue<TData>(m_bufferQueueLen);
            }
            FlushProcessItem(oldCacheQueue);
            oldCacheQueue = null;
        }

        /// <summary>
        ///     Processes the buffer queue item.
        /// </summary>
        /// <param name="dataBufferQueue"></param>
        protected abstract void FlushProcessItem(IEnumerable<TData> dataBufferQueue);

        #region Dispose

        /// <summary>
        ///     Flag: Has Dispose already been called?
        /// </summary>
        private bool disposed;

        /// <summary>
        ///     Protected implementation of Dispose pattern.
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

        #endregion Dispose
    }
}
