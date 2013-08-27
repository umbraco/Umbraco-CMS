using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
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
            var dataTypeService = MockRepository.GenerateStub<IDataTypeService>();
            var editor = new PublishValuesMultipleValueEditor(true, dataTypeService, new ValueEditor());

            var result = editor.FormatValueForCache(
                new Property(1, Guid.NewGuid(),
                             new PropertyType(new DataTypeDefinition(1, Guid.NewGuid())),
                             "1234,4567,8910"));

            Assert.AreEqual("1234,4567,8910", result);
        }

        [Test]
        public void DropDownMultipleValueEditor_No_Keys_Format_Data_For_Cache()
        {
            var dataTypeService = MockRepository.GenerateStub<IDataTypeService>();
            dataTypeService
                .Stub(x => x.GetPreValuesCollectionByDataTypeId(Arg<int>.Is.Anything))
                           .Return(new PreValueCollection(new Dictionary<string, PreValue>
                               {
                                   {"key0", new PreValue(4567, "Value 1")},
                                   {"key1", new PreValue(1234, "Value 2")},
                                   {"key2", new PreValue(8910, "Value 3")}
                               }));
            var editor = new PublishValuesMultipleValueEditor(false, dataTypeService, new ValueEditor());

            var result = editor.FormatValueForCache(
                new Property(1, Guid.NewGuid(),
                             new PropertyType(new DataTypeDefinition(1, Guid.NewGuid())),
                             "1234,4567,8910"));

            Assert.AreEqual("Value 1,Value 2,Value 3", result);
        }

        [Test]
        public void DropDownValueEditor_Format_Data_For_Cache()
        {
            var dataTypeService = MockRepository.GenerateStub<IDataTypeService>();
            dataTypeService
                .Stub(x => x.GetPreValuesCollectionByDataTypeId(Arg<int>.Is.Anything))
                           .Return(new PreValueCollection(new Dictionary<string, PreValue>
                               {
                                   {"key0", new PreValue(10, "Value 1")},
                                   {"key1", new PreValue(1234, "Value 2")},
                                   {"key2", new PreValue(11, "Value 3")}
                               }));
            var editor = new PublishValueValueEditor(dataTypeService, new ValueEditor());

            var result = editor.FormatValueForCache(
                new Property(1, Guid.NewGuid(),
                             new PropertyType(new DataTypeDefinition(1, Guid.NewGuid())),
                             "1234"));

            Assert.AreEqual("Value 2", result);
        }

        [Test]
        public void DropDownPreValueEditor_Format_Data_For_Editor()
        {

            var defaultVals = new Dictionary<string, object>();
            var persisted = new PreValueCollection(new Dictionary<string, PreValue>
                {
                    {"item1", new PreValue(1, "Item 1")},
                    {"item2", new PreValue(2, "Item 2")},
                    {"item3", new PreValue(3, "Item 3")}
                });

            var editor = new ValueListPreValueEditor();

            var result = editor.FormatDataForEditor(defaultVals, persisted);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("items"));
            var items = result["items"] as IDictionary<int, string>;
            Assert.IsNotNull(items);
            Assert.AreEqual("Item 1", items[1]);
            Assert.AreEqual("Item 2", items[2]);
            Assert.AreEqual("Item 3", items[3]);
        }

    }
}