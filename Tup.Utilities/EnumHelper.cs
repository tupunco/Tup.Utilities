using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Tup.Utilities
{
    /// <summary>
    /// EnumHelper
    /// </summary>
    /// <remarks>
    /// FROM: http://www.cnblogs.com/emrys5/p/Enum-rename-htmlhelper.html
    /// </remarks>
    public static class EnumHelper
    {
        /// <summary>
        /// 获取当前枚举值的描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            int order;
            return GetDescription(value, out order);
        }

        /// <summary>
        /// 获取当前枚举值的描述和排序
        /// </summary>
        /// <param name="value"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value, out int order)
        {
            var description = string.Empty;
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            order = 0;
            if (attr != null)
            {
                //order = attr.Order;
                description = attr.Description;
            }
            return description;
        }

        /// <summary>
        /// 获取当前枚举的所有描述
        /// </summary>
        /// <returns></returns>
        public static List<KeyValuePair<int, string>> GetAll<T>()
            where T : IComparable, IFormattable, IConvertible
        {
            return GetAll(typeof(T));
        }

        /// <summary>
        /// 获取所有的枚举描述和值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<KeyValuePair<int, string>> GetAll(Type type)
        {
            var list = new List<KeyValuePair<int, string>>();
            foreach (var field in type.GetFields())
            {
                if (field.FieldType.IsEnum)
                {
                    var tmp = field.GetValue(null);
                    var enumValue = (Enum)tmp;
                    var intValue = Convert.ToInt32(enumValue);
                    var showName = enumValue.GetDescription();
                    list.Add(new KeyValuePair<int, string>(intValue, showName));
                }
            }

            return list;
        }
    }
}