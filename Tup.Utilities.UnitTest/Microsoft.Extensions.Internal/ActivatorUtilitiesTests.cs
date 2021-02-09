// FROM: https://github.com/dotnet/runtime/blob/v5.0.0/src/libraries/Microsoft.Extensions.DependencyInjection/tests/DI.Specification.Tests/ActivatorUtilitiesTests.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Extensions.Internal.Tests
{
    [TestClass]
    public class ActivatorUtilitiesTests
    {
        public delegate object CreateInstanceFunc(IServiceProvider provider, Type type, object[] args);

        private static object CreateInstanceDirectly(IServiceProvider provider, Type type, object[] args)
        {
            return ActivatorUtilities.CreateInstance(provider, type, args);
        }

        private static object CreateInstanceFromFactory(IServiceProvider provider, Type type, object[] args)
        {
            var factory = ActivatorUtilities.CreateFactory(type, args.Select(a => a.GetType()).ToArray());
            return factory(provider, args);
        }

        private static T CreateInstance<T>(CreateInstanceFunc func, IServiceProvider provider, params object[] args)
        {
            return (T)func(provider, typeof(T), args);
        }

        public static IEnumerable<object[]> CreateInstanceFuncs
        {
            get
            {
                yield return new[] { (CreateInstanceFunc)CreateInstanceDirectly };
                yield return new[] { (CreateInstanceFunc)CreateInstanceFromFactory };
            }
        }

        #region TypeActivatorWorksWithStaticCtor

        [TestMethod]
        public void TypeActivatorWorksWithStaticCtor()
        {
            foreach (var item in CreateInstanceFuncs)
            {
                TypeActivatorWorksWithStaticCtor_Theory((CreateInstanceFunc)item.FirstOrDefault());
            }
        }

        //[Theory]
        //[MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorWorksWithStaticCtor_Theory(CreateInstanceFunc createFunc)
        {
            // Act
            var anotherClass = CreateInstance<ClassWithStaticCtor>(createFunc, provider: null);

            // Assert
            Assert2.NotNull(anotherClass);
        }

        #endregion

        #region TypeActivatorRequiresPublicConstructor

        public static IEnumerable<object[]> TypesWithNonPublicConstructorData =>
          CreateInstanceFuncs.Zip(
                  new[]
                  {
                       typeof(ClassWithPrivateCtor),
                       typeof(ClassWithInternalConstructor),
                       typeof(ClassWithProtectedConstructor),
                       typeof(StaticConstructorClass)
                  },
                  (a, b) => new object[] { a[0], b });

        [TestMethod]
        public void TypeActivatorRequiresPublicConstructor()
        {
            foreach (var item in TypesWithNonPublicConstructorData)
            {
                TypeActivatorRequiresPublicConstructor_Theory((CreateInstanceFunc)item[0], (Type)item[1]);
            }
        }

        //[Theory]
        //[MemberData(nameof(TypesWithNonPublicConstructorData))]
        public void TypeActivatorRequiresPublicConstructor_Theory(CreateInstanceFunc createFunc, Type type)
        {
            // Arrange
            var expectedMessage = $"A suitable constructor for type '{type}' could not be located. " +
                "Ensure the type is concrete and services are registered for all parameters of a public constructor.";

            // Act and Assert
            var ex = Assert2.Throws<InvalidOperationException>(() =>
                createFunc(provider: null, type: type, args: new object[0]));

            Assert2.Equal(expectedMessage, ex.Message);
        }

        #endregion

        [TestMethod]
        public void CreateInstance_WithAbstractTypeAndPublicConstructor_ThrowsCorrectException()
        {
            // Act & Assert
            var ex = Assert2.Throws<InvalidOperationException>(() => ActivatorUtilities.CreateInstance(default(IServiceProvider), typeof(AbstractFoo)));
            //var msg = "A suitable constructor for type 'Microsoft.Extensions.DependencyInjection.Specification.DependencyInjectionSpecificationTests+AbstractFoo' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.";
            Assert2.NotNull(ex.Message);
        }

        [TestMethod]
        public void CreateInstance_CapturesInnerException_OfTargetInvocationException()
        {
            // Act & Assert
            var ex = Assert2.Throws<InvalidOperationException>(() => ActivatorUtilities.CreateInstance(default(IServiceProvider), typeof(Bar)));
            var msg = "some error";
            Assert2.Equal(msg, ex.Message);
        }

        private abstract class AbstractFoo
        {
            // The constructor should be public, since that is checked as well.
            public AbstractFoo()
            {
            }
        }

        private class Bar
        {
            public Bar()
            {
                throw new InvalidOperationException("some error");
            }
        }

        private class StaticConstructorClass
        {
            static StaticConstructorClass()
            {
            }

            private StaticConstructorClass()
            {
            }
        }

        public class ClassWithStaticCtor
        {
            static ClassWithStaticCtor()
            {
            }
        }

        public class ClassWithPrivateCtor
        {
            private ClassWithPrivateCtor()
            {
            }
        }

        public class ClassWithInternalConstructor
        {
            internal ClassWithInternalConstructor()
            {
            }
        }

        public class ClassWithProtectedConstructor
        {
            internal ClassWithProtectedConstructor()
            {
            }
        }
    }
}