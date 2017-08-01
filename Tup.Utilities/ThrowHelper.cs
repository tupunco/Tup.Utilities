using System;
using System.Linq;

namespace Tup.Utilities
{
    /// <summary>
    ///     Helper methods to make it easier to throw exceptions.
    /// </summary>
    /// <remarks>
    ///     原始代码FROM:   MoreLINQ - Extensions to LINQ to Objects
    /// </remarks>
    internal static class ThrowHelper
    {
        /// <summary>
        ///     对象 null   判断
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfNull<T>(T argument, string name) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///     对象 null   判断
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfNull<T>(T? argument, string name) where T : struct
        {
            if (!argument.HasValue)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///     字符串对象 null,empty   判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfNull(string argument, string name)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///     逻辑对象 false   判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfFalse(bool argument, string name)
        {
            if (!argument)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///     逻辑对象 true  判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfTrue(bool argument, string name)
        {
            if (argument)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///     时间对象 MinValue  判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfNull(DateTime argument, string name)
        {
            if (argument == DateTime.MinValue)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        ///     小于0   判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfNegative(int argument, string name)
        {
            if (argument < 0)
                throw new ArgumentOutOfRangeException(name);
        }

        /// <summary>
        ///     小于等于0   判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfNonPositive(int argument, string name)
        {
            if (argument <= 0)
                throw new ArgumentOutOfRangeException(name);
        }

        /// <summary>
        ///     小于等于0   判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="name"></param>
        public static void ThrowIfNonPositive(long argument, string name)
        {
            if (argument <= 0L)
                throw new ArgumentOutOfRangeException(name);
        }

        /// <summary>
        ///     超界    判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="lowerBound">下界(不包含)</param>
        /// <param name="upperBound">上界(不包含)</param>
        /// <param name="name"></param>
        public static void ThrowIfOutOfRange(int argument, int lowerBound, int upperBound, string name)
        {
            if (argument < lowerBound || argument > upperBound)
                throw new ArgumentOutOfRangeException(name, string.Format("x<{0} or x>{1}", lowerBound, upperBound));
        }

        /// <summary>
        ///     超界    判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="boundArray">需要匹配的枚举</param>
        /// <param name="name"></param>
        public static void ThrowIfOutOfRange(int argument, int[] boundArray, string name)
        {
            if (!boundArray.Any(x => x == argument))
                throw new ArgumentOutOfRangeException(name,
                    string.Format("[{0}]", string.Join("],[", Array.ConvertAll(boundArray, x => x.ToString()))));
        }

        /// <summary>
        ///     超界    判断
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="boundArray">需要匹配的枚举</param>
        /// <param name="name"></param>
        public static void ThrowIfOutOfRange(string argument, string[] boundArray, string name)
        {
            if (!boundArray.Any(x => x == argument))
                throw new ArgumentOutOfRangeException(name, string.Format("[{0}]", string.Join("],[", boundArray)));
        }
    }
}
