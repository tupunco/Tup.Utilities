using System;
using System.Collections.Generic;
using System.Linq;

using Tup.Utilities;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    /// <summary>
    /// Like xUnit Assert Extensions
    /// </summary>
    public static class Assert2
    {
        public static void NotNull(object value)
        {
            Assert.IsNotNull(value);
        }
        public static void Null(object value)
        {
            Assert.IsNull(value);
        }

        public static void Equal(object expected, object actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void Equal<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void Equal<T>(IList<T> expected, IList<T> actual)
        {
            var eq = expected.SequenceEqual(actual, new FuncEqualityComparer<T>((x, y) => object.Equals(x, y)));
            Assert.IsTrue(eq, "Equal");
        }

        public static void NotEqual(object expected, object actual)
        {
            Assert.AreNotEqual(expected, actual);
        }

        public static void NotEqual<T>(T expected, T actual)
        {
            Assert.AreNotEqual(expected, actual);
        }

        public static void Equal(string expected, string actual, StringComparison comparisonType)
        {
            var eq = string.Equals(expected, actual, comparisonType);
            Assert.IsTrue(eq);
        }

        public static void Equal<T>(T expected, T actual, IEqualityComparer<T> equalityComparer)
        {
            Assert.IsNotNull(equalityComparer);

            var eq = equalityComparer.Equals(expected, actual);
            Assert.IsTrue(eq);
        }

        public static void NotEqual(string expected, string actual, StringComparison comparisonType)
        {
            var eq = string.Equals(expected, actual, comparisonType);
            Assert.IsFalse(eq);
        }

        public static void NotEqual<T>(T expected, T actual, IEqualityComparer<T> equalityComparer)
        {
            Assert.IsNotNull(equalityComparer);

            var eq = equalityComparer.Equals(expected, actual);
            Assert.IsFalse(eq);
        }

        public static void Same(object expected, object actual)
        {
            Assert.AreSame(expected, actual);
        }

        public static void NotSame(object expected, object actual)
        {
            var eq = expected == actual;
            Assert.IsFalse(eq, "NotSame");
        }

        /// <summary>
        /// IsInstanceOfType
        /// </summary>
        /// <typeparam name="TExpectedType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TExpectedType IsType<TExpectedType>(object value)
        {
            Assert.IsInstanceOfType(value, typeof(TExpectedType));

            return (TExpectedType)value;
        }

        /// <summary>
        /// is as Assignable
        /// </summary>
        /// <typeparam name="TExpectedType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TExpectedType IsAssignableFrom<TExpectedType>(object value)
        {
            Assert.IsTrue(value is TExpectedType, "IsAssignableFrom");

            return (TExpectedType)value;
        }

        /// <summary>
        /// IsNotNull IsTrue
        /// </summary>
        /// <typeparam name="TExpectedType"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static TExpectedType Single<TExpectedType>(IEnumerable<TExpectedType> values)
        {
            Assert.IsNotNull(values);
            Assert.IsTrue(values.Any());

            return values.FirstOrDefault();
        }

        public static void Empty<TExpectedType>(IEnumerable<TExpectedType> values)
        {
            Assert.IsTrue(values.Count() == 0);
        }

        public static void Collection<TExpectedType>(IEnumerable<TExpectedType> values,
            params Action<TExpectedType>[] assertActs)
        {
            Assert.IsNotNull(values);
            Assert.IsNotNull(assertActs);

            var valuesList = values.ToList();
            Assert.IsTrue(valuesList.Count() >= assertActs.Length);

            var index = 0;
            foreach (var item in values)
            {
                assertActs[index](item);
            }
        }

        public static T Throws<T>(Action action) where T : Exception
        {
            return Assert.ThrowsException<T>(action);
        }

        public static T Throws<T>(Action action, string message) where T : Exception
        {
            return Assert.ThrowsException<T>(action, message);
        }
    }

    public class TheoryData<TValue> : List<TValue>
    {
    }

    public class TheoryData<TKey, TValue> : Dictionary<TKey, TValue>
    {
    }
}