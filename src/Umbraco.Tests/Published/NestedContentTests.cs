using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Published
{
    [TestFixture]
    public class NestedContentTests
    {
        private (IPublishedContentType, IPublishedContentType) CreateContentTypes()
        {
            Current.Reset();

            var logger = Mock.Of<ILogger>();
            var profiler = Mock.Of<IProfiler>();
            var proflog = new ProfilingLogger(logger, profiler);

            PropertyEditorCollection editors = null;
            var editor = new NestedContentPropertyEditor(logger, new Lazy<PropertyEditorCollection>(() => editors), Mock.Of<IDataTypeService>(), Mock.Of<IContentTypeService>(), Mock.Of<ILocalizedTextService>());
            editors = new PropertyEditorCollection(new DataEditorCollection(new DataEditor[] { editor }));

            var dataType1 = new DataType(editor)
            {
                Id = 1,
                Configuration = new NestedContentConfiguration
                {
                    MinItems = 1,
                    MaxItems = 1,
                    ContentTypes = new[]
                    {
                        new NestedContentConfiguration.ContentType { Alias = "contentN1" }
                    }
                }
            };

            var dataType2 = new DataType(editor)
            {
                Id = 2,
                Configuration = new NestedContentConfiguration
                {
                    MinItems = 1,
                    MaxItems = 99,
                    ContentTypes = new[]
                    {
                        new NestedContentConfiguration.ContentType { Alias = "contentN1" }
                    }
                }
            };

            var dataType3 = new DataType(new TextboxPropertyEditor(logger))
            {
                Id = 3
            };

            // mocked dataservice returns nested content preValues
            var dataTypeService = new TestObjects.TestDataTypeService(dataType1, dataType2, dataType3);

            var publishedModelFactory = new Mock<IPublishedModelFactory>();

            // mocked model factory returns model type
            var modelTypes = new Dictionary<string, Type>
            {
                { "contentN1", typeof(TestElementModel) }
            };
            publishedModelFactory
                .Setup(x => x.MapModelType(It.IsAny<Type>()))
                .Returns((Type type) => ModelType.Map(type, modelTypes));

            // mocked model factory creates models
            publishedModelFactory
                .Setup(x => x.CreateModel(It.IsAny<IPublishedElement>()))
                .Returns((IPublishedElement element) =>
                {
                    if (element.ContentType.Alias.InvariantEquals("contentN1"))
                        return new TestElementModel(element);
                    return element;
                });

            // mocked model factory creates model lists
            publishedModelFactory
                .Setup(x => x.CreateModelList(It.IsAny<string>()))
                .Returns((string alias) =>
                {
                    return alias == "contentN1"
                        ? (IList)new List<TestElementModel>()
                        : (IList)new List<IPublishedElement>();
                });

            var contentCache = new Mock<IPublishedContentCache>();
            var publishedSnapshot = new Mock<IPublishedSnapshot>();

            // mocked published snapshot returns a content cache
            publishedSnapshot
                .Setup(x => x.Content)
                .Returns(contentCache.Object);

            var publishedSnapshotAccessor = new Mock<IPublishedSnapshotAccessor>();

            // mocked published snapshot accessor returns a facade
            publishedSnapshotAccessor
                .Setup(x => x.PublishedSnapshot)
                .Returns(publishedSnapshot.Object);

            var converters = new PropertyValueConverterCollection(new IPropertyValueConverter[]
            {
                new NestedContentSingleValueConverter(publishedSnapshotAccessor.Object, publishedModelFactory.Object, proflog),
                new NestedContentManyValueConverter(publishedSnapshotAccessor.Object, publishedModelFactory.Object, proflog),
            });

            var factory = new PublishedContentTypeFactory(publishedModelFactory.Object, converters, dataTypeService);

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes1(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "property1", 1);
            }

            IEnumerable<IPublishedPropertyType> CreatePropertyTypes2(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "property2", 2);
            }

            IEnumerable<IPublishedPropertyType> CreatePropertyTypesN1(IPublishedContentType contentType)
            {
                yield return factory.CreatePropertyType(contentType, "propertyN1", 3);
            }

            var contentType1 = factory.CreateContentType(Guid.NewGuid(), 1, "content1", CreatePropertyTypes1);
            var contentType2 = factory.CreateContentType(Guid.NewGuid(), 2, "content2", CreatePropertyTypes2);
            var contentTypeN1 = factory.CreateContentType(Guid.NewGuid(), 2, "contentN1", CreatePropertyTypesN1, isElement: true);

            // mocked content cache returns content types
            contentCache
                .Setup(x => x.GetContentType(It.IsAny<string>()))
                .Returns((string alias) =>
                {
                    if (alias.InvariantEquals("contentN1")) return contentTypeN1;
                    return null;
                });

            return (contentType1, contentType2);
        }

        [Test]
        public void SingleNestedTest()
        {
            (var contentType1, _) = CreateContentTypes();

            // nested single converter returns the proper value clr type TestModel, and cache level
            Assert.AreEqual(typeof(TestElementModel), contentType1.GetPropertyType("property1").ClrType);
            Assert.AreEqual(PropertyCacheLevel.Element, contentType1.GetPropertyType("property1").CacheLevel);

            var key = Guid.NewGuid();
            var keyA = Guid.NewGuid();
            var content = new SolidPublishedContent(contentType1)
            {
                Key = key,
                Properties = new[]
                {
                    new TestPublishedProperty(contentType1.GetPropertyType("property1"), $@"[
                    {{ ""key"": ""{keyA}"", ""propertyN1"": ""foo"", ""ncContentTypeAlias"": ""contentN1"" }}
                ]")
                }
            };
            var value = content.Value("property1");

            // nested single converter returns proper TestModel value
            Assert.IsInstanceOf<TestElementModel>(value);
            var valueM = (TestElementModel)value;
            Assert.AreEqual("foo", valueM.PropValue);
            Assert.AreEqual(keyA, valueM.Key);
        }

        [Test]
        public void ManyNestedTest()
        {
            (_, var contentType2) = CreateContentTypes();

            // nested many converter returns the proper value clr type IEnumerable<TestModel>, and cache level
            Assert.AreEqual(typeof(IEnumerable<TestElementModel>), contentType2.GetPropertyType("property2").ClrType);
            Assert.AreEqual(PropertyCacheLevel.Element, contentType2.GetPropertyType("property2").CacheLevel);

            var key = Guid.NewGuid();
            var keyA = Guid.NewGuid();
            var keyB = Guid.NewGuid();
            var content = new SolidPublishedContent(contentType2)
            {
                Key = key,
                Properties = new[]
                {
                    new TestPublishedProperty(contentType2.GetPropertyType("property2"), $@"[
                    {{ ""key"": ""{keyA}"", ""propertyN1"": ""foo"", ""ncContentTypeAlias"": ""contentN1"" }},
                    {{ ""key"": ""{keyB}"", ""propertyN1"": ""bar"", ""ncContentTypeAlias"": ""contentN1"" }}
                ]")
                }
            };
            var value = content.Value("property2");

            // nested many converter returns proper IEnumerable<TestModel> value
            Assert.IsInstanceOf<IEnumerable<IPublishedElement>>(value);
            Assert.IsInstanceOf<IEnumerable<TestElementModel>>(value);
            var valueM = ((IEnumerable<TestElementModel>)value).ToArray();
            Assert.AreEqual("foo", valueM[0].PropValue);
            Assert.AreEqual(keyA, valueM[0].Key);
            Assert.AreEqual("bar", valueM[1].PropValue);
            Assert.AreEqual(keyB, valueM[1].Key);
        }

        public class TestElementModel : PublishedElementModel
        {
            public TestElementModel(IPublishedElement content)
                : base(content)
            { }

            public string PropValue => this.Value<string>("propertyN1");
        }

        class TestPublishedProperty : PublishedPropertyBase
        {
            private readonly bool _preview;
            private readonly object _sourceValue;
            private readonly bool _hasValue;
            private IPublishedElement _owner;

            public TestPublishedProperty(IPublishedPropertyType propertyType, object source)
                : base(propertyType, PropertyCacheLevel.Element) // initial reference cache level always is .Content
            {
                _sourceValue = source;
                _hasValue = source != null && (!(source is string ssource) || !string.IsNullOrWhiteSpace(ssource));
            }

            public TestPublishedProperty(IPublishedPropertyType propertyType, IPublishedElement element, bool preview, PropertyCacheLevel referenceCacheLevel, object source)
                : base(propertyType, referenceCacheLevel)
            {
                _sourceValue = source;
                _hasValue = source != null && (!(source is string ssource) || !string.IsNullOrWhiteSpace(ssource));
                _owner = element;
                _preview = preview;
            }

            private object InterValue => PropertyType.ConvertSourceToInter(null, _sourceValue, false);

            internal void SetOwner(IPublishedElement owner)
            {
                _owner = owner;
            }

            public override bool HasValue(string culture = null, string segment = null) => _hasValue;
            public override object GetSourceValue(string culture = null, string segment = null) => _sourceValue;
            public override object GetValue(string culture = null, string segment = null) => PropertyType.ConvertInterToObject(_owner, ReferenceCacheLevel, InterValue, _preview);
            public override object GetXPathValue(string culture = null, string segment = null) => throw new InvalidOperationException("This method won't be implemented.");
        }
    }
}
