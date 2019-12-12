using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.Components;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class PropertyEditorValueEditorTests
    {
        [SetUp]
        public virtual void TestSetup()
        {
            //normalize culture
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            var register = TestHelper.GetRegister();
            var composition = new Composition(register, TestHelper.GetMockedTypeLoader(), Mock.Of<IProfilingLogger>(), ComponentTests.MockRuntimeState(RuntimeLevel.Run), TestHelper.GetConfigs(), TestHelper.IOHelper, AppCaches.NoCache);

            register.Register<IShortStringHelper>(_
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(SettingsForTests.GetDefaultUmbracoSettings())));

            Current.Factory = composition.CreateFactory();
        }

        [TearDown]
        public virtual void TestTearDown()
        {
            Current.Reset();
        }

        [TestCase("{prop1: 'val1', prop2: 'val2'}", true)]
        [TestCase("{1,2,3,4}", false)]
        [TestCase("[1,2,3,4]", true)]
        [TestCase("hello world", false)]
        public void Value_Editor_Can_Convert_To_Json_Object_For_Editor(string value, bool isOk)
        {
            var prop = new Property(1, new PropertyType("test", ValueStorageType.Nvarchar));
            prop.SetValue(value);

            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.String);

            var result = valueEditor.ToEditor(prop);
            Assert.AreEqual(isOk, !(result is string));
        }

        [TestCase("STRING", "hello", "hello")]
        [TestCase("TEXT", "hello", "hello")]
        [TestCase("INT", "123", 123)]
        [TestCase("INT", "", null)] //test empty string for int
        [TestCase("DATETIME", "", null)] //test empty string for date
        public void Value_Editor_Can_Convert_To_Clr_Type(string valueType, string val, object expected)
        {
            var valueEditor = TestHelper.CreateDataValueEditor(valueType);

            var result = valueEditor.TryConvertValueToCrlType(val);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Result);
        }

        // The following decimal tests have not been added as [TestCase]s
        // as the decimal type cannot be used as an attribute parameter
        [Test]
        public void Value_Editor_Can_Convert_To_Decimal_Clr_Type()
        {
            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.Decimal);

            var result = valueEditor.TryConvertValueToCrlType("12.34");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(12.34M, result.Result);
        }

        [Test]
        public void Value_Editor_Can_Convert_To_Decimal_Clr_Type_With_Other_Separator()
        {
            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.Decimal);

            var result = valueEditor.TryConvertValueToCrlType("12,34");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(12.34M, result.Result);
        }

        [Test]
        public void Value_Editor_Can_Convert_To_Decimal_Clr_Type_With_Empty_String()
        {
            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.Decimal);

            var result = valueEditor.TryConvertValueToCrlType(string.Empty);
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Result);
        }

        [Test]
        public void Value_Editor_Can_Convert_To_Date_Clr_Type()
        {
            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.Date);

            var result = valueEditor.TryConvertValueToCrlType("2010-02-05");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(new DateTime(2010, 2, 5), result.Result);
        }

        [TestCase(ValueTypes.String, "hello", "hello")]
        [TestCase(ValueTypes.Text, "hello", "hello")]
        [TestCase(ValueTypes.Integer, 123, "123")]
        [TestCase(ValueTypes.Integer, "", "")] //test empty string for int
        [TestCase(ValueTypes.DateTime, "", "")] //test empty string for date
        public void Value_Editor_Can_Serialize_Value(string valueType, object val, string expected)
        {
            var prop = new Property(1, new PropertyType("test", ValueStorageType.Nvarchar));
            prop.SetValue(val);

            var valueEditor = TestHelper.CreateDataValueEditor(valueType);

            var result = valueEditor.ToEditor(prop);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Value_Editor_Can_Serialize_Decimal_Value()
        {
            var value = 12.34M;
            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.Decimal);

            var prop = new Property(1, new PropertyType("test", ValueStorageType.Decimal));
            prop.SetValue(value);

            var result = valueEditor.ToEditor(prop);
            Assert.AreEqual("12.34", result);
        }

        [Test]
        public void Value_Editor_Can_Serialize_Decimal_Value_With_Empty_String()
        {
            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.Decimal);

            var prop = new Property(1, new PropertyType("test", ValueStorageType.Decimal));
            prop.SetValue(string.Empty);

            var result = valueEditor.ToEditor(prop);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void Value_Editor_Can_Serialize_Date_Value()
        {
            var now = DateTime.Now;
            var valueEditor = TestHelper.CreateDataValueEditor(ValueTypes.Date);

            var prop = new Property(1, new PropertyType("test", ValueStorageType.Date));
            prop.SetValue(now);

            var result = valueEditor.ToEditor(prop);
            Assert.AreEqual(now.ToIsoString(), result);
        }
    }
}
