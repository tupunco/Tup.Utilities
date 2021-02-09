﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tup.Utilities.Tests
{
    /// <summary>
    /// FROM: https://github.com/dotnet/aspnetcore/blob/v3.1.11/src/Shared/test/Shared.Tests/PropertyHelperTest.cs
    /// </summary>
    [TestClass()]
    public class PropertyHelperTests
    {
        [TestMethod]
        public void PropertyHelper_ReturnsNameCorrectly()
        {
            // Arrange
            var anonymous = new { foo = "bar" };
            var property = PropertyHelper.GetPropertyAccessors(anonymous.GetType()).First().Property;

            // Act
            var helper = new PropertyHelper.PropertyAccessor(property);

            // Assert
            Assert.AreEqual("foo", property.Name);
            Assert.AreEqual("foo", helper.Name);
        }

        [TestMethod]
        public void PropertyHelper_ReturnsValueCorrectly()
        {
            // Arrange
            var anonymous = new { bar = "baz" };
            var property = PropertyHelper.GetPropertyAccessors(anonymous.GetType()).First().Property;

            // Act
            var helper = new PropertyHelper.PropertyAccessor(property);

            // Assert
            Assert.AreEqual("bar", helper.Name);
            Assert.AreEqual("baz", helper.GetValue(anonymous));
        }

        [TestMethod]
        public void PropertyHelper_ReturnsGetterDelegate()
        {
            // Arrange
            var anonymous = new { bar = "baz" };
            var property = PropertyHelper.GetPropertyAccessors(anonymous.GetType()).First().Property;

            // Act
            var helper = new PropertyHelper.PropertyAccessor(property);

            // Assert
            Assert.IsNotNull(helper.ValueGetter);
            Assert.AreEqual("baz", helper.ValueGetter(anonymous));
        }

        [TestMethod]
        public void SetValue_SetsPropertyValue()
        {
            // Arrange
            var expected = "new value";
            var instance = new BaseClass { PropA = "old value" };
            var helper = PropertyHelper.GetPropertyAccessors(
                instance.GetType()).First(prop => prop.Name == "PropA");

            // Act
            helper.SetValue(instance, expected);

            // Assert
            Assert.AreEqual(expected, instance.PropA);
        }

        [TestMethod]
        public void PropertyHelper_ReturnsSetterDelegate()
        {
            // Arrange
            var expected = "new value";
            var instance = new BaseClass { PropA = "old value" };
            var helper = PropertyHelper.GetPropertyAccessors(
                instance.GetType()).First(prop => prop.Name == "PropA");

            // Act and Assert
            Assert.IsNotNull(helper.ValueSetter);
            helper.ValueSetter(instance, expected);

            // Assert
            Assert.AreEqual(expected, instance.PropA);
        }

        [TestMethod]
        public void PropertyHelper_ReturnsValueCorrectly_ForValueTypes()
        {
            // Arrange
            var anonymous = new { foo = 32 };
            var property = PropertyHelper.GetPropertyAccessors(anonymous.GetType()).First().Property;

            // Act
            var helper = new PropertyHelper.PropertyAccessor(property);

            // Assert
            Assert.AreEqual("foo", helper.Name);
            Assert.AreEqual(32, helper.GetValue(anonymous));
        }

        [TestMethod]
        public void PropertyHelper_ReturnsCachedPropertyHelper()
        {
            // Arrange
            var anonymous = new { foo = "bar" };

            // Act
            var helpers1 = PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo());
            var helpers2 = PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo());

            // Assert
            Assert2.Single(helpers1);
            Assert.AreSame(helpers1, helpers2);
            Assert.AreSame(helpers1[0], helpers2[0]);
        }

        [TestMethod]
        public void PropertyHelper_DoesNotChangeUnderscores()
        {
            // Arrange
            var anonymous = new { bar_baz2 = "foo" };

            // Act + Assert
            var helper = Assert2.Single(PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo()));
            Assert.AreEqual("bar_baz2", helper.Name);
        }

        [TestMethod]
        public void PropertyHelper_DoesNotFindPrivateProperties()
        {
            // Arrange
            var anonymous = new PrivateProperties();

            // Act + Assert
            var helper = Assert2.Single(PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo()));
            Assert.AreEqual("Prop1", helper.Name);
        }

        [TestMethod]
        public void PropertyHelper_DoesNotFindStaticProperties()
        {
            // Arrange
            var anonymous = new Static();

            // Act + Assert
            var helper = Assert2.Single(PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo()));
            Assert.AreEqual("Prop5", helper.Name);
        }

#if NETSTANDARD || NETCOREAPP
        [TestMethod]
        public void PropertyHelper_RefStructProperties()
        {
            // Arrange
            var obj = new RefStructProperties();

            // Act + Assert
            var helper = Assert2.Single(PropertyHelper.GetPropertyAccessors(obj.GetType().GetTypeInfo()));
            Assert.AreEqual("Prop5", helper.Name);
        }
#elif NET46 || NET461
#else
        //#error Unknown TFM - update the set of TFMs where we test for ref structs
#endif

        //[TestMethod]
        //public void PropertyHelper_DoesNotFindSetOnlyProperties()
        //{
        //    // Arrange
        //    var anonymous = new SetOnly();

        //    // Act + Assert
        //    var helper = Assert2.Single(PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo()));
        //    Assert.AreEqual("Prop6", helper.Name);
        //}

        //[Theory]
        //[InlineData(typeof(int?))]
        //[InlineData(typeof(DayOfWeek?))]
        [TestMethod]
        public void PropertyHelper_WorksForNullablePrimitiveAndEnumTypes()
        {
            // Act
            var properties = PropertyHelper.GetPropertyAccessors(typeof(int?));
            // Assert
            Assert2.Empty(properties);

            // Act
            var properties2 = PropertyHelper.GetPropertyAccessors(typeof(DayOfWeek?));
            // Assert
            Assert2.Empty(properties2);
        }

        [TestMethod]
        public void PropertyHelper_UnwrapsNullableTypes()
        {
            // Arrange
            var myType = typeof(MyStruct?);

            // Act
            var properties = PropertyHelper.GetPropertyAccessors(myType);

            // Assert
            var property = Assert2.Single(properties);
            Assert.AreEqual("Foo", property.Name);
        }

        [TestMethod]
        public void PropertyHelper_WorksForStruct()
        {
            // Arrange
            var anonymous = new MyProperties();

            anonymous.IntProp = 3;
            anonymous.StringProp = "Five";

            // Act + Assert
            var helper1 = Assert2.Single(PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo()).Where(prop => prop.Name == "IntProp"));
            var helper2 = Assert2.Single(PropertyHelper.GetPropertyAccessors(anonymous.GetType().GetTypeInfo()).Where(prop => prop.Name == "StringProp"));
            Assert.AreEqual(3, helper1.GetValue(anonymous));
            Assert.AreEqual("Five", helper2.GetValue(anonymous));
        }

        [TestMethod]
        public void PropertyHelper_ForDerivedClass()
        {
            // Arrange
            var derived = new DerivedClass { PropA = "propAValue", PropB = "propBValue" };

            // Act
            var helpers = PropertyHelper.GetPropertyAccessors(derived.GetType().GetTypeInfo()).ToArray();

            // Assert
            Assert.IsNotNull(helpers);
            Assert.AreEqual(2, helpers.Length);

            var propAHelper = Assert2.Single(helpers.Where(h => h.Name == "PropA"));
            var propBHelper = Assert2.Single(helpers.Where(h => h.Name == "PropB"));

            Assert.AreEqual("propAValue", propAHelper.GetValue(derived));
            Assert.AreEqual("propBValue", propBHelper.GetValue(derived));
        }

        [TestMethod]
        public void PropertyHelper_ForDerivedClass_WithNew()
        {
            // Arrange
            var derived = new DerivedClassWithNew { PropA = "propAValue" };

            // Act
            var helpers = PropertyHelper.GetPropertyAccessors(derived.GetType().GetTypeInfo()).ToArray();

            // Assert
            Assert.IsNotNull(helpers);
            Assert.AreEqual(2, helpers.Length);

            var propAHelper = Assert2.Single(helpers.Where(h => h.Name == "PropA"));
            var propBHelper = Assert2.Single(helpers.Where(h => h.Name == "PropB"));

            Assert.AreEqual("propAValue", propAHelper.GetValue(derived));
            Assert.AreEqual("Newed", propBHelper.GetValue(derived));
        }

        [TestMethod]
        public void PropertyHelper_ForDerived_WithVirtual()
        {
            // Arrange
            var derived = new DerivedClassWithOverride { PropA = "propAValue", PropB = "propBValue" };

            // Act
            var helpers = PropertyHelper.GetPropertyAccessors(derived.GetType().GetTypeInfo()).ToArray();

            // Assert
            Assert.IsNotNull(helpers);
            Assert.AreEqual(2, helpers.Length);

            var propAHelper = Assert2.Single(helpers.Where(h => h.Name == "PropA"));
            var propBHelper = Assert2.Single(helpers.Where(h => h.Name == "PropB"));

            Assert.AreEqual("OverridenpropAValue", propAHelper.GetValue(derived));
            Assert.AreEqual("propBValue", propBHelper.GetValue(derived));
        }

        //[TestMethod]
        //public void PropertyHelper_ForInterface_ReturnsExpectedProperties()
        //{
        //    // Arrange
        //    var expectedNames = new[] { "Count", "IsReadOnly" };

        //    // Act
        //    var helpers = PropertyHelper.GetPropertyAccessors(typeof(ICollection<string>));

        //    // Assert
        //    Assert2.Collection(
        //        helpers.OrderBy(helper => helper.Name, StringComparer.Ordinal),
        //        helper => { Assert2.AreEqual(expectedNames[0], helper.Name, StringComparer.Ordinal); },
        //        helper => { Assert2.AreEqual(expectedNames[1], helper.Name, StringComparer.Ordinal); });
        //}

        //[TestMethod]
        //public void PropertyHelper_ForDerivedInterface_ReturnsAllProperties()
        //{
        //    // Arrange
        //    var expectedNames = new[] { "Count", "IsReadOnly", "Keys", "Values" };

        //    // Act
        //    var helpers = PropertyHelper.GetPropertyAccessors(typeof(IDictionary<string, string>));

        //    // Assert
        //    Assert2.Collection(
        //        helpers.OrderBy(helper => helper.Name, StringComparer.Ordinal),
        //        helper => { Assert2.AreEqual(expectedNames[0], helper.Name, StringComparer.Ordinal); },
        //        helper => { Assert2.AreEqual(expectedNames[1], helper.Name, StringComparer.Ordinal); },
        //        helper => { Assert2.AreEqual(expectedNames[2], helper.Name, StringComparer.Ordinal); },
        //        helper => { Assert2.AreEqual(expectedNames[3], helper.Name, StringComparer.Ordinal); });
        //}

        [TestMethod]
        public void GetProperties_ExcludesIndexersAndPropertiesWithoutPublicGetters()
        {
            // Arrange
            var type = typeof(DerivedClassWithNonReadableProperties);

            // Act
            var result = PropertyHelper.GetPropertyAccessors(type).ToArray();

            // Assert
            Assert.IsNotNull(result);

            //Assert.AreEqual(3, result.Length);
            //Assert.AreEqual("Visible", result[0].Name);
            //Assert.AreEqual("PropA", result[1].Name);
            //Assert.AreEqual("PropB", result[2].Name);
        }

        //[TestMethod]
        //public void GetVisibleProperties_NoHiddenProperty()
        //{
        //    // Arrange
        //    var type = typeof(string);

        //    // Act
        //    var result = PropertyHelper.GetVisibleProperties(type).ToArray();

        //    // Assert
        //    var property = Assert2.Single(result);
        //    Assert.AreEqual("Length", property.Name);
        //    Assert.AreEqual(typeof(int), property.Property.PropertyType);
        //}

        //[TestMethod]
        //public void GetVisibleProperties_HiddenProperty()
        //{
        //    // Arrange
        //    var type = typeof(DerivedHiddenProperty);

        //    // Act
        //    var result = PropertyHelper.GetVisibleProperties(type).ToArray();

        //    // Assert
        //    Assert.AreEqual(2, result.Length);
        //    Assert.AreEqual("Id", result[0].Name);
        //    Assert.AreEqual(typeof(string), result[0].Property.PropertyType);
        //    Assert.AreEqual("Name", result[1].Name);
        //    Assert.AreEqual(typeof(string), result[1].Property.PropertyType);
        //}

        //[TestMethod]
        //public void GetVisibleProperties_HiddenProperty_TwoLevels()
        //{
        //    // Arrange
        //    var type = typeof(DerivedHiddenProperty2);

        //    // Act
        //    var result = PropertyHelper.GetVisibleProperties(type).ToArray();

        //    // Assert
        //    Assert.AreEqual(2, result.Length);
        //    Assert.AreEqual("Id", result[0].Name);
        //    Assert.AreEqual(typeof(Guid), result[0].Property.PropertyType);
        //    Assert.AreEqual("Name", result[1].Name);
        //    Assert.AreEqual(typeof(string), result[1].Property.PropertyType);
        //}

        //[TestMethod]
        //public void GetVisibleProperties_NoHiddenPropertyWithTypeInfoInput()
        //{
        //    // Arrange
        //    var type = typeof(string);

        //    // Act
        //    var result = PropertyHelper.GetVisibleProperties(type.GetTypeInfo()).ToArray();

        //    // Assert
        //    var property = Assert2.Single(result);
        //    Assert.AreEqual("Length", property.Name);
        //    Assert.AreEqual(typeof(int), property.Property.PropertyType);
        //}

        //[TestMethod]
        //public void GetVisibleProperties_HiddenPropertyWithTypeInfoInput()
        //{
        //    // Arrange
        //    var type = typeof(DerivedHiddenProperty);

        //    // Act
        //    var result = PropertyHelper.GetVisibleProperties(type.GetTypeInfo()).ToArray();

        //    // Assert
        //    Assert.AreEqual(2, result.Length);
        //    Assert.AreEqual("Id", result[0].Name);
        //    Assert.AreEqual(typeof(string), result[0].Property.PropertyType);
        //    Assert.AreEqual("Name", result[1].Name);
        //    Assert.AreEqual(typeof(string), result[1].Property.PropertyType);
        //}

        //[TestMethod]
        //public void GetVisibleProperties_HiddenProperty_TwoLevelsWithTypeInfoInput()
        //{
        //    // Arrange
        //    var type = typeof(DerivedHiddenProperty2);

        //    // Act
        //    var result = PropertyHelper.GetVisibleProperties(type.GetTypeInfo()).ToArray();

        //    // Assert
        //    Assert.AreEqual(2, result.Length);
        //    Assert.AreEqual("Id", result[0].Name);
        //    Assert.AreEqual(typeof(Guid), result[0].Property.PropertyType);
        //    Assert.AreEqual("Name", result[1].Name);
        //    Assert.AreEqual(typeof(string), result[1].Property.PropertyType);
        //}

        [TestMethod]
        public void MakeFastPropertySetter_SetsPropertyValues_ForPublicAndNobPublicProperties()
        {
            // Arrange
            var instance = new BaseClass();
            var typeInfo = instance.GetType().GetTypeInfo();
            var publicProperty = typeInfo.GetDeclaredProperty("PropA");
            var protectedProperty = typeInfo.GetDeclaredProperty("PropProtected");
            var publicPropertySetter = PropertyHelper.MakeFastPropertySetter(publicProperty);
            var protectedPropertySetter = PropertyHelper.MakeFastPropertySetter(protectedProperty);

            // Act
            publicPropertySetter(instance, "TestPublic");
            protectedPropertySetter(instance, "TestProtected");

            // Assert
            Assert.AreEqual("TestPublic", instance.PropA);
            Assert.AreEqual("TestProtected", instance.GetPropProtected());
        }

        [TestMethod]
        public void MakeFastPropertySetter_SetsPropertyValues_ForOverridenProperties()
        {
            // Arrange
            var instance = new DerivedClassWithOverride();
            var typeInfo = instance.GetType().GetTypeInfo();
            var property = typeInfo.GetDeclaredProperty("PropA");
            var propertySetter = PropertyHelper.MakeFastPropertySetter(property);

            // Act
            propertySetter(instance, "Test value");

            // Assert
            Assert.AreEqual("OverridenTest value", instance.PropA);
        }

        [TestMethod]
        public void MakeFastPropertySetter_SetsPropertyValues_ForNewedProperties()
        {
            // Arrange
            var instance = new DerivedClassWithNew();
            var typeInfo = instance.GetType().GetTypeInfo();
            var property = typeInfo.GetDeclaredProperty("PropB");
            var propertySetter = PropertyHelper.MakeFastPropertySetter(property);

            // Act
            propertySetter(instance, "Test value");

            // Assert
            Assert.AreEqual("NewedTest value", instance.PropB);
        }

        [TestMethod]
        public void MakeFastPropertyGetter_ReferenceType_ForNullObject_Throws()
        {
            // Arrange
            var property = PropertyHelper
                .GetPropertyAccessors(typeof(BaseClass))
                .Single(p => p.Name == nameof(BaseClass.PropA));

            var accessor = PropertyHelper.MakeFastPropertyGetter(property.Property);

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() => accessor(null));
        }

        [TestMethod]
        public void MakeFastPropertyGetter_ValueType_ForNullObject_Throws()
        {
            // Arrange
            var property = PropertyHelper
                .GetPropertyAccessors(typeof(MyProperties))
                .Single(p => p.Name == nameof(MyProperties.StringProp));

            var accessor = PropertyHelper.MakeFastPropertyGetter(property.Property);

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() => accessor(null));
        }

        [TestMethod]
        public void MakeNullSafeFastPropertyGetter_ReferenceType_Success()
        {
            // Arrange
            var property = PropertyHelper
                .GetPropertyAccessors(typeof(BaseClass))
                .Single(p => p.Name == nameof(BaseClass.PropA));

            var accessor = PropertyHelper.MakeNullSafeFastPropertyGetter(property.Property);

            // Act
            var value = accessor(new BaseClass() { PropA = "Hi" });

            // Assert
            Assert.AreEqual("Hi", value);
        }

        [TestMethod]
        public void MakeNullSafeFastPropertyGetter_ValueType_Success()
        {
            // Arrange
            var property = PropertyHelper
                .GetPropertyAccessors(typeof(MyProperties))
                .Single(p => p.Name == nameof(MyProperties.StringProp));

            var accessor = PropertyHelper.MakeNullSafeFastPropertyGetter(property.Property);

            // Act
            var value = accessor(new MyProperties() { StringProp = "Hi" });

            // Assert
            Assert.AreEqual("Hi", value);
        }

        [TestMethod]
        public void MakeNullSafeFastPropertyGetter_ReferenceType_ForNullObject_ReturnsNull()
        {
            // Arrange
            var property = PropertyHelper
                .GetPropertyAccessors(typeof(BaseClass))
                .Single(p => p.Name == nameof(BaseClass.PropA));

            var accessor = PropertyHelper.MakeNullSafeFastPropertyGetter(property.Property);

            // Act
            var value = accessor(null);

            // Assert
            Assert.IsNull(value);
        }

        [TestMethod]
        public void MakeNullSafeFastPropertyGetter_ValueType_ForNullObject_ReturnsNull()
        {
            // Arrange
            var property = PropertyHelper
                .GetPropertyAccessors(typeof(MyProperties))
                .Single(p => p.Name == nameof(MyProperties.StringProp));

            var accessor = PropertyHelper.MakeNullSafeFastPropertyGetter(property.Property);

            // Act
            var value = accessor(null);

            // Assert
            Assert.IsNull(value);
        }

        public static TheoryData<object, KeyValuePair<string, object>> IgnoreCaseTestData
        {
            get
            {
                return new TheoryData<object, KeyValuePair<string, object>>
                {
                    {
                        new
                        {
                            selected = true,
                            SeLeCtEd = false
                        },
                        new KeyValuePair<string, object>("selected", false)
                    },
                    {
                        new
                        {
                            SeLeCtEd = false,
                            selected = true
                        },
                        new KeyValuePair<string, object>("SeLeCtEd", true)
                    },
                    {
                        new
                        {
                            SelECTeD = false,
                            SeLECTED = true
                        },
                        new KeyValuePair<string, object>("SelECTeD", true)
                    }
                };
            }
        }

        [TestMethod]
        public void ObjectToDictionary_IgnoresPropertyCase()
        {
            foreach (var item in IgnoreCaseTestData)
            {
                ObjectToDictionary_IgnoresPropertyCase_Theory(item.Key, item.Value);
            }
        }

        //[Theory]
        //[MemberData(nameof(IgnoreCaseTestData))]
        private void ObjectToDictionary_IgnoresPropertyCase_Theory(object testObject,
                                                           KeyValuePair<string, object> expectedEntry)
        {
            // Act
            var result = PropertyHelper.ObjectToDictionary(testObject);

            // Assert
            var entry = Assert2.Single(result);
            Assert.AreEqual(expectedEntry, entry);
        }

        [TestMethod]
        public void ObjectToDictionary_WithNullObject_ReturnsEmptyDictionary()
        {
            // Arrange
            object value = null;

            // Act
            var dictValues = PropertyHelper.ObjectToDictionary(value);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(0, dictValues.Count);
        }

        [TestMethod]
        public void ObjectToDictionary_WithPlainObjectType_ReturnsEmptyDictionary()
        {
            // Arrange
            var value = new object();

            // Act
            var dictValues = PropertyHelper.ObjectToDictionary(value);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(0, dictValues.Count);
        }

        //[TestMethod]
        //public void ObjectToDictionary_WithPrimitiveType_LooksUpPublicProperties()
        //{
        //    // Arrange
        //    var value = "test";

        //    // Act
        //    var dictValues = PropertyHelper.ObjectToDictionary(value);

        //    // Assert
        //    Assert.IsNotNull(dictValues);
        //    Assert.AreEqual(1, dictValues.Count);
        //    Assert.AreEqual(4, dictValues["Length"]);
        //}

        [TestMethod]
        public void ObjectToDictionary_WithAnonymousType_LooksUpProperties()
        {
            // Arrange
            var value = new { test = "value", other = 1 };

            // Act
            var dictValues = PropertyHelper.ObjectToDictionary(value);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(2, dictValues.Count);
            Assert.AreEqual("value", dictValues["test"]);
            Assert.AreEqual(1, dictValues["other"]);
        }

        [TestMethod]
        public void ObjectToDictionary_ReturnsCaseInsensitiveDictionary()
        {
            // Arrange
            var value = new { TEST = "value", oThEr = 1 };

            // Act
            var dictValues = PropertyHelper.ObjectToDictionary(value);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(2, dictValues.Count);
            Assert.AreEqual("value", dictValues["test"]);
            Assert.AreEqual(1, dictValues["other"]);
        }

        [TestMethod]
        public void ObjectToDictionary_ReturnsInheritedProperties()
        {
            // Arrange
            var value = new ThreeDPoint() { X = 5, Y = 10, Z = 17 };

            // Act
            var dictValues = PropertyHelper.ObjectToDictionary(value);

            // Assert
            Assert.IsNotNull(dictValues);
            Assert.AreEqual(3, dictValues.Count);
            Assert.AreEqual(5, dictValues["X"]);
            Assert.AreEqual(10, dictValues["Y"]);
            Assert.AreEqual(17, dictValues["Z"]);
        }

        private class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private class ThreeDPoint : Point
        {
            public int Z { get; set; }
        }

        private class Static
        {
            public static int Prop2 { get; set; }
            public int Prop5 { get; set; }
        }

#if NETSTANDARD || NETCOREAPP
        private class RefStructProperties
        {
            public Span<bool> Span => throw new NotImplementedException();
            public MyRefStruct UserDefined => throw new NotImplementedException();

            public int Prop5 { get; set; }
        }

        private readonly ref struct MyRefStruct
        {
        }
#elif NET46 || NET461
#else
        //#error Unknown TFM - update the set of TFMs where we test for ref structs
#endif

        private struct MyProperties
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        private class SetOnly
        {
            public int Prop2 { set { } }
            public int Prop6 { get; set; }
        }

        private class PrivateProperties
        {
            public int Prop1 { get; set; }
            protected int Prop2 { get; set; }
            private int Prop3 { get; set; }
        }

        public class BaseClass
        {
            public string PropA { get; set; }

            protected string PropProtected { get; set; }

            public string GetPropProtected()
            {
                return PropProtected;
            }
        }

        public class DerivedClass : BaseClass
        {
            public string PropB { get; set; }
        }

        public class BaseClassWithVirtual
        {
            public virtual string PropA { get; set; }
            public string PropB { get; set; }
        }

        public class DerivedClassWithNew : BaseClassWithVirtual
        {
            private string _value = "Newed";

            public new string PropB
            {
                get { return _value; }
                set { _value = "Newed" + value; }
            }
        }

        public class DerivedClassWithOverride : BaseClassWithVirtual
        {
            private string _value = "Overriden";

            public override string PropA
            {
                get { return _value; }
                set { _value = "Overriden" + value; }
            }
        }

        private class DerivedClassWithNonReadableProperties : BaseClassWithVirtual
        {
            public string this[int index]
            {
                get { return string.Empty; }
                set { }
            }

            public int Visible { get; set; }

            private string NotVisible { get; set; }

            public string NotVisible2 { private get; set; }

            public string NotVisible3
            {
                set { }
            }

            public static string NotVisible4 { get; set; }
        }

        private struct MyStruct
        {
            public string Foo { get; set; }
        }

        private class BaseHiddenProperty
        {
            public int Id { get; set; }
        }

        //private class DerivedHiddenProperty : BaseHiddenProperty
        //{
        //    public new string Id { get; set; }

        //    public string Name { get; set; }
        //}

        //private class DerivedHiddenProperty2 : DerivedHiddenProperty
        //{
        //    public new Guid Id { get; set; }

        //    public new string Name { get; private set; }
        //}
    }
}