using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Tup.Utilities
{
    /// <summary>
    ///     集合处理 工具类
    /// </summary>
    public static class CollectionHelper
    {
        #region ToDictionary

        /// <summary>
        /// 向字典的 value 链表中添加一个对象，
        /// 当 key 不存在于字典中时， 新建一个键值， 且向字典的 value 链表中添加对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="model"></param>
        public static void AddToDictionary<T>(this IDictionary<string, List<T>> dic, string key, T model)
        {
            if (dic == null || key.IsEmpty())
                return;

            if (dic.ContainsKey(key))
                dic[key].Add(model);
            else
                dic.Add(key, new List<T> { model });
        }

        /// <summary>
        /// NameValueCollection To Dictionary
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            if (collection == null)
                return null;

            var resMap = new Dictionary<string, string>();
            foreach (string item in collection.Keys)
            {
                resMap[item ?? "-"] = collection.Get(item);
            }

            return resMap;
        }

        #endregion

        #region Hashtable2Dictionary

        /// <summary>
        /// Hashtable To Dictionary
        /// </summary>
        /// <param name="ht"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this Hashtable ht)
        {
            if (ht == null)
                return null;

            var resMap = new Dictionary<string, object>();
            foreach (DictionaryEntry item in ht)
            {
                resMap.Add(item.Key.ToString(), item.Value);
            }

            return resMap;
        }

        /// <summary>
        /// Dictionary To Hashtable
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static Hashtable ToHashtable(this IDictionary<string, object> dic)
        {
            if (dic == null)
                return null;

            var resMap = new Hashtable();
            foreach (var item in dic)
            {
                resMap.Add(item.Key, item.Value);
            }

            return resMap;
        }

        #endregion

        #region ToList/ToSet

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

        /// <summary>
        ///     Set ToList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static ISet<T> ToSet<T>(this IEnumerable<T> input, IEqualityComparer<T> comparer = null)
        {
            if (input == null)
                return null;

            if (comparer != null)
                return new HashSet<T>(input, comparer);

            return new HashSet<T>(input);
        }

        #endregion

        #region ForEach

        /// <summary>
        ///  IEnumerable ForEach
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            if (input == null || action == null)
                return;

            foreach (var item in input)
            {
                action(item);
            }
        }

        #endregion

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
        public static bool IsEmpty<T>(this IEnumerable<T> input)
        {
            if (input == null)
                return true;

            if (input is ICollection<T>)
                return (input as ICollection<T>).Count <= 0;

            return !input.Any();
        }

        #endregion IsEmpty

        #region Max

        /// <summary>
        /// 调用转换函数对序列的每个元素并返回最大 System.Int32 值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static int? Max2<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            if (source.Any())
                return source.Max(selector);
            return null;
        }

        /// <summary>
        /// 调用转换函数对序列的每个元素并返回最大 System.Int32 值。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static int? Max2<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
        {
            if (source.Any())
                return source.Max(selector);
            return null;
        }

        #endregion

        #region FirstOrDefault

        /// <summary>
        /// FirstOrDefault2
        ///     NULL 不会抛出异常
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TSource FirstOrDefault2<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return default(TSource);

            return source.FirstOrDefault();
        }

        /// <summary>
        /// FirstOrDefault2
        ///     NULL 不会抛出异常
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TSource FirstOrDefault2<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                return default(TSource);

            return source.FirstOrDefault(predicate);
        }

        #endregion

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

        #region Distinct

        /// <summary>
        /// LINQ Distinct 使用 Default EqualityComparer
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TKey> Distinct<TKey, TValue>(this IEnumerable<TKey> source,
                                                                    Func<TKey, TValue> keySelector)
        {
            return source.Distinct(new CommonEqualityComparer<TKey, TValue>(keySelector));
        }

        /// <summary>
        /// LINQ Distinct 使用 Func EqualityComparer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer">比较 Func</param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> comparer)
        {
            return source.Distinct(new FuncEqualityComparer<T>(comparer));
        }

        #endregion
    }

    /// <summary>
    /// Common Default EqualityComparer
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class CommonEqualityComparer<TKey, TValue> : IEqualityComparer<TKey>
    {
        private readonly Func<TKey, TValue> keySelector;
        private readonly EqualityComparer<TValue> comparer;

        public CommonEqualityComparer(Func<TKey, TValue> keySelector)
        {
            this.keySelector = keySelector;
            this.comparer = EqualityComparer<TValue>.Default;
        }

        public bool Equals(TKey x, TKey y)
        {
            return comparer.Equals(keySelector(x), keySelector(y));
        }

        public int GetHashCode(TKey obj)
        {
            return comparer.GetHashCode(keySelector(obj));
        }
    }

    /// <summary>
    /// Func EqualityComparer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FuncEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;
        private readonly Func<T, int> _hash;

        public FuncEqualityComparer(Func<T, T, bool> comparer)
            : this(comparer, t => 0/*强制走 Equals*/)
        {
        }

        public FuncEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hash)
        {
            _comparer = comparer;
            _hash = hash;
        }

        public bool Equals(T x, T y)
        {
            return _comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hash(obj);
        }
    }
}