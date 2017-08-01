using System;

namespace Tup.Utilities
{
    /// <summary>
    ///     讲一个对象序列化为一个Json字符串，或者将一个Json字符串序列化为一个对象
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        ///     Deserialize Object
        /// </summary>
        /// <param name="s"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object DeserializeObject(string s, Type targetType)
        {
            //TODO DeserializeObject
            //return JsonConvert.DeserializeObject(s, targetType);
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Serialize Object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject(object obj)
        {
            //TODO SerializeObject
            //return JsonConvert.SerializeObject(obj);
            throw new NotImplementedException();
        }
    }
}
