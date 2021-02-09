using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tup.Utilities
{
    /// <summary>
    /// 批量执行动作 Helper
    /// </summary>
    /// <remarks>
    /// BatchExecuteQuery `每批执行结果` 都一样, 返回 `最后一个` 结果
    /// BatchExecuteReader `每批执行结果` 为 `列表`, 并 `合并` 返回
    /// </remarks>
    public static class BatchHelper
    {
        #region BatchExecuteQuery/BatchExecuteReader

        /// <summary>
        /// 批量执行 查询操作 分组长度 (分批大小)
        /// </summary>
        public readonly static int BatchExecute_Default_MaxCount = 200;

        #region BatchExecuteQuery

        #region Sync

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static int BatchExecuteNonQuery<TFieldValue>(TFieldValue[] fValues,
                                               Func<TFieldValue[], int> executeQueryAction)
        {
            return BatchExecuteNonQuery<TFieldValue, int>(BatchExecute_Default_MaxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static int BatchExecuteNonQuery<TFieldValue>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], int> executeQueryAction)
        {
            return BatchExecuteNonQuery<TFieldValue, int>(maxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <param name="firstExecQueryAction">第一页查询执行动作</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static int BatchExecuteNonQuery<TFieldValue>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], int> executeQueryAction,
                                                Func<TFieldValue[], int> firstExecQueryAction)
        {
            return BatchExecuteNonQuery<TFieldValue, int>(maxCount, fValues, executeQueryAction, firstExecQueryAction);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static TRes BatchExecuteNonQuery<TFieldValue, TRes>(TFieldValue[] fValues,
                                                Func<TFieldValue[], TRes> executeQueryAction)
        {
            return BatchExecuteNonQuery(BatchExecute_Default_MaxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static TRes BatchExecuteNonQuery<TFieldValue, TRes>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], TRes> executeQueryAction)
        {
            return BatchExecuteNonQuery(maxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <param name="firstExecQueryAction">第一页查询执行动作</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <example>
        ///   BatchHelper.BatchExecuteQuery(100, new int[]{1,2,3,4}, ids => saveInfo(ids, info2, info3));
        /// </example>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static TRes BatchExecuteNonQuery<TFieldValue, TRes>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], TRes> executeQueryAction,
                                                Func<TFieldValue[], TRes> firstExecQueryAction)
        {
            ThrowHelper.ThrowIfNull(fValues, "filterValues");
            ThrowHelper.ThrowIfNull(executeQueryAction, "executeQueryAction");

            if (maxCount <= 0) maxCount = 200;

            if (fValues.Length <= maxCount)
            {
                if (firstExecQueryAction != null)
                    return firstExecQueryAction(fValues);

                return executeQueryAction(fValues);
            }

            //INFO 段太长保存会出错, 所以分页截取保存
            TFieldValue[] tfValues = null;
            var pageIndex = 0;
            var valuesLen = fValues.Length;
            var execRes = default(TRes);
            while (valuesLen > 0)
            {
                tfValues = fValues.Skip(pageIndex * maxCount)
                                  .Take(valuesLen > maxCount ? maxCount : valuesLen)
                                  .ToArray();
                if (tfValues == null || tfValues.Length <= 0)
                    break;

                valuesLen -= maxCount;
                pageIndex++;
                if (pageIndex <= 1 && firstExecQueryAction != null) //第一页 查询动作
                    execRes = firstExecQueryAction(tfValues);
                else
                {
                    execRes = executeQueryAction(tfValues); //其余页 查询动作
                    //if (execRes < 0)
                    //{
                    //    LogHelper.LogError("BaseDao-BatchExecuteQuery-maxCount:{0}-tFieldValues:[{1}]-valuesLen:{2}-pageIndex:{3}-execRes:{4}",
                    //                            maxCount, StringEx.Join(",", tfValues), valuesLen, pageIndex, execRes);
                    //}
                }
                System.Threading.Thread.Sleep(1);
            }
            return execRes;
        }

        #endregion

        #region Async

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static Task<int> BatchExecuteNonQueryAsync<TFieldValue>(TFieldValue[] fValues,
                                               Func<TFieldValue[], Task<int>> executeQueryAction)
        {
            return BatchExecuteNonQueryAsync<TFieldValue, int>(BatchExecute_Default_MaxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static Task<int> BatchExecuteNonQueryAsync<TFieldValue>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], Task<int>> executeQueryAction)
        {
            return BatchExecuteNonQueryAsync<TFieldValue, int>(maxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <param name="firstExecQueryAction">第一页查询执行动作</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static Task<int> BatchExecuteNonQueryAsync<TFieldValue>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], Task<int>> executeQueryAction,
                                                Func<TFieldValue[], Task<int>> firstExecQueryAction)
        {
            return BatchExecuteNonQueryAsync<TFieldValue, int>(maxCount, fValues, executeQueryAction, firstExecQueryAction);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static Task<TRes> BatchExecuteNonQueryAsync<TFieldValue, TRes>(TFieldValue[] fValues,
                                                Func<TFieldValue[], Task<TRes>> executeQueryAction)
        {
            return BatchExecuteNonQueryAsync(BatchExecute_Default_MaxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static Task<TRes> BatchExecuteNonQueryAsync<TFieldValue, TRes>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], Task<TRes>> executeQueryAction)
        {
            return BatchExecuteNonQueryAsync(maxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <param name="firstExecQueryAction">第一页查询执行动作</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <example>
        ///   BatchHelper.BatchExecuteQuery(100, new int[]{1,2,3,4}, ids => saveInfo(ids, info2, info3));
        /// </example>
        /// <remarks>
        /// 参考: DbCommand.ExecuteNonQuery
        /// </remarks>
        public static async Task<TRes> BatchExecuteNonQueryAsync<TFieldValue, TRes>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], Task<TRes>> executeQueryAction,
                                                Func<TFieldValue[], Task<TRes>> firstExecQueryAction)
        {
            ThrowHelper.ThrowIfNull(fValues, "filterValues");
            ThrowHelper.ThrowIfNull(executeQueryAction, "executeQueryAction");

            if (maxCount <= 0) maxCount = 200;

            if (fValues.Length <= maxCount)
            {
                if (firstExecQueryAction != null)
                    return await firstExecQueryAction(fValues);

                return await executeQueryAction(fValues);
            }

            //INFO 段太长保存会出错, 所以分页截取保存
            TFieldValue[] tfValues = null;
            var pageIndex = 0;
            var valuesLen = fValues.Length;
            var execRes = default(TRes);
            while (valuesLen > 0)
            {
                tfValues = fValues.Skip(pageIndex * maxCount)
                                  .Take(valuesLen > maxCount ? maxCount : valuesLen)
                                  .ToArray();
                if (tfValues == null || tfValues.Length <= 0)
                    break;

                valuesLen -= maxCount;
                pageIndex++;
                if (pageIndex <= 1 && firstExecQueryAction != null) //第一页 查询动作
                    execRes = await firstExecQueryAction(tfValues);
                else
                {
                    execRes = await executeQueryAction(tfValues); //其余页 查询动作
                    //if (execRes < 0)
                    //{
                    //    LogHelper.LogError("BaseDao-BatchExecuteQuery-maxCount:{0}-tFieldValues:[{1}]-valuesLen:{2}-pageIndex:{3}-execRes:{4}",
                    //                            maxCount, StringEx.Join(",", tfValues), valuesLen, pageIndex, execRes);
                    //}
                }
                await Task.Yield();
            }
            return execRes;
        }

        #endregion

        #endregion

        #region BatchExecuteReader

        #region Sync

        /// <summary>
        /// 批量执行 读取动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeReaderAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeReaderAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteReader
        /// </remarks>
        public static IList<TResult> BatchExecuteReader<TFieldValue, TResult>(TFieldValue[] fValues,
                                                Func<TFieldValue[], IList<TResult>> executeReaderAction)
        {
            return BatchExecuteReader(BatchExecute_Default_MaxCount, fValues, executeReaderAction);
        }

        /// <summary>
        /// 批量执行 读取动作
        /// </summary>
        /// <param name="maxCount">长字段分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeReaderAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeReaderAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteReader
        /// </remarks>
        public static IList<TResult> BatchExecuteReader<TFieldValue, TResult>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], IList<TResult>> executeReaderAction)
        {
            ThrowHelper.ThrowIfNull(fValues, "filterValues");
            ThrowHelper.ThrowIfNull(executeReaderAction, "executeReaderAction");

            if (maxCount <= 0) maxCount = 200;

            if (fValues.Length <= maxCount)
                return executeReaderAction(fValues);

            //INFO 段太长保存会出错, 所以分页截取保存
            TFieldValue[] tfValues = null;
            var pageIndex = 0;
            var valuesLen = fValues.Length;
            var execRes = new List<TResult>();
            IList<TResult> tExecRes = null;
            while (valuesLen > 0)
            {
                tfValues = fValues.Skip(pageIndex * maxCount)
                                  .Take(valuesLen > maxCount ? maxCount : valuesLen)
                                  .ToArray();
                if (tfValues == null || tfValues.Length <= 0)
                    break;

                valuesLen -= maxCount;
                pageIndex++;

                tExecRes = executeReaderAction(tfValues); //其余页 查询动作
                if (tExecRes != null)
                    execRes.AddRange(tExecRes);

                System.Threading.Thread.Sleep(1);
            }
            return execRes;
        }

        #endregion

        #region Async

        /// <summary>
        /// 批量执行 读取动作 Async
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeReaderAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeReaderAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteReader
        /// </remarks>
        public static Task<IList<TResult>> BatchExecuteReaderAsync<TFieldValue, TResult>(TFieldValue[] fValues,
                                                Func<TFieldValue[], Task<IList<TResult>>> executeReaderAction)
        {
            return BatchExecuteReaderAsync(BatchExecute_Default_MaxCount, fValues, executeReaderAction);
        }

        /// <summary>
        /// 批量执行 读取动作 Async
        /// </summary>
        /// <param name="maxCount">长字段分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeReaderAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeReaderAction 返回</returns>
        /// <remarks>
        /// 参考: DbCommand.ExecuteReader
        /// </remarks>
        public static async Task<IList<TResult>> BatchExecuteReaderAsync<TFieldValue, TResult>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], Task<IList<TResult>>> executeReaderAction)
        {
            ThrowHelper.ThrowIfNull(fValues, "filterValues");
            ThrowHelper.ThrowIfNull(executeReaderAction, "executeReaderAction");

            if (maxCount <= 0) maxCount = 200;

            if (fValues.Length <= maxCount)
                return await executeReaderAction(fValues);

            //INFO 段太长保存会出错, 所以分页截取保存
            TFieldValue[] tfValues = null;
            var pageIndex = 0;
            var valuesLen = fValues.Length;
            var execRes = new List<TResult>();
            IList<TResult> tExecRes = null;
            while (valuesLen > 0)
            {
                tfValues = fValues.Skip(pageIndex * maxCount)
                                  .Take(valuesLen > maxCount ? maxCount : valuesLen)
                                  .ToArray();
                if (tfValues == null || tfValues.Length <= 0)
                    break;

                valuesLen -= maxCount;
                pageIndex++;

                tExecRes = await executeReaderAction(tfValues); //其余页 查询动作
                if (tExecRes != null)
                    execRes.AddRange(tExecRes);

                await Task.Yield();
            }
            return execRes;
        }

        #endregion

        #endregion

        #endregion
    }
}