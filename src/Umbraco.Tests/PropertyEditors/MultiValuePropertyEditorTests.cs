using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.PropertyEditors;
using System.Linq;

namespace Umbraco.Tests.PropertyEditors
{
    /// <summary>
    /// Tests for the base classes of ValueEditors and PreValueEditors that are used for Property Editors that edit
    /// multiple values such as the drop down list, check box list, color picker, etc....
    /// </summary>
    /// <remarks>
    /// Mostly this used to test the we'd store INT Ids in the Db but publish STRING values or sometimes the INT values
    /// to cache. Now we always just deal with strings and we'll keep the tests that show that.
    /// </remarks>
    [TestFixture]
    public class MultiValuePropertyEditorTests
    {
        [Test]
        public void DropDownMultipleValueEditor_Format_Data_For_Cache()
        {
            var dataType = new DataType(new CheckBoxListPropertyEditor(Mock.Of<ILogger>(), Mock.Of<ILocalizedTextService>()))
            {
                Configuration = new ValueListConfiguration
                {
                    Items = new List<ValueListConfiguration.ValueListItem>
                    {
                        new ValueListConfiguration.ValueListItem { Id = 4567, Value = "Value 1" },
                        new ValueListConfiguration.ValueListItem { Id = 1234, Value = "Value 2" },
                        new ValueListConfiguration.ValueListItem { Id = 8910, Value = "Value 3" }
                    }
                },
                Id = 1
            };

            var dataTypeService = new TestObjects.TestDataTypeService(dataType);

            var prop = new Property(1, new PropertyType(dataType));
            prop.SetValue("Value 1,Value 2,Value 3");

            var valueEditor = dataType.Editor.GetValueEditor();
            ((DataValueEditor) valueEditor).Configuration = dataType.Configuration;
            var result = valueEditor.ConvertDbToString(prop.PropertyType, prop.GetValue(), dataTypeService);

            Assert.AreEqual("Value 1,Value 2,Value 3", result);
        }

        [Test]
        public void DropDownValueEditor_Format_Data_For_Cache()
        {
            var dataType = new DataType(new CheckBoxListPropertyEditor(Mock.Of<ILogger>(), Mock.Of<ILocalizedTextService>()))
            {
                Configuration = new ValueListConfiguration
                {
                    Items = new List<ValueListConfiguration.ValueListItem>
                    {
                        new ValueListConfiguration.ValueListItem { Id = 10, Value = "Value 1" },
                        new ValueListConfiguration.ValueListItem { Id = 1234, Value = "Value 2" },
                        new ValueListConfiguration.ValueListItem { Id = 11, Value = "Value 3" }
                    }
                },
                Id = 1
            };

            var dataTypeService = new TestObjects.TestDataTypeService(dataType);

            var prop = new Property(1, new PropertyType(dataType));
            prop.SetValue("Value 2");

            var result = dataType.Editor.GetValueEditor().ConvertDbToString(prop.PropertyType, prop.GetValue(), dataTypeService);

            Assert.AreEqual("Value 2", result);
        }

        [Test]
        public void ValueListConfiguration_ToConfigurationEditor()
        {
            var configuration = new ValueListConfiguration
            {
                Items = new List<ValueListConfiguration.ValueListItem>
                {
                    new ValueListConfiguration.ValueListItem { Id = 1, Value = "Item 1" },
                    new ValueListConfiguration.ValueListItem { Id = 2, Value = "Item 2" },
                    new ValueListConfiguration.ValueListItem { Id = 3, Value = "Item 3" }
                }
            };

            var editor = new ValueListConfigurationEditor(Mock.Of<ILocalizedTextService>());

            var result = editor.ToConfigurationEditor(configuration);

            // 'result' is meant to be serialized, is built with anonymous objects
            // so we cannot really test what's in it - but by serializing it
            var json = JsonConvert.SerializeObject(result);
            Assert.AreEqual("{\"items\":{\"1\":{\"value\":\"Item 1\",\"sortOrder\":1},\"2\":{\"value\":\"Item 2\",\"sortOrder\":2},\"3\":{\"value\":\"Item 3\",\"sortOrder\":3}}}", json);
        }

        [Test]
        public void ValueListConfiguration_FromConfigurationEditor_JObject()
        {
            const string json = "{\"items\":{\"1\":{\"value\":\"Item 1\",\"sortOrder\":1},\"2\":{\"value\":\"Item 2\",\"sortOrder\":2},\"3\":{\"value\":\"Item 3\",\"sortOrder\":3}}}";

            var editorValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var editor = new ValueListConfigurationEditor(Mock.Of<ILocalizedTextService>());
            var currentConfiguration = new ValueListConfiguration();
            var result = editor.FromConfigurationEditor(editorValues, currentConfiguration);

            Assert.AreEqual(3, result.Items.Count);
            Assert.AreEqual("Item 1", result.Items[0].Value);
            Assert.AreEqual(1, result.Items[0].Id);
            Assert.AreEqual("Item 2", result.Items[1].Value);
            Assert.AreEqual(2, result.Items[1].Id);
            Assert.AreEqual("Item 3", result.Items[2].Value);
            Assert.AreEqual(3, result.Items[2].Id);
        }

        // [{"key":"multiple","value":false},{"key":"items","value":[{"value":"a","sortOrder":1,"id":"1"},{"value":"b","sortOrder":2,"id":"2"},{"value":"c"}]}]

        [Test]
        public void ValueListConfiguration_FromConfigurationEditor_JArray()
        {
            const string json = "[{\"key\":\"multiple\",\"value\":false},{\"key\":\"items\",\"value\":[{\"value\":\"Item 1\",\"sortOrder\":1,\"id\":\"1\"},{\"value\":\"Item 2\",\"sortOrder\":2,\"id\":\"2\"},{\"value\":\"Item 3\"}]}]";

            // this is what happens in DataTypeController
            var editorValues = JsonConvert.DeserializeObject<IEnumerable<DataTypeConfigurationFieldSave>>(json).ToDictionary(x => x.Key, x => x.Value);

            var editor = new ValueListConfigurationEditor(Mock.Of<ILocalizedTextService>());
            var currentConfiguration = new ValueListConfiguration();
            var result = editor.FromConfigurationEditor(editorValues, currentConfiguration);

            Assert.AreEqual(3, result.Items.Count);
            Assert.AreEqual("Item 1", result.Items[0].Value);
            Assert.AreEqual(1, result.Items[0].Id);
            Assert.AreEqual("Item 2", result.Items[1].Value);
            Assert.AreEqual(2, result.Items[1].Id);
            Assert.AreEqual("Item 3", result.Items[2].Value);
            Assert.AreEqual(3, result.Items[2].Id);
        }
    }
}
