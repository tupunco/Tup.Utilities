using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Tup.Utilities
{
    using Logging;

    /// <summary>
    /// 重试 Helper
    /// </summary>
    /// <remarks>
    ///  RetryHelper.Retry("Retry-XXXX-操作", () => DoSome(1, 2));
    ///
    ///  service.Retry("Retry-XXXX-事务操作",
    ///        trans =>
    ///        {
    ///            DoChangeSome1(1, 2, 3, trans);
    ///            DoChangeSome2(1, 2, 3, trans);
    ///        });
    /// </remarks>
    public static class RetryHelper
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(RetryHelper));

        #region Consts

        /// <summary>
        /// 异常重试 间隔时间, 10 秒, (单位:毫秒)
        /// </summary>
        public const int ErrorRetrySleep = 10 * 1000;

        /// <summary>
        /// 异常重试 间隔时间, 60 秒, (单位:毫秒)
        /// </summary>
        public const int ErrorRetrySleep60 = 60 * 1000;

        /// <summary>
        /// 异常重试 间隔时间, 10 秒, (单位:毫秒)
        /// </summary>
        public const int ErrorRetrySleep10 = 10 * 1000;

        /// <summary>
        /// 异常重试 次数 5
        /// </summary>
        public const int ErrorRetryCount = 5;

        /// <summary>
        /// 异常重试 次数 10
        /// </summary>
        public const int ErrorRetryCount10 = 10;

        /// <summary>
        /// 异常重试 次数 60
        /// </summary>
        public const int ErrorRetryCount60 = 60;

        #endregion

        #region Sync

        /// <summary>
        /// 重试
        /// </summary>
        /// <param name="tag">标记</param>
        /// <param name="doAction">处理动作</param>
        /// <param name="doData">处理动作参数</param>
        /// <param name="errorAction">失败动作</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retrySleep">重试间隔, 单位:毫秒</param>
        public static void Retry(string tag,
            Action<object> doAction, object doData,
            Action<Exception> errorAction = null,
            int retryCount = ErrorRetryCount,
            int retrySleep = ErrorRetrySleep)
        {
            ThrowHelper.ThrowIfNull(tag, "tag");
            ThrowHelper.ThrowIfNull(doAction, "doAction");

            if (retryCount <= 0)
                retryCount = ErrorRetryCount;

            if (retrySleep < 0)
                retrySleep = ErrorRetrySleep;

            var saveTryCount = 0;
            while (true)
            {
                try
                {
                    doAction(doData);
                    return;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Retry-{0}-error, tryCount：{1}, doData:{2}, ex：{3}",
                                                tag, saveTryCount, doData?.ToJson(), ex);

                    errorAction?.Invoke(ex);

                    saveTryCount++;
                    if (saveTryCount >= retryCount)
                        throw ex;

                    Thread.Sleep(retrySleep * saveTryCount);
                }
            }
        }

        /// <summary>
        /// 重试
        /// </summary>
        /// <param name="tag">标记</param>
        /// <param name="doAction">处理动作</param>
        /// <param name="errorAction">失败动作</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retrySleep">重试间隔, 单位:毫秒</param>
        public static void Retry(string tag,
            Action doAction,
            Action<Exception> errorAction = null,
            int retryCount = ErrorRetryCount,
            int retrySleep = ErrorRetrySleep)
        {
            Retry(tag, _ => doAction(), null, ex => errorAction?.Invoke(ex), retryCount, retrySleep);
        }

        /// <summary>
        /// 重试 For <see cref="IServiceTransaction"/>
        /// </summary>
        /// <param name="service">事物 Service</param>
        /// <param name="tag">标记</param>
        /// <param name="doAction">处理动作</param>
        /// <param name="errorAction">失败动作</param>
        /// <param name="transContext">上下文事物对象, 设置后不提交事物</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retrySleep">重试间隔, 单位:毫秒</param>
        public static void Retry(this IServiceTransaction service,
            string tag,
            Action<IDbTransaction> doAction,
            Action<Exception> errorAction = null,
            IDbTransaction transContext = null,
            int retryCount = ErrorRetryCount,
            int retrySleep = ErrorRetrySleep)
        {
            ThrowHelper.ThrowIfNull(service, "service");
            ThrowHelper.ThrowIfNull(tag, "tag");
            ThrowHelper.ThrowIfNull(doAction, "doAction");

            if (retryCount <= 0)
                retryCount = ErrorRetryCount;

            if (retrySleep < 0)
                retrySleep = ErrorRetrySleep;

            var saveTryCount = 0;
            while (true)
            {
                var trans = transContext ?? service.BeginTransaction();
                try
                {
                    doAction(trans);

                    if (transContext == null)
                        service.CommitTransaction(trans);

                    return;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Retry-{0}-error, tryCount：{1}, ex：{2}", tag, saveTryCount, ex);

                    if (transContext == null)
                        service.RollbackTransaction(trans);

                    errorAction?.Invoke(ex);

                    saveTryCount++;
                    if (saveTryCount >= retryCount)
                        throw ex;

                    Thread.Sleep(retrySleep * saveTryCount);
                }
            }
        }

        #endregion

        #region Async

        /// <summary>
        /// 重试 Async
        /// </summary>
        /// <param name="tag">标记</param>
        /// <param name="doAction">处理动作</param>
        /// <param name="doData">处理动作参数</param>
        /// <param name="errorAction">失败动作</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retrySleep">重试间隔, 单位:毫秒</param>
        /// <param name="ignoreEx">忽略异常, 需 <paramref name="errorAction"/> 捕获异常</param>
        public static async Task RetryAsync(string tag,
            Func<object, Task> doAction, object doData,
            Func<Exception, Task> errorAction = null,
            int retryCount = ErrorRetryCount,
            int retrySleep = ErrorRetrySleep,
            bool ignoreEx = false)
        {
            ThrowHelper.ThrowIfNull(tag, "tag");
            ThrowHelper.ThrowIfNull(doAction, "doAction");

            if (retryCount <= 0)
                retryCount = ErrorRetryCount;

            if (retrySleep < 0)
                retrySleep = ErrorRetrySleep;

            var saveTryCount = 0;
            while (true)
            {
                try
                {
                    await doAction(doData);

                    return;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("RetryAsync-{0}-error, tryCount：{1}, doData:{2}, ex：{3}",
                                                tag, saveTryCount, doData?.ToJson(), ex);

                    errorAction?.Invoke(ex);

                    saveTryCount++;
                    if (saveTryCount >= retryCount)
                    {
                        if (ignoreEx)
                            return;

                        throw ex;
                    }

                    await Task.Delay(retrySleep * saveTryCount);
                }
            }
        }

        /// <summary>
        /// 重试 Async
        /// </summary>
        /// <param name="tag">标记</param>
        /// <param name="doAction">处理动作</param>
        /// <param name="errorAction">失败动作</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retrySleep">重试间隔, 单位:毫秒</param>
        /// <param name="ignoreEx">忽略异常, 需 <paramref name="errorAction"/> 捕获异常</param>
        public static Task RetryAsync(string tag,
            Func<Task> doAction,
            Func<Exception, Task> errorAction = null,
            int retryCount = ErrorRetryCount,
            int retrySleep = ErrorRetrySleep,
            bool ignoreEx = false)
        {
            return RetryAsync(tag, _ => doAction(), null, ex => errorAction?.Invoke(ex),
                            retryCount, retrySleep, ignoreEx);
        }

        /// <summary>
        /// 重试 Async For <see cref="IServiceTransaction"/>
        /// </summary>
        /// <param name="service">事物 Service</param>
        /// <param name="tag">标记</param>
        /// <param name="doAction">处理动作</param>
        /// <param name="errorAction">失败动作</param>
        /// <param name="transContext">上下文事物对象, 设置后不提交事物</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="retrySleep">重试间隔, 单位:毫秒</param>
        /// <param name="ignoreEx">忽略异常, 需 <paramref name="errorAction"/> 捕获异常</param>
        public static async Task RetryAsync(this IServiceTransaction service,
            string tag,
            Func<IDbTransaction, Task> doAction,
            Func<Exception, Task> errorAction = null,
            IDbTransaction transContext = null,
            int retryCount = ErrorRetryCount,
            int retrySleep = ErrorRetrySleep,
            bool ignoreEx = false)
        {
            ThrowHelper.ThrowIfNull(service, "service");
            ThrowHelper.ThrowIfNull(tag, "tag");
            ThrowHelper.ThrowIfNull(doAction, "doAction");

            if (retryCount <= 0)
                retryCount = ErrorRetryCount;

            if (retrySleep < 0)
                retrySleep = ErrorRetrySleep;

            var saveTryCount = 0;
            while (true)
            {
                var trans = transContext ?? service.BeginTransaction();
                try
                {
                    await doAction(trans);

                    if (transContext == null)
                        service.CommitTransaction(trans);

                    return;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("RetryAsync-{0}-error, tryCount：{1}, ex：{2}", tag, saveTryCount, ex);

                    if (transContext == null)
                        service.RollbackTransaction(trans);

                    errorAction?.Invoke(ex);

                    saveTryCount++;
                    if (saveTryCount >= retryCount)
                    {
                        if (ignoreEx)
                            return;

                        throw ex;
                    }

                    await Task.Delay(retrySleep * saveTryCount);
                }
            }
        }

        #endregion
    }
}