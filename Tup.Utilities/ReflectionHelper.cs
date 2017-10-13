using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Tup.Utilities
{
    /// <summary>
    /// Reflection Helper
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// TypePair
        /// </summary>
        private struct TypePair
        {
            public TypePair(Type targetType, BindingFlags bindingAttr)
            {
                TargetType = targetType;
                BindingAttr = bindingAttr;
            }

            public BindingFlags BindingAttr;
            public Type TargetType;

            public static bool operator ==(TypePair t1, TypePair t2)
            {
                return t1.Equals(t2);
            }

            public static bool operator !=(TypePair t1, TypePair t2)
            {
                return !t1.Equals(t2);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if (!(obj is TypePair))
                    return false;

                var value = (TypePair)obj;
                return value.TargetType == this.TargetType && value.BindingAttr == this.BindingAttr;
            }

            /// <summary>
            ///
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'
                return unchecked(TargetType.GetHashCode() ^ BindingAttr.GetHashCode());
#pragma warning restore RECS0025 // Non-readonly field referenced in 'GetHashCode()'
            }
        }

        #region 扩展

        private static readonly ConcurrentDictionary<TypePair, PropertyInfo[]> s_PropertiesCache
                                           = new ConcurrentDictionary<TypePair, PropertyInfo[]>();

        /// <summary>
        /// Get Properties From Cache
        /// </summary>
        /// <param name="type"></param>
        /// <param name="staticFlags">GetProperties() 默认参数 方式获取</param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertiesFromCache(this Type type, bool staticFlags = false, BindingFlags bindingAttr = BindingFlags.Default)
        {
            ThrowHelper.ThrowIfNull(type, "type");

            BindingFlags flags = BindingFlags.Default;
            if (staticFlags) //GetProperties() 默认参数
                flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
            if (bindingAttr != BindingFlags.Default)
                flags = bindingAttr;
            else
                flags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public;

            return s_PropertiesCache.GetOrAdd(new TypePair(type, flags), t => type.GetProperties(flags));
        }

        #endregion 扩展
    }
}

#if NET_4

namespace System.Reflection
{
    using Collections.Generic;

#pragma warning disable CSE0003 // Use expression-bodied members

    /// <summary>
    /// CustomAttribute Extensions
    /// </summary>
    public static class AttributeExtensions
    {
        #region APIs that return a single attribute

        public static Attribute GetCustomAttribute(this Assembly element, Type attributeType)
        {
            return Attribute.GetCustomAttribute(element, attributeType);
        }

        public static Attribute GetCustomAttribute(this Module element, Type attributeType)
        {
            return Attribute.GetCustomAttribute(element, attributeType);
        }

        public static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType)
        {
            return Attribute.GetCustomAttribute(element, attributeType);
        }

        public static Attribute GetCustomAttribute(this ParameterInfo element, Type attributeType)
        {
            return Attribute.GetCustomAttribute(element, attributeType);
        }

        public static T GetCustomAttribute<T>(this Assembly element) where T : Attribute
        {
            return (T)GetCustomAttribute(element, typeof(T));
        }

        public static T GetCustomAttribute<T>(this Module element) where T : Attribute
        {
            return (T)GetCustomAttribute(element, typeof(T));
        }

        public static T GetCustomAttribute<T>(this MemberInfo element) where T : Attribute
        {
            return (T)GetCustomAttribute(element, typeof(T));
        }

        public static T GetCustomAttribute<T>(this ParameterInfo element) where T : Attribute
        {
            return (T)GetCustomAttribute(element, typeof(T));
        }

        public static Attribute GetCustomAttribute(this MemberInfo element, Type attributeType, bool inherit)
        {
            return Attribute.GetCustomAttribute(element, attributeType, inherit);
        }

        public static Attribute GetCustomAttribute(this ParameterInfo element, Type attributeType, bool inherit)
        {
            return Attribute.GetCustomAttribute(element, attributeType, inherit);
        }

        public static T GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute
        {
            return (T)GetCustomAttribute(element, typeof(T), inherit);
        }

        public static T GetCustomAttribute<T>(this ParameterInfo element, bool inherit) where T : Attribute
        {
            return (T)GetCustomAttribute(element, typeof(T), inherit);
        }

        #endregion

        #region APIs that return all attributes

        public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element)
        {
            return Attribute.GetCustomAttributes(element);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this Module element)
        {
            return Attribute.GetCustomAttributes(element);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element)
        {
            return Attribute.GetCustomAttributes(element);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element)
        {
            return Attribute.GetCustomAttributes(element);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, bool inherit)
        {
            return Attribute.GetCustomAttributes(element, inherit);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, bool inherit)
        {
            return Attribute.GetCustomAttributes(element, inherit);
        }

        #endregion

        #region APIs that return all attributes of a particular type

        public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element, Type attributeType)
        {
            return Attribute.GetCustomAttributes(element, attributeType);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this Module element, Type attributeType)
        {
            return Attribute.GetCustomAttributes(element, attributeType);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType)
        {
            return Attribute.GetCustomAttributes(element, attributeType);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, Type attributeType)
        {
            return Attribute.GetCustomAttributes(element, attributeType);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Assembly element) where T : Attribute
        {
            return (IEnumerable<T>)GetCustomAttributes(element, typeof(T));
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Module element) where T : Attribute
        {
            return (IEnumerable<T>)GetCustomAttributes(element, typeof(T));
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element) where T : Attribute
        {
            return (IEnumerable<T>)GetCustomAttributes(element, typeof(T));
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element) where T : Attribute
        {
            return (IEnumerable<T>)GetCustomAttributes(element, typeof(T));
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, Type attributeType, bool inherit)
        {
            return Attribute.GetCustomAttributes(element, attributeType, inherit);
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element, Type attributeType, bool inherit)
        {
            return Attribute.GetCustomAttributes(element, attributeType, inherit);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element, bool inherit) where T : Attribute
        {
            return (IEnumerable<T>)GetCustomAttributes(element, typeof(T), inherit);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element, bool inherit) where T : Attribute
        {
            return (IEnumerable<T>)GetCustomAttributes(element, typeof(T), inherit);
        }

        #endregion

        #region IsDefined

        public static bool IsDefined(this Assembly element, Type attributeType)
        {
            return Attribute.IsDefined(element, attributeType);
        }

        public static bool IsDefined(this Module element, Type attributeType)
        {
            return Attribute.IsDefined(element, attributeType);
        }

        public static bool IsDefined(this MemberInfo element, Type attributeType)
        {
            return Attribute.IsDefined(element, attributeType);
        }

        public static bool IsDefined(this ParameterInfo element, Type attributeType)
        {
            return Attribute.IsDefined(element, attributeType);
        }

        public static bool IsDefined(this MemberInfo element, Type attributeType, bool inherit)
        {
            return Attribute.IsDefined(element, attributeType, inherit);
        }

        public static bool IsDefined(this ParameterInfo element, Type attributeType, bool inherit)
        {
            return Attribute.IsDefined(element, attributeType, inherit);
        }

        #endregion
    }

#pragma warning restore CSE0003 // Use expression-bodied members
}

#endif