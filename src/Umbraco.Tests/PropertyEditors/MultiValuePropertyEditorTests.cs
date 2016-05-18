using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
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
    [TestFixture]
    public class MultiValuePropertyEditorTests
    {
        //TODO: Test the other formatting methods for the drop down classes

        [Test]
        public void DropDownMultipleValueEditor_With_Keys_Format_Data_For_Cache()
        {
            var dataTypeServiceMock = new Mock<IDataTypeService>();
            var dataTypeService = dataTypeServiceMock.Object;
            var editor = new PublishValuesMultipleValueEditor(true, dataTypeService, new PropertyValueEditor());

            var prop = new Property(1, Guid.NewGuid(),
                                    new PropertyType(new DataTypeDefinition(1, "Test.TestEditor")),
                                    "1234,4567,8910");

            var result = editor.ConvertDbToString(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

            Assert.AreEqual("1234,4567,8910", result);
        }

        [Test]
        public void DropDownMultipleValueEditor_No_Keys_Format_Data_For_Cache()
        {
            var dataTypeServiceMock = new Mock<IDataTypeService>();

            dataTypeServiceMock
                .Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                           .Returns(new PreValueCollection(new Dictionary<string, PreValue>
                               {
                                   {"key0", new PreValue(4567, "Value 1")},
                                   {"key1", new PreValue(1234, "Value 2")},
                                   {"key2", new PreValue(8910, "Value 3")}
                               }));

            var dataTypeService = dataTypeServiceMock.Object;
            var editor = new PublishValuesMultipleValueEditor(false, dataTypeService, new PropertyValueEditor());

            var prop = new Property(1, Guid.NewGuid(),
                                    new PropertyType(new DataTypeDefinition(1, "Test.TestEditor")),
                                    "1234,4567,8910");

            var result = editor.ConvertDbToString(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

            Assert.AreEqual("Value 1,Value 2,Value 3", result);
        }

        [Test]
        public void DropDownValueEditor_Format_Data_For_Cache()
        {
            var dataTypeServiceMock = new Mock<IDataTypeService>();
            dataTypeServiceMock
                .Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                           .Returns(new PreValueCollection(new Dictionary<string, PreValue>
                               {
                                   {"key0", new PreValue(10, "Value 1")},
                                   {"key1", new PreValue(1234, "Value 2")},
                                   {"key2", new PreValue(11, "Value 3")}
                               }));

            var dataTypeService = dataTypeServiceMock.Object;
            var editor = new PublishValueValueEditor(dataTypeService, new PropertyValueEditor());

            var prop = new Property(1, Guid.NewGuid(),
                                    new PropertyType(new DataTypeDefinition(1, "Test.TestEditor")),
                                    "1234");

            var result = editor.ConvertDbToString(prop, prop.PropertyType, new Mock<IDataTypeService>().Object);

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
            var appContext = new ApplicationContext(
                new DatabaseContext(TestObjects.GetIDatabaseFactoryMock(), logger),
                new ServiceContext(
                    localizedTextService: textService.Object
                ),
                Mock.Of<CacheHelper>(),
                new ProfilingLogger(logger, Mock.Of<IProfiler>()))
            {
                IsReady = true
            };
            ApplicationContext.Current = appContext;

            var defaultVals = new Dictionary<string, object>();
            var persisted = new PreValueCollection(new Dictionary<string, PreValue>
                {
                    {"item1", new PreValue(1, "Item 1")},
                    {"item2", new PreValue(2, "Item 2")},
                    {"item3", new PreValue(3, "Item 3")}
                });

            var editor = new ValueListPreValueEditor(Mock.Of<ILocalizedTextService>());

            var result = editor.ConvertDbToEditor(defaultVals, persisted);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("items"));
            var items = result["items"] as IDictionary<int, IDictionary<string, object>>;
            Assert.IsNotNull(items);
            Assert.AreEqual("Item 1", items[1]["value"]);
            Assert.AreEqual("Item 2", items[2]["value"]);
            Assert.AreEqual("Item 3", items[3]["value"]);
        }

    }
}