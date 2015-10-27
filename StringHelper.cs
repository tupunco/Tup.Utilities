using System;
using System.Collections.Generic;

namespace Tup.Utilities
{
    /// <summary>
    /// String Helper
    /// </summary>
    internal static class StringHelper
    {
        #region ToArrayEx

        /// <summary>
        /// 获取 int 类型的参数值列表, 逗号分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static int[] ToArrayEx(this string str, string splitChar, int defaultVal)
        {
            return ToArrayEx(str, splitChar,
                cs =>
                {
                    var rel = 0;
                    if (!int.TryParse(cs, out rel))
                        rel = defaultVal;

                    return rel;
                });
        }

        /// <summary>
        /// 获取 long 类型的参数值列表, 逗号分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static long[] ToArrayEx(this string str, string splitChar, long defaultVal)
        {
            return ToArrayEx(str, splitChar,
                cs =>
                {
                    var rel = 0L;
                    if (!long.TryParse(cs, out rel))
                        rel = defaultVal;

                    return rel;
                });
        }

        /// <summary>
        /// 获取 float 类型的参数值列表, 逗号分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static float[] ToArrayEx(this string str, string splitChar, float defaultVal)
        {
            return ToArrayEx(str, splitChar,
                cs =>
                {
                    var rel = 0F;
                    if (!float.TryParse(cs, out rel))
                        rel = defaultVal;

                    return rel;
                });
        }

        /// <summary>
        /// 获取 string 类型的参数值列表, 逗号分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <returns></returns>
        public static string[] ToArrayEx(this string str, string splitChar)
        {
            return ToArrayEx(str, splitChar, cs => cs.Trim2());
        }

        /// <summary>
        /// 转换 特殊符号 分隔的字符串成 指定类型 的数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="parseAction">解析 Action [当前项, 结果]</param>
        /// <returns></returns>
        public static TResult[] ToArrayEx<TResult>(this string str,
                                                            string splitChar,
                                                            Func<string, TResult> parseAction)
        {
            return ToArrayEx<TResult>(str, new string[] { splitChar }, parseAction);
        }

        /// <summary>
        /// 获取指定类型的参数值列表, 逗号分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="parseAction">解析 Action [当前项, 结果]</param>
        /// <returns></returns>
        public static TResult[] ToArrayEx<TResult>(this string str,
                                                            string[] splitChars,
                                                            Func<string, TResult> parseAction)
        {
            ThrowHelper.ThrowIfNull(splitChars, "splitChars");
            ThrowHelper.ThrowIfNull(parseAction, "parseAction");

            if (string.IsNullOrEmpty(str))
                return null;

            var ss = str.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            if (ss == null || ss.Length <= 0)
                return null;

            var outList = new List<TResult>(ss.Length);
            foreach (var cs in ss)
                outList.Add(parseAction(cs));

            return outList.ToArray();
        }

        #endregion ToArrayEx

        #region ParseTo

        /// <summary>
        /// 解析当前字符串到 Int32 值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="str"></param>
        /// <param name="parseAction"></param>
        /// <returns></returns>
        public static int ParseToInt32(this string str, int defaultVal)
        {
            if (str.IsEmpty())
                return defaultVal;

            return ParseTo(str, cs =>
            {
                var rel = 0;
                if (!int.TryParse(cs, out rel))
                    rel = defaultVal;

                return rel;
            });
        }

        /// <summary>
        /// 解析当前字符串到 Int64 值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="str"></param>
        /// <param name="parseAction"></param>
        /// <returns></returns>
        public static long ParseToInt64(this string str, long defaultVal)
        {
            if (str.IsEmpty())
                return defaultVal;

            return ParseTo(str, cs =>
            {
                var rel = 0L;
                if (!long.TryParse(cs, out rel))
                    rel = defaultVal;

                return rel;
            });
        }

        /// <summary>
        /// 解析当前字符串到 DateTime 值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="str"></param>
        /// <param name="parseAction"></param>
        /// <returns></returns>
        public static DateTime ParseToDateTime(this string str, DateTime defaultVal)
        {
            if (str.IsEmpty())
                return defaultVal;

            return ParseTo(str, cs =>
            {
                var rel = DateTime.MinValue;
                if (!DateTime.TryParse(cs, out rel))
                    rel = defaultVal;

                return rel;
            });
        }

        /// <summary>
        /// 解析当前字符串到指定类型的值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="str"></param>
        /// <param name="parseAction"></param>
        /// <returns></returns>
        public static TResult ParseTo<TResult>(this string str, Func<string, TResult> parseAction)
        {
            ThrowHelper.ThrowIfNull(parseAction, "parseAction");

            if (str.IsEmpty())
                return default(TResult);

            return parseAction(str);
        }

        #endregion ParseTo

        #region Trim/Empty/Fmt

        /// <summary>
        /// 从当前 System.String 对象移除所有前导空白字符和尾部空白字符。
        /// NULL 字符串不会抛出异常
        /// </summary>
        /// <param name="strOri"></param>
        /// <returns></returns>
        public static string Trim2(this string strOri)
        {
            if (null == strOri)
                return strOri;

            return strOri.Trim();
        }

        /// <summary>
        /// Check that a string is not null or empty
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>bool</returns>
        public static bool HasValue(this string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// 指示指定的 System.String 对象是 null 还是 System.String.Empty 字符串。
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>bool</returns>
        public static bool IsEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        /// <summary>
        /// 将指定 System.String 中的格式项替换为指定数组中相应 System.Object 实例的值的文本等效项。
        /// </summary>
        /// <param name="format">复合格式字符串。</param>
        /// <param name="args">包含零个或多个要格式化的对象的 System.Object 数组。</param>
        /// <returns>format 的一个副本，其中格式项已替换为 args 中相应 System.Object 实例的 System.String 等效项。</returns>
        /// <exception cref="System.ArgumentNullException">format 或 args 为 null</exception>
        /// <exception cref="System.FormatException">format 无效。 - 或 - 用于指示要格式化的参数的数字小于零，或者大于等于 args 数组的长度</exception>
        public static string Fmt(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        #endregion Trim/Empty/Fmt
    }
}