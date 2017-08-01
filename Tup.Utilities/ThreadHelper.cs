using System;
using System.Threading;
//using System.Windows;
//using System.Windows.Threading;

namespace Tup.Utilities
{
    /// <summary>
    /// Thread(WPF) Helper
    /// </summary>
    public static class ThreadHelper
    {
        ///// <summary>
        ///// 延迟 Action
        ///// </summary>
        ///// <param name="cUIElement"></param>
        ///// <param name="action"></param>
        ///// <param name="deferTime">延迟时间, 单位:毫秒</param>
        //public static void DeferAction(this UIElement cUIElement, Action action, int deferTime = 200)
        //{
        //    ThrowHelper.ThrowIfNull(action, "action");
        //    ThrowHelper.ThrowIfNull(cUIElement, "cUIElement");
        //    if (deferTime <= 0)
        //        deferTime = 200;

        //    ThreadPool_QueueUserWorkItem(_ =>
        //    {
        //        Thread.Sleep(deferTime);

        //        BeginInvoke(cUIElement, action);
        //    });
        //}

        //#region DispatcherTimer
        ///// <summary>
        ///// DispatcherTimer Stop
        ///// </summary>
        ///// <param name="timer"></param>
        //public static void StopEx(this DispatcherTimer timer)
        //{
        //    ThrowHelper.ThrowIfNull(timer, "timer");

        //    timer.Stop();
        //    timer.IsEnabled = false;
        //}
        ///// <summary>
        ///// DispatcherTimer Start
        ///// </summary>
        ///// <param name="timer"></param>
        //public static void StartEx(this DispatcherTimer timer)
        //{
        //    ThrowHelper.ThrowIfNull(timer, "timer");

        //    timer.IsEnabled = true;
        //    timer.Start();
        //}
        //#endregion

        #region ThreadPool_QueueUserWorkItem
        /// <summary>
        /// Queues a method for execution. The method executes when a thread pool thread
        /// becomes available.
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public static void ThreadPool_QueueUserWorkItem(WaitCallback callBack)
        {
            ThreadPool_QueueUserWorkItem(callBack, null);
        }
        /// <summary>
        /// Queues a method for execution, and specifies an object containing data to
        /// be used by the method. The method executes when a thread pool thread becomes
        /// available.
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static void ThreadPool_QueueUserWorkItem(WaitCallback callBack,
                                                        object state,
                                                        Action<Exception> failedAction = null)
        {
            ThrowHelper.ThrowIfNull(callBack, "action");

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    callBack(_);
                }
                catch (Exception ex)
                {
                    LogHelper.Write("ThreadPool_QueueUserWorkItem-EX:{0}".Fmt(ex), LogHelper.LogMessageType.Error);

                    if (failedAction != null)
                        failedAction(ex);
#if DEBUG
                    throw ex;
#else
                    ex = null;
#endif
                }
            }, state);
        }
        #endregion

        #region Dispatcher_BeginInvoke
//        /// <summary>
//        /// Dispatcher_BeginInvoke
//        /// </summary>
//        /// <param name="cUIElement"></param>
//        /// <param name="action"></param>
//        public static void BeginInvoke(this UIElement cUIElement, Action action)
//        {
//            ThrowHelper.ThrowIfNull(cUIElement, "cUIElement");
//            ThrowHelper.ThrowIfNull(action, "action");

//            if (cUIElement.Dispatcher.CheckAccess())
//                action.Invoke();
//            else
//                cUIElement.Dispatcher.BeginInvoke(action);
//        }
//        /// <summary>
//        /// Dispatcher_BeginInvoke
//        /// </summary>
//        /// <param name="action"></param>
//        public static void Dispatcher_BeginInvoke(Action action)
//        {
//            ThrowHelper.ThrowIfNull(action, "action");

//            if (Dispatcher.CurrentDispatcher.CheckAccess())
//                action.Invoke();
//            else
//                Dispatcher.CurrentDispatcher.BeginInvoke(action);
//        }
//        /// <summary>
//        /// 同步执行的 Dispatcher_BeginInvoke
//        /// </summary>
//        /// <param name="action"></param>
//        /// <param name="waitHandle">EventWaitHandle, 如果为NULL会内部创建, 并且使用完毕会自动销毁. 如果外部传入不做销毁处理</param>
//        public static void Sync_Dispatcher_BeginInvoke(Action action, EventWaitHandle waitHandle = null)
//        {
//            ThrowHelper.ThrowIfNull(action, "action");

//            if (Dispatcher.CurrentDispatcher.CheckAccess())
//                action.Invoke();
//            else
//            {
//                #region 异步线程 调用方式
//                var autoEvent = waitHandle ?? new System.Threading.AutoResetEvent(false);

//                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
//                {
//                    Thread.Sleep(0);

//                    try
//                    {
//                        action();
//                    }
//                    finally
//                    {
//                        try
//                        {
//                            if (autoEvent != null)
//                                autoEvent.Set();
//                        }
//                        catch (Exception ex)
//                        {

//                            LogHelper.Error("Sync_Dispatcher_BeginInvoke-EX:{0}", ex);
//#if DEBUG
//                            throw ex;
//#else
//                            ex = null;
//#endif
//                        }
//                    }
//                }));
//                autoEvent.WaitOne();

//                if (waitHandle == null)
//                {
//                    autoEvent.Dispose();
//                    autoEvent = null;
//                }
//                #endregion
//            }
//        }
        #endregion
    }
}
