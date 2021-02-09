using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Tup.Utilities
{
    /// <summary>
    ///  Newtonsoft.Json JSON 操作封装 工具类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Safe Json Deserialize
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T SafeJsonDeserialize<T>(this string s)
        {
            var type = typeof(T);
            var isString = type == typeof(string);
            var targetArray = !isString && typeof(IEnumerable).IsAssignableFrom(type);
            if (s.IsEmpty())
            {
                if (targetArray)
                    s = "[]";
                else if (!isString && !type.IsPrimitive)
                    s = "{}";
                else
                    s = "";
            }
            else if (targetArray && s == "{}")
            {
                s = "[]";
            }

            return DeserializeObject<T>(s);
        }

        /// <summary>
        ///     Deserialize Object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        /// <summary>
        ///     Serialize Object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        ///     Deserialize Object
        /// </summary>
        /// <param name="s"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object DeserializeObject(this string s, Type targetType)
        {
            return JsonConvert.DeserializeObject(s, targetType);
        }

        /// <summary>
        ///     Serialize Object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        ///  JSON Collection Object Serialize
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SafeJsonSerialize<T>(this T obj)
            where T : class
        {
            if (obj != null)
                return obj.JsonSerialize();
            else
                return string.Empty;
        }

        /// <summary>
        ///  JSON Collection Object Serialize
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string JsonSerialize<T>(this T obj)
            where T : class
        {
            if (obj == null)
            {
                if (obj is IEnumerable)
                    return "[]";
                else
                    return "{}";
            }

            return SerializeObject(obj);
        }

        #region FromJson/ToJson

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Deserialize String To JSON Object
        /// </summary>
        /// <param name="jsonBytes"></param>
        /// <param name="type">The System.Type of object being deserialized.</param>
        /// <returns></returns>
        public static object FromJson(this byte[] jsonBytes, Type type = null)
        {
            if (jsonBytes == null)
                jsonBytes = new byte[0];

            return FromJson(DefaultEncoding.GetString(jsonBytes), type);
        }

        /// <summary>
        /// Deserialize String To JSON Object
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <param name="type">The System.Type of object being deserialized.</param>
        /// <returns></returns>
        public static object FromJson(this string jsonStr, Type type = null)
        {
            if (type == null)
                return JsonConvert.DeserializeObject(jsonStr);

            return JsonConvert.DeserializeObject(jsonStr, type);
        }

        /// <summary>
        /// Deserialize String To JSON Object
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        /// <summary>
        /// Serialize Object To JSON String Bytes
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToJsonBytes(this object obj)
        {
            return DefaultEncoding.GetBytes(ToJson(obj));
        }

        /// <summary>
        /// Serialize Object To JSON String
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Serialize Object To JSON String
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj, JsonSerializerSettings settings)
        {
            if (settings == null)
                return JsonConvert.SerializeObject(obj);

            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// Serialize Object To JSON String
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="datetimeFormats">IsoDateTimeConverter DateTime Format</param>
        /// <returns></returns>
        public static string ToJson(this object obj, string datetimeFormats)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = datetimeFormats };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }

        #endregion

        #region ToJSONObject/Array

        /// <summary>
        /// JObject To IDictionary[string, object]
        /// JArray To List[object]
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <remarks>
        /// FROM: https://stackoverflow.com/a/51415620
        /// </remarks>
        private static object ToCollections(this object o)
        {
            if (o is JObject jo)
                return jo.ToObject<IDictionary<string, object>>().ToDictionary(k => k.Key, v => ToCollections(v.Value));
            if (o is JArray ja)
                return ja.ToObject<IList<object>>().Select(ToCollections).ToList();
            return o;
        }

        /// <summary>
        /// Object To JSON Object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToJsonObject<T>(this T obj)
            where T : class
        {
            if (obj == null)
                return new Dictionary<string, object>(0);

            return (IDictionary<string, object>)JObject.FromObject(obj).ToCollections();
        }

        /// <summary>
        /// JSON Object String To JSON Object
        /// </summary>
        /// <param name="jsonStr">JSON Object 结构 JSON 字符串, `{} 格式`</param>
        /// <returns></returns>
        public static IDictionary<string, object> ToJsonObject(this string jsonStr)
        {
            if (jsonStr.IsEmpty())
                return new Dictionary<string, object>(0);

            return (IDictionary<string, object>)JObject.Parse(jsonStr).ToCollections();
        }

        /// <summary>
        /// Object To JSON Array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IList<IDictionary<string, object>> ToJsonArray<T>(this IEnumerable<T> obj)
                  where T : class
        {
            if (!obj.Any())
                return new List<IDictionary<string, object>>(0);

            return obj.Select(x => x.ToJsonObject()).ToList();
        }

        /// <summary>
        /// DataTable Object To JSON Array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IList<IDictionary<string, object>> ToJsonArray(this DataTable obj)
        {
            if (obj == null || obj.Rows.Count <= 0)
                return new List<IDictionary<string, object>>(0);

            var outList = (IList<object>)JArray.FromObject(obj).ToCollections();
            return outList.Select(o => (IDictionary<string, object>)o.ToCollections()).ToList();
        }

        /// <summary>
        /// JSON Array String To JSON Array
        /// </summary>
        /// <param name="jsonStr">JSON Array 结构 JSON 字符串, `[] 格式`</param>
        /// <returns></returns>
        public static IList<IDictionary<string, object>> ToJsonArray(this string jsonStr)
        {
            if (jsonStr.IsEmpty())
                return new List<IDictionary<string, object>>(0);

            var outList = (IList<object>)JArray.Parse(jsonStr).ToCollections();
            return outList.Select(o => (IDictionary<string, object>)o.ToCollections()).ToList();
        }

        #endregion
    }
}