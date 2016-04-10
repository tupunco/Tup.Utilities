using System;
using System.Collections.Generic;

namespace Tup.Utilities
{
    /// <summary>
    ///     集合处理 工具类
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        ///     Set ToList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this ISet<T> input)
        {
            if (input == null)
                return null;

            return new List<T>(input);
        }

        #region IsEmpty

        /// <summary>
        ///     指示指定类型的 数组对象是 null 或者 Length = 0。
        /// </summary>
        /// <param name="input">array to check</param>
        /// <returns>bool</returns>
        public static bool IsEmpty<T>(this T[] input)
        {
            return input == null || input.Length <= 0;
        }

        /// <summary>
        ///     指示指定类型的 数组对象是 null 或者 Length = 0。
        /// </summary>
        /// <param name="input">array to check</param>
        /// <returns>bool</returns>
        public static bool IsEmpty<T>(this ICollection<T> input)
        {
            return input == null || input.Count <= 0;
        }

        #endregion IsEmpty

        #region AddRange

        /// <summary>
        ///     AddRange ICollection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="addCollection"></param>
        public static void AddRange<TInput>(this ICollection<TInput> collection, IEnumerable<TInput> addCollection)
        {
            if (collection == null || addCollection == null)
                return;

            foreach (var item in addCollection)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        ///     AddRange IDictionary
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="addCollection"></param>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> collection,
                                                  IDictionary<TKey, TValue> addCollection)
        {
            if (collection == null || addCollection == null)
                return;

            foreach (var item in addCollection)
            {
                collection[item.Key] = item.Value;
            }
        }

        /// <summary>
        ///     AddRange ICollection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="addCollection"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public static void AddRange<TInput>(this ICollection<TInput> collection,
                                            IEnumerable<TInput> addCollection,
                                            int startIndex, int count)
        {
            if (collection == null || addCollection == null || count <= 0)
                return;

            if (startIndex < 0) startIndex = 0;
            foreach (var item in addCollection)
            {
                if (startIndex > 0)
                {
                    startIndex--;
                    continue;
                }

                collection.Add(item);
                count--;
                if (count <= 0)
                    break;
            }
        }

        /// <summary>
        ///     AddRange To Queue
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="addCollection"></param>
        public static void AddRange<TInput>(this Queue<TInput> collection, IEnumerable<TInput> addCollection)
        {
            if (collection == null || addCollection == null)
                return;

            foreach (var item in addCollection)
            {
                collection.Enqueue(item);
            }
        }

        /// <summary>
        ///     AddRange To Stack
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="addCollection"></param>
        public static void AddRange<TInput>(this Stack<TInput> collection, IEnumerable<TInput> addCollection)
        {
            if (collection == null || addCollection == null)
                return;

            foreach (var item in addCollection)
            {
                collection.Push(item);
            }
        }

        #endregion AddRange

        #region Dictionary GetValue

        /// <summary>
        ///     GetValue From Dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetStringValue<TValue>(this IDictionary<string, TValue> obj, string key)
        {
            return GetStringValue(obj, key, string.Empty);
        }

        /// <summary>
        ///     GetValue From Dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetStringValue<TValue>(this IDictionary<string, TValue> obj,
                                                    string key,
                                                    string defaultValue)
        {
            return GetObjectValue(obj, key, defaultValue, tObj => tObj.ToString());
        }

        /// <summary>
        ///     GetValue From Dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="parseAction"></param>
        /// <returns></returns>
        public static TResult GetObjectValue<TValue, TResult>(this IDictionary<string, TValue> obj,
                                                              string key,
                                                              TResult defaultValue,
                                                              Func<TValue, TResult> parseAction)
        {
            if (obj == null)
                return defaultValue;

            ThrowHelper.ThrowIfNull("key", key);
            ThrowHelper.ThrowIfNull(parseAction, "parseAction");

            var tObj = default(TValue);
            if (obj.TryGetValue(key, out tObj))
            {
                if (tObj is TResult)
                    return (TResult)(object)tObj;

                return parseAction(tObj);
            }

            return defaultValue;
        }

        /// <summary>
        ///     GetValue From Dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> obj, TKey key)
        {
            return GetValue(obj, key, default(TValue));
        }

        /// <summary>
        ///     GetValue From Dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> obj,
                                                    TKey key, TValue defaultValue)
        {
            if (obj == null)
                return defaultValue;

            var tObj = default(TValue);
            if (obj.TryGetValue(key, out tObj))
                return tObj;

            return defaultValue;
        }

        #endregion Dictionary GetValue
    }
}
