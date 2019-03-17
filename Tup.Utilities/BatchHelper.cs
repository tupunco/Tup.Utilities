using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        public static int BatchExecuteQuery<TFieldValue>(TFieldValue[] fValues,
                                               Func<TFieldValue[], int> executeQueryAction)
        {
            return BatchExecuteQuery<TFieldValue, int>(BatchExecute_Default_MaxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        public static int BatchExecuteQuery<TFieldValue>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], int> executeQueryAction)
        {
            return BatchExecuteQuery<TFieldValue, int>(maxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <param name="firstExecQueryAction">第一页查询执行动作</param>
        /// <returns>executeQueryAction 返回</returns>
        public static int BatchExecuteQuery<TFieldValue>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], int> executeQueryAction,
                                                Func<TFieldValue[], int> firstExecQueryAction)
        {
            return BatchExecuteQuery<TFieldValue, int>(maxCount, fValues, executeQueryAction, firstExecQueryAction);
        }

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        public static TRes BatchExecuteQuery<TFieldValue, TRes>(TFieldValue[] fValues,
                                                Func<TFieldValue[], TRes> executeQueryAction)
        {
            return BatchExecuteQuery(BatchExecute_Default_MaxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeQueryAction 返回</returns>
        public static TRes BatchExecuteQuery<TFieldValue, TRes>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], TRes> executeQueryAction)
        {
            return BatchExecuteQuery(maxCount, fValues, executeQueryAction, null);
        }

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="maxCount">长字段 分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeQueryAction">查询执行动作 (每批执行动作)</param>
        /// <param name="firstExecQueryAction">第一页查询执行动作</param>
        /// <returns>executeQueryAction 返回</returns>
        /// <example>
        ///   BatchHelper.BatchExecuteQuery(100, new int[]{1,2,3,4}, ids => saveInfo(ids, info2, info3));
        /// </example>
        public static TRes BatchExecuteQuery<TFieldValue, TRes>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], TRes> executeQueryAction,
                                                Func<TFieldValue[], TRes> firstExecQueryAction)
        {
            ThrowHelper.ThrowIfNull(fValues, "filterValues");
            ThrowHelper.ThrowIfNull(executeQueryAction, "executeQueryAction");

            if (maxCount <= 0) maxCount = 200;

            if (fValues.Length <= maxCount)
            {
                if (fValues.Length <= maxCount)
                {
                    if (firstExecQueryAction != null)
                        return firstExecQueryAction(fValues);
                    else
                        return executeQueryAction(fValues);
                }
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

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeReaderAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeReaderAction 返回</returns>
        public static List<TResult> BatchExecuteReader<TFieldValue, TResult>(TFieldValue[] fValues,
                                                Func<TFieldValue[], List<TResult>> executeReaderAction)
        {
            return BatchExecuteReader(BatchExecute_Default_MaxCount, fValues, executeReaderAction);
        }

        /// <summary>
        /// 批量执行 查询动作
        /// </summary>
        /// <param name="maxCount">长字段分组长度</param>
        /// <param name="fValues">长字段 值</param>
        /// <param name="executeReaderAction">查询执行动作 (每批执行动作)</param>
        /// <returns>executeReaderAction 返回</returns>
        public static List<TResult> BatchExecuteReader<TFieldValue, TResult>(int maxCount, TFieldValue[] fValues,
                                                Func<TFieldValue[], List<TResult>> executeReaderAction)
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
            List<TResult> tExecRes = null;
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
    }
}