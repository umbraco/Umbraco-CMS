using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.PropertyEditors;

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
        public void DropDownPreValueEditor_Format_Data_For_Editor()
        {
            // editor wants ApplicationContext.Current.Services.TextService
            // (that should be fixed with proper injection)
            var logger = Mock.Of<ILogger>();
            var textService = new Mock<ILocalizedTextService>();
            textService.Setup(x => x.Localize(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>())).Returns("blah");
            //var appContext = new ApplicationContext(
            //    new DatabaseContext(TestObjects.GetIDatabaseFactoryMock(), logger, Mock.Of<IRuntimeState>(), Mock.Of<IMigrationEntryService>()),
            //    new ServiceContext(
            //        localizedTextService: textService.Object
            //    ),
            //    Mock.Of<CacheHelper>(),
            //    new ProfilingLogger(logger, Mock.Of<IProfiler>()))
            //{
            //    //IsReady = true
            //};
            //Current.ApplicationContext = appContext;

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
        public void FromConfigurationEditor_WithSerializedConfiguration_DoesReturnCorrectValueListConfiguration()
        {
            var serializedJsonInput = "{\"items\":{\"1\":{\"value\":\"Item 1\",\"sortOrder\":1},\"2\":{\"value\":\"Item 2\",\"sortOrder\":2},\"3\":{\"value\":\"Item 3\",\"sortOrder\":3}}}";
            var inputDictionary = JsonConvert.DeserializeObject(serializedJsonInput, typeof(IDictionary<string, object>));
            var existingConfiguration = new ValueListConfiguration();

            var sut = new ValueListConfigurationEditor(Mock.Of<ILocalizedTextService>());
            var result = sut.FromConfigurationEditor((IDictionary<string, object>)inputDictionary, existingConfiguration);

            Assert.That(result.Items.Count, Is.EqualTo(3));
        }
    }
}
