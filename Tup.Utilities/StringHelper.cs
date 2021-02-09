﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tup.Utilities
{
    /// <summary>
    ///     String Helper
    /// </summary>
    public static class StringHelper
    {
        #region ToArrayEx

        /// <summary>
        ///     获取 int 类型的参数值列表, 逗号分隔
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
        ///     获取 long 类型的参数值列表, 逗号分隔
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
        ///     获取 float 类型的参数值列表, 逗号分隔
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
        ///     获取 string 类型的参数值列表, 逗号分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <returns></returns>
        public static string[] ToArrayEx(this string str, string splitChar)
        {
            return ToArrayEx(str, splitChar, cs => cs.Trim2());
        }

        /// <summary>
        ///     转换 特殊符号 分隔的字符串成 指定类型 的数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="parseAction">解析 Action [当前项, 结果]</param>
        /// <returns></returns>
        public static TResult[] ToArrayEx<TResult>(this string str,
                                                   string splitChar,
                                                   Func<string, TResult> parseAction)
        {
            return ToArrayEx(str, new[] { splitChar }, parseAction);
        }

        /// <summary>
        ///     获取指定类型的参数值列表, 逗号分隔
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChars"></param>
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
        ///     解析当前字符串到 Boolean 值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static bool ParseToBoolean(this string str, bool defaultVal)
        {
            if (str.IsEmpty())
                return defaultVal;

            return ParseTo(str, cs =>
            {
                var rel = false;
                if (!bool.TryParse(cs, out rel))
                    rel = defaultVal;

                return rel;
            });
        }

        /// <summary>
        ///     解析当前字符串到 Int32 值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultVal"></param>
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
        ///     解析当前字符串到 Int64 值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultVal"></param>
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
        ///     解析当前字符串到 Double 值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static double ParseToDouble(this string str, double defaultVal)
        {
            if (str.IsEmpty())
                return defaultVal;

            return ParseTo(str, cs =>
            {
                var rel = 0d;
                if (!double.TryParse(cs, out rel))
                    rel = defaultVal;

                return rel;
            });
        }

        /// <summary>
        ///     解析当前字符串到 DateTime 值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultVal"></param>
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
        ///     解析当前字符串到指定类型的值
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

        #region Trim/Empty/Fmt/Substring/Split/Join

        /// <summary>
        /// 从此实例检索子字符串。 子字符串从指定的字符位置开始且具有指定的长度。
        /// 超出可以添加 `省略号`
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length">子字符串中的字符数。</param>
        /// <param name="ellipsis">超出省略号</param>
        /// <returns>
        /// 与此实例中在 length 处开头、长度为 startIndex 的子字符串等效的一个字符串；如果 System.String.Empty 等于此实例的长度且
        /// startIndex 为零，则为 length。
        /// </returns>
        public static string Substring2(this string input, int length, string ellipsis = null)
        {
            if (input.IsEmpty())
                return string.Empty;

            if (input.Length <= length)
                return input;

            var values = input.Substring(0, length);
            return ellipsis.HasValue() ? (values + ellipsis) : values;
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited
        ///     by elements of a specified string array. A parameter specifies whether to return
        ///     empty array elements.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] Split2(this string input,
            params char[] separator)
        {
            if (input.IsEmpty())
                return new string[0];

            return input.Split(separator);
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited
        ///     by elements of a specified string array. A parameter specifies whether to return
        ///     empty array elements.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="separator"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string[] Split2(this string input,
            char[] separator,
            StringSplitOptions options = StringSplitOptions.None)
        {
            if (input.IsEmpty())
                return new string[0];

            return input.Split(separator, options);
        }

        /// <summary>
        ///  Concatenates the members of a collection, using the specified separator between
        //     each member.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join2<T>(this IEnumerable<T> values, string separator)
        {
            if (values.IsEmpty())
                return string.Empty;

            return string.Join<T>(separator, values);
        }

        /// <summary>
        ///     从当前 System.String 对象移除所有前导空白字符和尾部空白字符。
        ///     NULL 字符串不会抛出异常
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
        ///     Check that a string is not null or empty
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>bool</returns>
        public static bool HasValue(this string input)
        {
            return !string.IsNullOrEmpty(input);
        }

        /// <summary>
        ///     指示指定的 System.String 对象是 null 还是 System.String.Empty 字符串。
        /// </summary>
        /// <param name="input">String to check</param>
        /// <returns>bool</returns>
        public static bool IsEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }

        /// <summary>
        ///     将指定 System.String 中的格式项替换为指定数组中相应 System.Object 实例的值的文本等效项。
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

        #endregion

        #region Base64

        /// <summary>
        /// 修复未 UrlEncode 的 Base64 字符串解码有问题
        /// </summary>
        /// <param name="base64Url"></param>
        /// <returns></returns>
        /// <remarks>
        /// base-64 字符数组的无效长度”错误解决方案
        /// http://www.cnblogs.com/jueye/archive/2012/07/02/Url.html
        /// </remarks>
        public static string FixBase64UrlString(this string base64Url)
        {
            if (string.IsNullOrEmpty(base64Url))
                return base64Url;

            return base64Url.Replace(' ', '+');
        }

        /// <summary>
        /// 解码 Base64 字符串
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        /// <exception cref="System.FormatException"><paramref name="base64String"/> 的长度（忽略空白字符）不是 0 或 4 的倍数。 - 或 - <paramref name="base64String"/> 的格式无效。<paramref name="base64String"/> 包含一个非 base 64 字符、两个以上的填充字符或者在填充字符中包含非空白字符。</exception>
        public static string DecodeFromBase64String(this string base64String)
        {
            return DecodeFromBase64String(base64String, null, false);
        }

        /// <summary>
        /// 解码 Base64 字符串
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="isUrl"></param>
        /// <returns></returns>
        /// <exception cref="System.FormatException"><paramref name="base64String"/> 的长度（忽略空白字符）不是 0 或 4 的倍数。 - 或 - <paramref name="base64String"/> 的格式无效。<paramref name="base64String"/> 包含一个非 base 64 字符、两个以上的填充字符或者在填充字符中包含非空白字符。</exception>
        public static string DecodeFromBase64String(this string base64String, bool isUrl)
        {
            return DecodeFromBase64String(base64String, null, isUrl);
        }

        /// <summary>
        /// 解码 Base64 字符串
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="encoding"></param>
        /// <param name="isUrl"></param>
        /// <returns></returns>
        /// <exception cref="System.FormatException"><paramref name="base64String"/> 的长度（忽略空白字符）不是 0 或 4 的倍数。 - 或 - <paramref name="base64String"/> 的格式无效。<paramref name="base64String"/> 包含一个非 base 64 字符、两个以上的填充字符或者在填充字符中包含非空白字符。</exception>
        public static string DecodeFromBase64String(this string base64String, Encoding encoding, bool isUrl)
        {
            if (base64String.IsEmpty())
                return base64String;

            if (encoding == null)
                encoding = Encoding.UTF8;
            if (isUrl)
                base64String = base64String.FixBase64UrlString();

            return encoding.GetString(Convert.FromBase64String(base64String));
        }

        #endregion

        #region HexToBinary/BinaryToHex

        private static int HexToInt(char h)
        {
            if (h < '0' || h > '9')
            {
                if (h < 'a' || h > 'f')
                {
                    if (h < 'A' || h > 'F')
                    {
                        return -1;
                    }
                    return h - 65 + 10;
                }
                return h - 97 + 10;
            }
            return h - 48;
        }

        private static char NibbleToHex(byte nibble)
        {
            return (char)((nibble < 10) ? (nibble + 48) : (nibble - 10 + 65));
        }

        /// <summary>
        /// `十六进制串` 转 `字节`/Hex To Binary
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] HexToBinary(this string data)
        {
            if (data == null || data.Length % 2 != 0)
            {
                return null;
            }

            byte[] array = new byte[data.Length / 2];
            for (int i = 0; i < array.Length; i++)
            {
                int num = HexToInt(data[2 * i]);
                int num2 = HexToInt(data[2 * i + 1]);
                if (num == -1 || num2 == -1)
                {
                    return null;
                }
                array[i] = (byte)((num << 4) | num2);
            }
            return array;
        }

        /// <summary>
        /// `字节` 转 `十六进制串`/Binary to Hex
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string BinaryToHex(this byte[] data)
        {
            if (data == null)
                return null;

            char[] array = new char[checked(data.Length * 2)];
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                array[2 * i] = NibbleToHex((byte)(b >> 4));
                array[2 * i + 1] = NibbleToHex((byte)(b & 0xF));
            }
            return new string(array);
        }

        #endregion

        #region byte-AsString

        ///// <summary>
        ///// Read a stream into a byte array
        ///// </summary>
        ///// <param name="input">Stream to read</param>
        ///// <returns>byte[]</returns>
        //public static byte[] ReadAsBytes(this Stream input)
        //{
        //    if (input == null)
        //        return null;

        //    byte[] buffer = new byte[16 * 1024];
        //    using (var ms = new MemoryStream())
        //    {
        //        int read;
        //        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            ms.Write(buffer, 0, read);
        //        }
        //        return ms.ToArray();
        //    }
        //}

        /// <summary>
        /// Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string AsString(this byte[] buffer)
        {
            return AsString(buffer, null);
        }

        /// <summary>
        /// Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        /// http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
        /// </summary>
        /// <param name="buffer">An array of bytes to convert</param>
        /// <returns>The byte as a string.</returns>
        public static string AsString(this byte[] buffer, Encoding contentEncoding)
        {
            if (buffer == null || buffer.Length == 0)
                return string.Empty;

            if (contentEncoding != null)
                return contentEncoding.GetString(buffer, 0, buffer.Length);
            else
            {
                // Ansi as default
                Encoding encoding = Encoding.UTF8;
                /*
                    EF BB BF		UTF-8
                    FF FE UTF-16	little endian
                    FE FF UTF-16	big endian
                    FF FE 00 00		UTF-32, little endian
                    00 00 FE FF		UTF-32, big-endian
                    */
                if (buffer.Length >= 3 && buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                {
                    encoding = Encoding.UTF8;
                }
                else if (buffer.Length >= 2)
                {
                    if (buffer[0] == 0xfe && buffer[1] == 0xff)
                    {
                        encoding = Encoding.Unicode;
                    }
                    else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                    {
                        encoding = Encoding.BigEndianUnicode; // utf-16be
                    }
                }

                return encoding.GetString(buffer, 0, buffer.Length);
            }
        }

        #endregion
    }
}