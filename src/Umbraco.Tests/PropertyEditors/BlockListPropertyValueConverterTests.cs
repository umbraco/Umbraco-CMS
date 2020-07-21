using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class BlockListPropertyValueConverterTests
    {
        private readonly Guid Key1 = Guid.NewGuid();
        private readonly Guid Key2 = Guid.NewGuid();
        private readonly string Alias1 = "Test1";
        private readonly string Alias2 = "Test2";

        /// <summary>
        /// Setup mocks for IPublishedSnapshotAccessor
        /// </summary>
        /// <returns></returns>
        private IPublishedSnapshotAccessor GetPublishedSnapshotAccessor()
        {
            var test1ContentType = Mock.Of<IPublishedContentType2>(x =>
                x.IsElement == true
                && x.Key == Key1
                && x.Alias == Alias1);
            var test2ContentType = Mock.Of<IPublishedContentType2>(x =>
                x.IsElement == true
                && x.Key == Key2
                && x.Alias == Alias2);
            var contentCache = new Mock<IPublishedContentCache2>();
            contentCache.Setup(x => x.GetContentType(Key1)).Returns(test1ContentType);
            contentCache.Setup(x => x.GetContentType(Key2)).Returns(test2ContentType);
            var publishedSnapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == contentCache.Object);
            var publishedSnapshotAccessor = Mock.Of<IPublishedSnapshotAccessor>(x => x.PublishedSnapshot == publishedSnapshot);
            return publishedSnapshotAccessor;
        }

        private BlockListPropertyValueConverter CreateConverter()
        {
            var publishedSnapshotAccessor = GetPublishedSnapshotAccessor();
            var publishedModelFactory = new NoopPublishedModelFactory();
            var editor = new BlockListPropertyValueConverter(
                Mock.Of<IProfilingLogger>(),
                new BlockEditorConverter(publishedSnapshotAccessor, publishedModelFactory));
            return editor;
        }

        private BlockListConfiguration ConfigForMany() => new BlockListConfiguration
        {
            Blocks = new[] {
                    new BlockListConfiguration.BlockConfiguration
                    {
                        Key = Key1
                    },
                    new BlockListConfiguration.BlockConfiguration
                    {
                        Key = Key2
                    }
                }
        };

        private BlockListConfiguration ConfigForSingle() => new BlockListConfiguration
        {
            Blocks = new[] {
                    new BlockListConfiguration.BlockConfiguration
                    {
                        Key = Key1
                    }
                }
        };

        private IPublishedPropertyType GetPropertyType(BlockListConfiguration config)
        {            
            var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
            var propertyType = Mock.Of<IPublishedPropertyType>(x =>
                x.EditorAlias == Constants.PropertyEditors.Aliases.BlockList
                && x.DataType == dataType);
            return propertyType;
        }

        [Test]
        public void Is_Converter_For()
        {
            var editor = CreateConverter();
            Assert.IsTrue(editor.IsConverter(Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.BlockList)));
            Assert.IsFalse(editor.IsConverter(Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.NestedContent)));
        }

        [Test]
        public void Get_Value_Type_Multiple()
        {
            var editor = CreateConverter();
            var config = ConfigForMany();

            var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
            var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

            var valueType = editor.GetPropertyValueType(propType);

            // the result is always block list model
            Assert.AreEqual(typeof(BlockListModel), valueType);
        }

        [Test]
        public void Get_Value_Type_Single()
        {
            var editor = CreateConverter();
            var config = ConfigForSingle();

            var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
            var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

            var valueType = editor.GetPropertyValueType(propType);

            // the result is always block list model
            Assert.AreEqual(typeof(BlockListModel), valueType);
        }

        [Test]
        public void Convert_Null_Empty()
        {
            var editor = CreateConverter();
            var config = ConfigForMany();
            var propertyType = GetPropertyType(config);
            var publishedElement = Mock.Of<IPublishedElement>();

            string json = null;
            var converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.ContentData.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            json = string.Empty;
            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.ContentData.Count());
            Assert.AreEqual(0, converted.Layout.Count());            
        }

        [Test]
        public void Convert_Valid_Empty_Json()
        {
            var editor = CreateConverter();
            var config = ConfigForMany();
            var propertyType = GetPropertyType(config);
            var publishedElement = Mock.Of<IPublishedElement>();

            var json = "{}"; 
            var converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.ContentData.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            json = @"{
layout: {},
data: []}";
            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.ContentData.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            // Even though there is a layout, there is no data, so the conversion will result in zero elements in total
            json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9'
            }
        ]
    },
    contentData: []
}";
            
            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.ContentData.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            // Even though there is a layout and data, the data is invalid (missing required keys) so the conversion will result in zero elements in total
            json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9'
            }
        ]
    },
        contentData: [
        {
            'udi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9'
        }
    ]
}";

            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.ContentData.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            // Everthing is ok except the udi reference in the layout doesn't match the data so it will be empty
            json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
            }
        ]
    },
        contentData: [
        {
            'contentTypeKey': '" + Key1 + @"',
            'key': '1304E1DD-0000-4396-84FE-8A399231CB3D'
        }
    ]
}";

            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(1, converted.ContentData.Count());
            Assert.AreEqual(0, converted.Layout.Count());
        }

        [Test]
        public void Convert_Valid_Json()
        {
            var editor = CreateConverter();
            var config = ConfigForMany();
            var propertyType = GetPropertyType(config);
            var publishedElement = Mock.Of<IPublishedElement>();

            var json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
            }
        ]
    },
        contentData: [
        {
            'contentTypeKey': '" + Key1 + @"',
            'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
        }
    ]
}";
            var converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(1, converted.ContentData.Count());
            var item0 = converted.ContentData.ElementAt(0);
            Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Key);
            Assert.AreEqual("Test1", item0.ContentType.Alias);
            Assert.AreEqual(1, converted.Layout.Count());
            var layout0 = converted.Layout.ElementAt(0);
            Assert.IsNull(layout0.Settings);
            Assert.AreEqual(Udi.Parse("umb://element/1304E1DDAC87439684FE8A399231CB3D"), layout0.ContentUdi);
        }

        [Test]
        public void Get_Data_From_Layout_Item()
        {
            var editor = CreateConverter();
            var config = ConfigForMany();
            var propertyType = GetPropertyType(config);
            var publishedElement = Mock.Of<IPublishedElement>();

            var json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
            },
            {
                'contentUdi': 'umb://element/0A4A416E547D464FABCC6F345C17809A',
                'settingsUdi': null
            }
        ]
    },
        contentData: [
        {
            'contentTypeKey': '" + Key1 + @"',
            'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
        },
        {
            'contentTypeKey': '" + Key2 + @"',
            'udi': 'umb://element/E05A034704424AB3A520E048E6197E79'
        },
        {
            'contentTypeKey': '" + Key2 + @"',
            'udi': 'umb://element/0A4A416E547D464FABCC6F345C17809A'
        }
    ]
}";

            var converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(3, converted.ContentData.Count());
            Assert.AreEqual(2, converted.Layout.Count());

            var item0 = converted.Layout.ElementAt(0);
            Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Content.Key);
            Assert.AreEqual("Test1", item0.Content.ContentType.Alias);

            var item1 = converted.Layout.ElementAt(1);
            Assert.AreEqual(Guid.Parse("0A4A416E-547D-464F-ABCC-6F345C17809A"), item1.Content.Key);
            Assert.AreEqual("Test2", item1.Content.ContentType.Alias);

        }

    }
}
