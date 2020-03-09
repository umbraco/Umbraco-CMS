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
        /// <summary>
        /// Setup mocks for IPublishedSnapshotAccessor
        /// </summary>
        /// <returns></returns>
        private IPublishedSnapshotAccessor GetPublishedSnapshotAccessor()
        {
            var homeContentType = Mock.Of<IPublishedContentType>(x =>
                x.IsElement == true
                && x.Alias == "home");
            var contentCache = new Mock<IPublishedContentCache>();
            contentCache.Setup(x => x.GetContentType("home")).Returns(homeContentType);
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
                publishedModelFactory,
                new BlockEditorConverter(publishedSnapshotAccessor, publishedModelFactory));
            return editor;
        }

        private BlockListConfiguration ConfigForMany() => new BlockListConfiguration
        {
            Blocks = new[] {
                    new BlockListConfiguration.BlockConfiguration
                    {
                        Alias = "Test1"
                    },
                    new BlockListConfiguration.BlockConfiguration
                    {
                        Alias = "Test2"
                    }
                }
        };

        private BlockListConfiguration ConfigForSingle() => new BlockListConfiguration
        {
            Blocks = new[] {
                    new BlockListConfiguration.BlockConfiguration
                    {
                        Alias = "Test1"
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

            Assert.AreEqual(typeof(IEnumerable<IPublishedElement>), valueType);
        }

        [Test]
        public void Get_Value_Type_Single()
        {
            var editor = CreateConverter();
            var config = ConfigForSingle();

            var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
            var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

            var valueType = editor.GetPropertyValueType(propType);

            var modelType = typeof(IEnumerable<>).MakeGenericType(ModelType.For(config.Blocks[0].Alias));

            // we can't compare the exact match of types because ModelType.For generates a new/different type even if the same alias is used
            Assert.AreEqual(modelType.FullName, valueType.FullName);
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
            Assert.AreEqual(0, converted.Data.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            json = string.Empty;
            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.Data.Count());
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
            Assert.AreEqual(0, converted.Data.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            json = @"{
layout: [],
data: []}";
            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.Data.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            // Even though there is a layout, there is no data, so the conversion will result in zero elements in total
            json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'udi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9',
                'settings': {}
            }
        ]
    },
    data: []
}";
            
            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.Data.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            // Even though there is a layout and data, the data is invalid (missing required keys) so the conversion will result in zero elements in total
            json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'udi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9',
                'settings': {}
            }
        ]
    },
        data: [
        {
            'udi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9'
        }
    ]
}";

            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(0, converted.Data.Count());
            Assert.AreEqual(0, converted.Layout.Count());

            // Everthing is ok except the udi reference in the layout doesn't match the data so it will be empty
            json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D',
                'settings': {}
            }
        ]
    },
        data: [
        {
            'contentTypeAlias': 'home',
            'key': '1304E1DD-0000-4396-84FE-8A399231CB3D'
        }
    ]
}";

            converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(1, converted.Data.Count());
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
                'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D',
                'settings': {}
            }
        ]
    },
        data: [
        {
            'contentTypeAlias': 'home',
            'key': '1304E1DD-AC87-4396-84FE-8A399231CB3D'
        }
    ]
}";
            var converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(1, converted.Data.Count());
            var item0 = converted.Data.ElementAt(0);
            Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Key);
            Assert.AreEqual("home", item0.ContentType.Alias);
            Assert.AreEqual(1, converted.Layout.Count());
            var layout0 = converted.Layout.ElementAt(0);
            Assert.IsNull(layout0.Settings);
            Assert.AreEqual(Udi.Parse("umb://element/1304E1DDAC87439684FE8A399231CB3D"), layout0.Udi);
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
                'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D',
                'settings': {}
            },
            {
                'udi': 'umb://element/0A4A416E547D464FABCC6F345C17809A',
                'settings': {}
            }
        ]
    },
        data: [
        {
            'contentTypeAlias': 'home',
            'key': '1304E1DD-AC87-4396-84FE-8A399231CB3D'
        },
        {
            'contentTypeAlias': 'home',
            'key': 'E05A0347-0442-4AB3-A520-E048E6197E79'
        },
        {
            'contentTypeAlias': 'home',
            'key': '0A4A416E-547D-464F-ABCC-6F345C17809A'
        }
    ]
}";

            var converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

            Assert.IsNotNull(converted);
            Assert.AreEqual(3, converted.Data.Count());
            Assert.AreEqual(2, converted.Layout.Count());

            var item0 = converted.Layout.ElementAt(0);
            var item0Data = converted.GetData(item0.Udi);
            Assert.IsNotNull(item0Data);
            Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0Data.Key);
            Assert.AreEqual("home", item0Data.ContentType.Alias);

            var item1 = converted.Layout.ElementAt(1);
            var item1Data = converted.GetData(item1.Udi);
            Assert.AreEqual(Guid.Parse("0A4A416E-547D-464F-ABCC-6F345C17809A"), item1Data.Key);
            Assert.AreEqual("home", item1Data.ContentType.Alias);

        }

    }
}
