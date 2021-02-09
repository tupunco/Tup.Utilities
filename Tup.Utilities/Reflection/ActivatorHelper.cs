using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Tup.Utilities
{
    /// <summary>
    /// 类型 Activator 实例化 Helper
    /// </summary>
    /// <remarks>
    /// 自动创建 CreateInstanceFactory 并缓存
    /// FROM: https://agileobjects.co.uk/create-instance-of-type-net-core
    ///  https://github.com/agileobjects/eg-create-instance-from-type/blob/master/CreateInstanceFromType/CreateInstanceFromType2020DesignTimeArgs.cs
    ///  https://github.com/agileobjects/eg-create-instance-from-type/blob/master/CreateInstanceFromType/CreateInstanceFromType2012.cs
    /// </remarks>
    public static class ActivatorHelper
    {
        #region CreateInstance

        /// <summary>
        /// 使用无参数构造函数, 创建指定泛型类型参数所指定类型的 `实例`
        /// <see cref="Activator.CreateInstance{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstance<T>()
        {
            return (T)GetInstance(typeof(T));
        }

        /// <summary>
        /// 使用 `指定类型` 的默认构造函数来创建该类型的 `实例`
        /// <see cref="Activator.CreateInstance(Type)"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateInstance(Type type)
        {
            return GetInstance(type);
        }

        #endregion

        #region CreateInstanceFactory

        /// <summary>
        /// 使用无参数构造函数, 创建指定泛型类型参数所指定类型的 `实例工厂`
        /// <see cref="Activator.CreateInstance{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Func<T> CreateInstanceFactory<T>()
        {
            var f = GetInstanceFactory(typeof(T));
            return () => (T)f();
        }

        /// <summary>
        /// 使用 `指定类型` 的默认构造函数来创建该类型的 `实例工厂`
        /// <see cref="Activator.CreateInstance(Type)"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Func<object> CreateInstanceFactory(Type type)
        {
            return GetInstanceFactory(type);
        }

        #endregion

        #region GetInstance/GetInstanceFactory

        /// <summary>
        /// Returns an instance of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type an instance of which to create.</param>
        /// <returns>An instance of this <paramref name="type"/>.</returns>
        public static object GetInstance(this Type type)
            => InstanceFactoryCache.GetFactoryFor(type).Invoke();

        /// <summary>
        /// Returns an instance of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type an instance of which to create.</param>
        /// <returns>An instance of this <paramref name="type"/>.</returns>
        public static Func<object> GetInstanceFactory(this Type type)
            => InstanceFactoryCache.GetFactoryFor(type);

        /// <summary>
        /// Returns an instance of this <paramref name="type"/> using its single-parameter constructor
        /// and the given <paramref name="argument"/>.
        /// </summary>
        /// <typeparam name="TArg">
        /// The type of the single argument to pass to the constructor.
        /// </typeparam>
        /// <param name="type">The type an instance of which to create.</param>
        /// <param name="argument">The single argument to pass to the constructor.</param>
        /// <returns>An instance of this <paramref name="type"/>.</returns>
        public static object GetInstance<TArg>(
            this Type type,
            TArg argument)
        {
            return InstanceFactoryCache<TArg>.GetFactoryFor(type).Invoke(argument);
        }

        /// <summary>
        /// Returns an instance of this <paramref name="type"/> using its two-parameter constructor
        /// and the given <paramref name="argument1"/> and <paramref name="argument2"/>.
        /// </summary>
        /// <typeparam name="TArg1">
        /// The type of the first argument to pass to the constructor.
        /// </typeparam>
        /// <typeparam name="TArg2">
        /// The type of the second argument to pass to the constructor.
        /// </typeparam>
        /// <param name="type">The type an instance of which to create.</param>
        /// <param name="argument1">The first argument to pass to the constructor.</param>
        /// <param name="argument2">The second argument to pass to the constructor.</param>
        /// <returns>An instance of this <paramref name="type"/>.</returns>
        public static object GetInstance<TArg1, TArg2>(
            this Type type,
            TArg1 argument1,
            TArg2 argument2)
        {
            return InstanceFactoryCache<TArg1, TArg2>.GetFactoryFor(type)
                .Invoke(argument1, argument2);
        }

        /// <summary>
        /// Returns an instance of this <paramref name="type"/> using its two-parameter constructor
        /// and the given <paramref name="argument1"/>, <paramref name="argument2"/> and
        /// <paramref name="argument3"/>.
        /// </summary>
        /// <typeparam name="TArg1">
        /// The type of the first argument to pass to the constructor.
        /// </typeparam>
        /// <typeparam name="TArg2">
        /// The type of the second argument to pass to the constructor.
        /// </typeparam>
        /// <typeparam name="TArg3">
        /// The type of the third argument to pass to the constructor.
        /// </typeparam>
        /// <param name="type">The type an instance of which to create.</param>
        /// <param name="argument1">The first argument to pass to the constructor.</param>
        /// <param name="argument2">The second argument to pass to the constructor.</param>
        /// <param name="argument3">The third argument to pass to the constructor.</param>
        /// <returns>An instance of this <paramref name="type"/>.</returns>
        public static object GetInstance<TArg1, TArg2, TArg3>(
            this Type type,
            TArg1 argument1,
            TArg2 argument2,
            TArg3 argument3)
        {
            return InstanceFactoryCache<TArg1, TArg2, TArg3>.GetFactoryFor(type)
                .Invoke(argument1, argument2, argument3);
        }

        #endregion

        #region InstanceFactoryCache

        private static class InstanceFactoryCache
        {
            // A dictionary of object-creation Funcs, keyed by the created type:
            private static readonly ConcurrentDictionary<Type, Func<object>> _factoriesByType =
                new ConcurrentDictionary<Type, Func<object>>();

            public static Func<object> GetFactoryFor(Type type)
            {
                return _factoriesByType.GetOrAdd(type, t =>
                {
                    // The default, parameterless constructor:
                    var ctor = t.GetConstructor(Type.EmptyTypes);

                    // An Expression representing the default constructor call:
                    var instanceCreation = Expression.New(ctor);

                    // Compile the Expression into a Func which returns the
                    // constructed object:
                    var instanceCreationLambda = Expression
                        .Lambda<Func<object>>(instanceCreation);

                    return instanceCreationLambda.Compile();
                });
            }
        }

        private static class InstanceFactoryCache<TArg>
        {
            // A dictionary of object-creation Funcs, keyed by the created type:
#pragma warning disable RECS0108 // Warns about static fields in generic types

            private static readonly ConcurrentDictionary<Type, Func<TArg, object>> _factoriesByType =
                new ConcurrentDictionary<Type, Func<TArg, object>>();

#pragma warning restore RECS0108 // Warns about static fields in generic types

            public static Func<TArg, object> GetFactoryFor(Type type)
            {
                return _factoriesByType.GetOrAdd(type, t =>
                {
                    // The argument type:
                    var argType = typeof(TArg);

                    // The matching constructor:
                    var ctor = t.GetConstructor(new[] { argType });

                    // An Expression representing the parameter to
                    // pass to the Func and constructor:
                    var argument = Expression.Parameter(argType, "param");

                    // An Expression representing the constructor call,
                    // passing in the constructor parameter:
                    var instanceCreation = Expression.New(ctor, argument);

                    // Compile the Expression into a Func which takes one
                    // argument and returns the constructed object:
                    var instanceCreationLambda = Expression
                .Lambda<Func<TArg, object>>(instanceCreation, argument);

                    return instanceCreationLambda.Compile();
                });
            }
        }

        private static class InstanceFactoryCache<TArg1, TArg2>
        {
            // A dictionary of object-creation Funcs, keyed by the created type:
#pragma warning disable RECS0108 // Warns about static fields in generic types

            private static readonly ConcurrentDictionary<Type, Func<TArg1, TArg2, object>> _factoriesByType =
                new ConcurrentDictionary<Type, Func<TArg1, TArg2, object>>();

#pragma warning restore RECS0108 // Warns about static fields in generic types

            public static Func<TArg1, TArg2, object> GetFactoryFor(Type type)
            {
                return _factoriesByType.GetOrAdd(type, t =>
                {
                    // The argument types:
                    var arg1Type = typeof(TArg1);
                    var arg2Type = typeof(TArg2);

                    // The matching constructor:
                    var ctor = t.GetConstructor(new[] { arg1Type, arg2Type });

                    // A set of Expressions representing the parameters to
                    // pass to the Func and constructor:
                    var argument1 = Expression.Parameter(arg1Type, "param1");
                    var argument2 = Expression.Parameter(arg2Type, "param2");

                    // An Expression representing the constructor call,
                    // passing in the constructor parameters:
                    var instanceCreation = Expression
                .New(ctor, argument1, argument2);

                    // Compile the Expression into a Func which takes two
                    // arguments and returns the constructed object:
                    var instanceCreationLambda = Expression
                .Lambda<Func<TArg1, TArg2, object>>(instanceCreation, argument1, argument2);

                    return instanceCreationLambda.Compile();
                });
            }
        }

        private static class InstanceFactoryCache<TArg1, TArg2, TArg3>
        {
            // A dictionary of object-creation Funcs, keyed by the created type:
#pragma warning disable RECS0108 // Warns about static fields in generic types

            private static readonly ConcurrentDictionary<Type, Func<TArg1, TArg2, TArg3, object>> _factoriesByType =
                new ConcurrentDictionary<Type, Func<TArg1, TArg2, TArg3, object>>();

#pragma warning restore RECS0108 // Warns about static fields in generic types

            public static Func<TArg1, TArg2, TArg3, object> GetFactoryFor(Type type)
            {
                return _factoriesByType.GetOrAdd(type, t =>
                {
                    // The argument types:
                    var arg1Type = typeof(TArg1);
                    var arg2Type = typeof(TArg2);
                    var arg3Type = typeof(TArg3);

                    // The matching constructor:
                    var ctor = t.GetConstructor(new[] { arg1Type, arg2Type, arg3Type });

                    // A set of Expressions representing the parameters to
                    // pass to the Func and constructor:
                    var argument1 = Expression.Parameter(arg1Type, "param1");
                    var argument2 = Expression.Parameter(arg2Type, "param2");
                    var argument3 = Expression.Parameter(arg3Type, "param3");

                    // An Expression representing the constructor call,
                    // passing in the constructor parameters:
                    var instanceCreation = Expression
                .New(ctor, argument1, argument2, argument3);

                    // Compile the Expression into a Func which takes three
                    // arguments and returns the constructed object:
                    var instanceCreationLambda = Expression
                .Lambda<Func<TArg1, TArg2, TArg3, object>>(
                    instanceCreation, argument1, argument2, argument3);

                    return instanceCreationLambda.Compile();
                });
            }
        }

        #endregion
    }
}