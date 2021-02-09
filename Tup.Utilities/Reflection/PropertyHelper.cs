using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Tup.Utilities
{
    /// <summary>
    /// 快速属性反射 工具类
    /// Fast Property Reflection Helper
    /// </summary>
    /// <remarks>
    /// FROM: https://raw.githubusercontent.com/dotnet/aspnetcore/v3.1.10/src/Shared/PropertyHelper/PropertyHelper.cs
    /// </remarks>
    public static class PropertyHelper
    {
        // Delegate type for a by-ref property getter
        private delegate TValue ByRefFunc<TDeclaringType, TValue>(ref TDeclaringType arg);

        private static readonly MethodInfo CallPropertyGetterOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter));

        private static readonly MethodInfo CallPropertyGetterByReferenceOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetterByReference));

        private static readonly MethodInfo CallNullSafePropertyGetterOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallNullSafePropertyGetter));

        private static readonly MethodInfo CallNullSafePropertyGetterByReferenceOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallNullSafePropertyGetterByReference));

        private static readonly MethodInfo CallPropertySetterOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertySetter));

        // Using an array rather than IEnumerable, as target will be called on the hot path numerous times.
        private static readonly ConcurrentDictionary<Type, PropertyAccessor[]> PropertyAccessorsCache =
            new ConcurrentDictionary<Type, PropertyAccessor[]>();

        /// <summary>
        /// Property Setter/Getter Accessor
        /// </summary>
        public class PropertyAccessor
        {
            private Action<object, object> _valueSetter;
            private Func<object, object> _valueGetter;

            /// <summary>
            /// Initializes a fast <see cref="PropertyHelper"/>.
            /// This constructor does not cache the helper. For caching, use <see cref="GetPropertyAccessors(Type, bool)"/>.
            /// </summary>
            public PropertyAccessor(PropertyInfo property)
            {
                Property = property ?? throw new ArgumentNullException(nameof(property));
                Name = property.Name;
            }

            /// <summary>
            /// Gets the backing <see cref="PropertyInfo"/>.
            /// </summary>
            public PropertyInfo Property { get; }

            /// <summary>
            /// Gets the backing <see cref="PropertyInfo"/>.
            /// </summary>
            public Type PropertyType
            {
                get { return Property.PropertyType; }
            }

            /// <summary>
            /// 获取一个值，该值指示此属性是否可写。
            /// </summary>
            public bool CanWrite
            {
                get { return Property.CanWrite; }
            }

            /// <summary>
            ///  获取一个值，该值指示此属性是否可读。
            /// </summary>
            public bool CanRead
            {
                get { return Property.CanRead; }
            }

            /// <summary>
            /// Gets (or sets in derived types) the property name.
            /// </summary>
            public string Name { get; protected set; }

            /// <summary>
            /// Gets the property value getter.
            /// </summary>
            public Func<object, object> ValueGetter
            {
                get
                {
                    if (_valueGetter == null)
                    {
                        _valueGetter = MakeFastPropertyGetter(Property);
                    }

                    return _valueGetter;
                }
            }

            /// <summary>
            /// Gets the property value setter.
            /// </summary>
            public Action<object, object> ValueSetter
            {
                get
                {
                    if (_valueSetter == null)
                    {
                        _valueSetter = MakeFastPropertySetter(Property);
                    }

                    return _valueSetter;
                }
            }

            /// <summary>
            /// Returns the property value for the specified <paramref name="instance"/>.
            /// </summary>
            /// <param name="instance">The object whose property value will be returned.</param>
            /// <returns>The property value.</returns>
            public object GetValue(object instance)
            {
                return ValueGetter(instance);
            }

#pragma warning disable RECS0154 // Parameter is never used

            /// <summary>
            /// Returns the property value for the specified <paramref name="instance"/>.
            /// </summary>
            /// <param name="instance">The object whose property value will be returned.</param>
            /// <param name="index">占位符</param>
            /// <returns>The property value.</returns>
            public object GetValue(object instance, object[] index)
#pragma warning restore RECS0154 // Parameter is never used
            {
                return ValueGetter(instance);
            }

            /// <summary>
            /// Sets the property value for the specified <paramref name="instance" />.
            /// </summary>
            /// <param name="instance">The object whose property value will be set.</param>
            /// <param name="value">The property value.</param>
            public void SetValue(object instance, object value)
            {
                ValueSetter(instance, value);
            }

#pragma warning disable RECS0154 // Parameter is never used

            /// <summary>
            /// Sets the property value for the specified <paramref name="instance" />.
            /// </summary>
            /// <param name="instance">The object whose property value will be set.</param>
            /// <param name="value">The property value.</param>
            /// <param name="index">占位符</param>
            public void SetValue(object instance, object value, object[] index)
#pragma warning restore RECS0154 // Parameter is never used
            {
                ValueSetter(instance, value);
            }
        }

        #region MakeFastPropertySetter/Getter

        /// <summary>
        /// Creates a single fast property getter. The result is not cached.
        /// </summary>
        /// <param name="propertyInfo">propertyInfo to extract the getter for.</param>
        /// <returns>a fast getter.</returns>
        /// <remarks>
        /// This method is more memory efficient than a dynamically compiled lambda, and about the
        /// same speed.
        /// </remarks>
        public static Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            ThrowHelper.ThrowIfNull(propertyInfo, "propertyInfo");

            return MakeFastPropertyGetter(
                propertyInfo,
                CallPropertyGetterOpenGenericMethod,
                CallPropertyGetterByReferenceOpenGenericMethod);
        }

        /// <summary>
        /// Creates a single fast property getter which is safe for a null input object. The result is not cached.
        /// </summary>
        /// <param name="propertyInfo">propertyInfo to extract the getter for.</param>
        /// <returns>a fast getter.</returns>
        /// <remarks>
        /// This method is more memory efficient than a dynamically compiled lambda, and about the
        /// same speed.
        /// </remarks>
        public static Func<object, object> MakeNullSafeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            ThrowHelper.ThrowIfNull(propertyInfo, "propertyInfo");

            return MakeFastPropertyGetter(
                propertyInfo,
                CallNullSafePropertyGetterOpenGenericMethod,
                CallNullSafePropertyGetterByReferenceOpenGenericMethod);
        }

        private static Func<object, object> MakeFastPropertyGetter(
            PropertyInfo propertyInfo,
            MethodInfo propertyGetterWrapperMethod,
            MethodInfo propertyGetterByRefWrapperMethod)
        {
            ThrowHelper.ThrowIfNull(propertyInfo, "propertyInfo");
            ThrowHelper.ThrowIfNull(propertyGetterWrapperMethod, "propertyGetterWrapperMethod");
            ThrowHelper.ThrowIfNull(propertyGetterByRefWrapperMethod, "propertyGetterByRefWrapperMethod");

            // Must be a generic method with a Func<,> parameter
            Debug.Assert(propertyGetterWrapperMethod.IsGenericMethodDefinition);
            Debug.Assert(propertyGetterWrapperMethod.GetParameters().Length == 2);

            // Must be a generic method with a ByRefFunc<,> parameter
            Debug.Assert(propertyGetterByRefWrapperMethod.IsGenericMethodDefinition);
            Debug.Assert(propertyGetterByRefWrapperMethod.GetParameters().Length == 2);

            var getMethod = propertyInfo.GetMethod;
            ThrowHelper.ThrowIfNull(getMethod, "getMethod");

            Debug.Assert(!getMethod.IsStatic);
            Debug.Assert(getMethod.GetParameters().Length == 0);

            // Instance methods in the CLR can be turned into static methods where the first parameter
            // is open over "target". This parameter is always passed by reference, so we have a code
            // path for value types and a code path for reference types.
            if (getMethod.DeclaringType.GetTypeInfo().IsValueType)
            {
                // Create a delegate (ref TDeclaringType) -> TValue
                return MakeFastPropertyGetter(
                    typeof(ByRefFunc<,>),
                    getMethod,
                    propertyGetterByRefWrapperMethod);
            }
            else
            {
                // Create a delegate TDeclaringType -> TValue
                return MakeFastPropertyGetter(
                    typeof(Func<,>),
                    getMethod,
                    propertyGetterWrapperMethod);
            }
        }

        private static Func<object, object> MakeFastPropertyGetter(
            Type openGenericDelegateType,
            MethodInfo propertyGetMethod,
            MethodInfo openGenericWrapperMethod)
        {
            var typeInput = propertyGetMethod.DeclaringType;
            var typeOutput = propertyGetMethod.ReturnType;

            var delegateType = openGenericDelegateType.MakeGenericType(typeInput, typeOutput);
            var propertyGetterDelegate = propertyGetMethod.CreateDelegate(delegateType);

            var wrapperDelegateMethod = openGenericWrapperMethod.MakeGenericMethod(typeInput, typeOutput);
            var accessorDelegate = wrapperDelegateMethod.CreateDelegate(
                typeof(Func<object, object>),
                propertyGetterDelegate);

            return (Func<object, object>)accessorDelegate;
        }

        /// <summary>
        /// Creates a single fast property setter for reference types. The result is not cached.
        /// </summary>
        /// <param name="propertyInfo">propertyInfo to extract the setter for.</param>
        /// <returns>a fast getter.</returns>
        /// <remarks>
        /// This method is more memory efficient than a dynamically compiled lambda, and about the
        /// same speed. This only works for reference types.
        /// </remarks>
        public static Action<object, object> MakeFastPropertySetter(PropertyInfo propertyInfo)
        {
            ThrowHelper.ThrowIfNull(propertyInfo, "propertyInfo");

            Debug.Assert(!propertyInfo.DeclaringType.GetTypeInfo().IsValueType);

            var setMethod = propertyInfo.SetMethod;
            ThrowHelper.ThrowIfNull(setMethod, "setMethod");

            Debug.Assert(!setMethod.IsStatic);
            Debug.Assert(setMethod.ReturnType == typeof(void));
            var parameters = setMethod.GetParameters();
            Debug.Assert(parameters.Length == 1);

            // Instance methods in the CLR can be turned into static methods where the first parameter
            // is open over "target". This parameter is always passed by reference, so we have a code
            // path for value types and a code path for reference types.
            var typeInput = setMethod.DeclaringType;
            var parameterType = parameters[0].ParameterType;

            // Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
            var propertySetterAsAction =
                setMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, parameterType));
            var callPropertySetterClosedGenericMethod =
                CallPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, parameterType);
            var callPropertySetterDelegate =
                callPropertySetterClosedGenericMethod.CreateDelegate(
                    typeof(Action<object, object>), propertySetterAsAction);

            return (Action<object, object>)callPropertySetterDelegate;
        }

        #endregion

        /// <summary>
        /// Given an object, adds each instance property with a public get method as a key and its
        /// associated value to a dictionary.
        ///
        /// If the object is already an <see cref="IDictionary{String, Object}"/> instance, then a copy
        /// is returned.
        /// </summary>
        /// <remarks>
        /// The implementation of PropertyHelper will cache the property accessors per-type. This is
        /// faster when the same type is used multiple times with ObjectToDictionary.
        /// </remarks>
        public static IDictionary<string, object> ObjectToDictionary(object value)
        {
            var dictionary = value as IDictionary<string, object>;
            if (dictionary != null)
            {
                return new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
            }

            dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (value != null)
            {
                foreach (var accessor in GetPropertyAccessors(value.GetType()))
                {
                    dictionary[accessor.Name] = accessor.GetValue(value);
                }
            }

            return dictionary;
        }

        #region CallPropertyXXXXX

        // Called via reflection
        private static object CallPropertyGetter<TDeclaringType, TValue>(
            Func<TDeclaringType, TValue> getter,
            object target)
        {
            return getter((TDeclaringType)target);
        }

        // Called via reflection
        private static object CallPropertyGetterByReference<TDeclaringType, TValue>(
            ByRefFunc<TDeclaringType, TValue> getter,
            object target)
        {
            var unboxed = (TDeclaringType)target;
            return getter(ref unboxed);
        }

        // Called via reflection
        private static object CallNullSafePropertyGetter<TDeclaringType, TValue>(
            Func<TDeclaringType, TValue> getter,
            object target)
        {
            if (target == null)
            {
                return null;
            }

            return getter((TDeclaringType)target);
        }

        // Called via reflection
        private static object CallNullSafePropertyGetterByReference<TDeclaringType, TValue>(
            ByRefFunc<TDeclaringType, TValue> getter,
            object target)
        {
            if (target == null)
            {
                return null;
            }

            var unboxed = (TDeclaringType)target;
            return getter(ref unboxed);
        }

        private static void CallPropertySetter<TDeclaringType, TValue>(
            Action<TDeclaringType, TValue> setter,
            object target,
            object value)
        {
            setter((TDeclaringType)target, (TValue)value);
        }

        #endregion

        #region GetPropertyAccessorXXXXX

        /// <summary>
        /// Create PropertyAccessor Instance
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static PropertyAccessor CreateAccessorInstance(PropertyInfo property)
        {
            return new PropertyAccessor(property);
        }

        /// <summary>
        /// Creates and caches fast property accessors that expose getters for every public get property on the
        /// underlying type.
        /// </summary>
        /// <param name="typeInfo">The type info to extract property accessors for.</param>
        /// <param name="staticFlags">GetProperties() 默认参数 方式获取</param>
        /// <returns>A cached array of all public properties of the specified type.
        /// </returns>
        public static PropertyAccessor[] GetPropertyAccessors(this TypeInfo typeInfo, bool staticFlags = false)
        {
            return GetPropertyAccessors(typeInfo.AsType(), staticFlags);
        }

        /// <summary>
        /// Creates and caches fast property accessors that expose getters for every public get property on the
        /// specified type.
        /// </summary>
        /// <param name="type">The type to extract property accessors for.</param>
        /// <param name="staticFlags">GetProperties() 默认参数 方式获取</param>
        /// <returns>A cached array of all public properties of the specified type.
        /// </returns>
        public static PropertyAccessor[] GetPropertyAccessors(this Type type, bool staticFlags = false)
        {
            return InternalGetPropertyAccessors(type, p => CreateAccessorInstance(p), PropertyAccessorsCache, staticFlags);
        }

        /// <summary>
        /// Get PropertyAccessors Map From Cache
        /// </summary>
        /// <param name="type"></param>
        /// <param name="staticFlags">GetProperties() 默认参数 方式获取</param>
        /// <returns></returns>
        public static IDictionary<string, PropertyAccessor> GetPropertyAccessorsMap(this Type type, bool staticFlags = false)
        {
            var properties = type.GetPropertyAccessors(staticFlags);
            return properties.ToDictionary(x => x.Name, x => x);
        }

        /// <summary>
        /// GetPropertyAccessor From PropertyInfo Map
        /// </summary>
        /// <param name="propertiesMap"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyAccessor GetPropertyAccessor(this IDictionary<string, PropertyAccessor> propertiesMap, string name)
        {
            ThrowHelper.ThrowIfNull(propertiesMap, "propertiesMap");
            ThrowHelper.ThrowIfNull(name, "name");

            return propertiesMap.GetValue(name);
        }

        /// <summary>
        /// GetPropertyAccessor From PropertyInfo Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyAccessor GetPropertyAccessor(this Type type, string name)
        {
            ThrowHelper.ThrowIfNull(type, "propertyType");
            ThrowHelper.ThrowIfNull(name, "name");

            var properties = type.GetPropertyAccessors();
            return properties.FirstOrDefault2(x => name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Internal GetPropertyAccessors
        /// </summary>
        /// <param name="type"></param>
        /// <param name="createPropertyHelper"></param>
        /// <param name="cache"></param>
        /// <param name="staticFlags">GetProperties() 默认参数 方式获取</param>
        /// <returns></returns>
        private static PropertyAccessor[] InternalGetPropertyAccessors(
            Type type,
            Func<PropertyInfo, PropertyAccessor> createPropertyHelper,
            ConcurrentDictionary<Type, PropertyAccessor[]> cache,
            bool staticFlags = false)
        {
            // Unwrap nullable types. This means Nullable<T>.Value and Nullable<T>.HasValue will not be
            // part of the sequence of properties returned by this method.
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (!cache.TryGetValue(type, out var accessors))
            {
                var properties = type.GetPropertiesFromCache(staticFlags);
                accessors = properties.Select(p => createPropertyHelper(p)).ToArray();
                cache.TryAdd(type, accessors);
            }

            return accessors;
        }

        #endregion
    }
}