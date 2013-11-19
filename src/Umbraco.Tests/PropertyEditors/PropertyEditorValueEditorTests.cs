using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class PropertyEditorValueEditorTests
    {
        [TestCase("{prop1: 'val1', prop2: 'val2'}", true)]
        [TestCase("{1,2,3,4}", false)]
        [TestCase("[1,2,3,4]", true)]
        [TestCase("hello world", false)]
        public void Value_Editor_Can_Convert_To_Json_Object_For_Editor(string value, bool isOk)
        {
            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Nvarchar), value);

            var valueEditor = new PropertyValueEditor
                {
                    ValueType = "STRING"
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

        [Test]
        public void Value_Editor_Can_Convert_To_Date_Clr_Type()
        {
            var valueEditor = new PropertyValueEditor
                {
                    ValueType = "DATE"
                };

            var result = valueEditor.TryConvertValueToCrlType("2010-02-05");
            Assert.IsTrue(result.Success);
            Assert.AreEqual(new DateTime(2010, 2, 5), result.Result);
        }

        [TestCase("STRING", "hello", "hello")]
        [TestCase("TEXT", "hello", "hello")]
        [TestCase("INT", 123, "123")]
        [TestCase("INTEGER", 123, "123")]
        [TestCase("INTEGER", "", "")] //test empty string for int        
        [TestCase("DATETIME", "", "")] //test empty string for date
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
        public void Value_Editor_Can_Serialize_Date_Value()
        {
            var now = DateTime.Now;
            var valueEditor = new PropertyValueEditor
                {
                    ValueType = "DATE"
                };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Date), now);

            var result = valueEditor.ConvertDbToEditor(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);
            Assert.AreEqual(now.ToIsoString(), result);
        }
    }
}