using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class BlockListPropertyValueConverterTests
    {
        private BlockListPropertyValueConverter Create()
        {
            var publishedSnapshotAccessor = Mock.Of<IPublishedSnapshotAccessor>();
            var publishedModelFactory = Mock.Of<IPublishedModelFactory>();
            var editor = new BlockListPropertyValueConverter(
                Mock.Of<IProfilingLogger>(),
                publishedModelFactory,
                new BlockEditorConverter(publishedSnapshotAccessor, publishedModelFactory));
            return editor;
        }

        [Test]
        public void Is_Converter_For()
        {
            var editor = Create();
            Assert.IsTrue(editor.IsConverter(Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.BlockList)));
            Assert.IsFalse(editor.IsConverter(Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.NestedContent)));
        }

        [Test]
        public void Get_Value_Type_Multiple()
        {
            var editor = Create();
            var config = new BlockListConfiguration
            {
                ElementTypes = new[] {
                    new BlockListConfiguration.ElementType
                    {
                        Alias = "Test1"
                    },
                    new BlockListConfiguration.ElementType
                    {
                        Alias = "Test2"
                    }
                }
            };

            var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
            var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

            var valueType = editor.GetPropertyValueType(propType);

            Assert.AreEqual(typeof(IEnumerable<IPublishedElement>), valueType);
        }

        [Test]
        public void Get_Value_Type_Single()
        {
            var editor = Create();
            var config = new BlockListConfiguration
            {
                ElementTypes = new[] {
                    new BlockListConfiguration.ElementType
                    {
                        Alias = "Test1"
                    }
                }
            };

            var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
            var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

            var valueType = editor.GetPropertyValueType(propType);

            var modelType = typeof(IEnumerable<>).MakeGenericType(ModelType.For(config.ElementTypes[0].Alias));

            // we can't compare the exact match of types because ModelType.For generates a new/different type even if the same alias is used
            Assert.AreEqual(modelType.FullName, valueType.FullName);
        }


    }
}
