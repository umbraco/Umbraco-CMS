﻿using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Strings;
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
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new DefaultShortStringHelper(SettingsForTests.GetDefault()));
            Resolution.Freeze();
        }

        [TearDown]
        public virtual void TestTearDown()
        {
            ResolverCollection.ResetAll();
            Resolution.Reset();
        }

        [TestCase("{prop1: 'val1', prop2: 'val2'}", true)]
        [TestCase("{1,2,3,4}", false)]
        [TestCase("[1,2,3,4]", true)]
        [TestCase("hello world", false)]
        public void Value_Editor_Can_Convert_To_Json_Object_For_Editor(string value, bool isOk)
        {
            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Nvarchar), value);

            var valueEditor = new PropertyValueEditor
                {
                    ValueType = PropertyEditorValueTypes.String
            };

            var result = valueEditor.ConvertDbToEditor(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);
            Assert.AreEqual(isOk, !(result is string));
        }

        [TestCase("STRING", "hello", "hello")]
        [TestCase("TEXT", "hello", "hello")]
        [TestCase("INT", "123", 123)]
        [TestCase("INTEGER", "123", 123)]
        [TestCase("INTEGER", "", null)] //test empty string for int  
        [TestCase("DATETIME", "", null)] //test empty string for date
        public void Value_Editor_Can_Convert_To_Clr_Type(string valueType, string val, object expected)
        {
            var valueEditor = new PropertyValueEditor
                {
                    ValueType = valueType
                };

            var result = valueEditor.TryConvertValueToCrlType(val);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(expected, result.Result);
        }

        // The following decimal tests have not been added as [TestCase]s
        // as the decimal type cannot be used as an attribute parameter
        [Test]
        public void Value_Editor_Can_Convert_To_Decimal_Clr_Type()
        {
            var valueEditor = new PropertyValueEditor
            {
                ValueType = PropertyEditorValueTypes.Decimal
            };

            var result = valueEditor.TryConvertValueToCrlType("12.34");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(12.34M, result.Result);
        }

        [Test]
        public void Value_Editor_Can_Convert_To_Decimal_Clr_Type_With_Other_Separator()
        {
            var valueEditor = new PropertyValueEditor
            {
                ValueType = PropertyEditorValueTypes.Decimal
            };

            var result = valueEditor.TryConvertValueToCrlType("12,34");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(12.34M, result.Result);
        }

        [Test]
        public void Value_Editor_Can_Convert_To_Decimal_Clr_Type_With_Empty_String()
        {
            var valueEditor = new PropertyValueEditor
            {
                ValueType = PropertyEditorValueTypes.Decimal
            };

            var result = valueEditor.TryConvertValueToCrlType(string.Empty);
            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Result);
        }
        
        [Test]
        public void Value_Editor_Can_Convert_To_Date_Clr_Type()
        {
            var valueEditor = new PropertyValueEditor
                {
                    ValueType = PropertyEditorValueTypes.Date
            };

            var result = valueEditor.TryConvertValueToCrlType("2010-02-05");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(new DateTime(2010, 2, 5), result.Result);
        }

        [TestCase(PropertyEditorValueTypes.String, "hello", "hello")]
        [TestCase(PropertyEditorValueTypes.Text, "hello", "hello")]
        [TestCase(PropertyEditorValueTypes.Integer, 123, "123")]
        [TestCase(PropertyEditorValueTypes.IntegerAlternative, 123, "123")]
        [TestCase(PropertyEditorValueTypes.Integer, "", "")] //test empty string for int        
        [TestCase(PropertyEditorValueTypes.DateTime, "", "")] //test empty string for date
        public void Value_Editor_Can_Serialize_Value(string valueType, object val, string expected)
        {
            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Nvarchar), val);

            var valueEditor = new PropertyValueEditor
                {
                    ValueType = valueType
                };

            var result = valueEditor.ConvertDbToEditor(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Value_Editor_Can_Serialize_Decimal_Value()
        {
            var value = 12.34M;
            var valueEditor = new PropertyValueEditor
                {
                    ValueType = PropertyEditorValueTypes.Decimal
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Decimal), value);

            var result = valueEditor.ConvertDbToEditor(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);
            Assert.AreEqual("12.34", result);
        }

        [Test]
        public void Value_Editor_Can_Serialize_Decimal_Value_With_Empty_String()
        {
            var valueEditor = new PropertyValueEditor
                {
                    ValueType = PropertyEditorValueTypes.Decimal
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Decimal), string.Empty);

            var result = valueEditor.ConvertDbToEditor(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void Value_Editor_Can_Serialize_Date_Value()
        {
            var now = DateTime.Now;
            var valueEditor = new PropertyValueEditor
                {
                    ValueType = PropertyEditorValueTypes.Date
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Date), now);

            var result = valueEditor.ConvertDbToEditor(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);
            Assert.AreEqual(now.ToIsoString(), result);
        }
    }
}