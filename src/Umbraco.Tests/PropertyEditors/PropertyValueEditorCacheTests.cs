using System;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class PropertyValueEditorCacheTests
    {
        [TestCase("{prop1: 'val1', prop2: 'val2'}", "JSON")]
        [TestCase("[1,2,3,4]", "STRING")]
        [TestCase("hello world", "STRING")]
        public void Value_Editor_Can_Serialize_Text_And_Json_As_CData_For_Cache(string propertyValue, string valueType)
        {
            var settings = SettingsForTests.GenerateMockSettings();
            SettingsForTests.ConfigureSettings(settings);
            var valueEditor = new PropertyValueEditor
            {
                ValueType = valueType
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Ntext), propertyValue);

            var result = valueEditor.ConvertDbToXml(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

            // text and JSON data should default to string serialization in a CDATA element
            Assert.IsNotNull(result);
            Assert.IsTrue(result is XCData);
            Assert.AreEqual(propertyValue, (result as XCData).Value);
        }

        [TestCase("[1,2,3,4]")]
        [TestCase("hello world")]
        public void Value_Editor_Can_Serialize_Text_As_Xml_For_Cache(string propertyValue)
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.PublishJsonAsXml).Returns(true);
            SettingsForTests.ConfigureSettings(settings);

            var valueEditor = new PropertyValueEditor
            {
                ValueType = "STRING"
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Ntext), propertyValue);

            var result = valueEditor.ConvertDbToXml(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

            // when using PublishJsonAsXml=true, text data should still default to string serialization in a CDATA element
            Assert.IsNotNull(result);
            Assert.IsTrue(result is XCData);
            Assert.AreEqual(propertyValue, (result as XCData).Value);
        }

        [Test]
        public void Value_Editor_Can_Serialize_Json_As_Xml_For_Cache()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.PublishJsonAsXml).Returns(true);
            SettingsForTests.ConfigureSettings(settings);

            const string propertyValue = @"{prop1: 'val1', prop2: 'val2'}";
            var valueEditor = new PropertyValueEditor
            {
                ValueType = "JSON"
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Ntext), propertyValue);

            var result = valueEditor.ConvertDbToXml(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

            // when using PublishJsonAsXml=true, JSON data should be serialized as XML 
            Assert.IsNotNull(result);
            Assert.IsTrue(result is XElement);
            Assert.AreEqual(@"<jsonAsXml><prop1>val1</prop1><prop2>val2</prop2></jsonAsXml>", (result as XElement).ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void Value_Editor_Can_Serialize_Json_With_Arrays_As_Xml_For_Cache()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.PublishJsonAsXml).Returns(true);
            SettingsForTests.ConfigureSettings(settings);

            const string propertyValue = @"{prop1: ['val1'], prop2: ['val2', 'val3']}";
            var valueEditor = new PropertyValueEditor
            {
                ValueType = "JSON"
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Ntext), propertyValue);

            var result = valueEditor.ConvertDbToXml(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

            // when using PublishJsonAsXml=true, JSON data should be serialized as XML 
            Assert.IsNotNull(result);
            Assert.IsTrue(result is XElement);
            // array properties with only one element should be serialized with the json:Array attribute explicitly set to make sure the deserialization works properly
            Assert.AreEqual(@"<jsonAsXml><prop1 json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">val1</prop1><prop2>val2</prop2><prop2>val3</prop2></jsonAsXml>", (result as XElement).ToString(SaveOptions.DisableFormatting));
        }

        [TestCase("{prop1: ['val1'], prop2}")]
        [TestCase("{prop1: 'val1', prop2: 3")]
        public void Value_Editor_Can_Serialize_Invalid_Json_As_CData_For_Cache(string propertyValue)
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.PublishJsonAsXml).Returns(true);
            SettingsForTests.ConfigureSettings(settings);

            var valueEditor = new PropertyValueEditor
            {
                ValueType = "JSON"
            };

            var prop = new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Ntext), propertyValue);

            var result = valueEditor.ConvertDbToXml(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

            // when using PublishJsonAsXml=true, invalid JSON data should still default to string serialization in a CDATA element
            Assert.IsNotNull(result);
            Assert.IsTrue(result is XCData);
            Assert.AreEqual(propertyValue, (result as XCData).Value);
        }

    }
}
