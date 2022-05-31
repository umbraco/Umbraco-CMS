// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.PublishedCache.Internal;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

[TestFixture]
public class NestedContentTests
{
    private (IPublishedContentType, IPublishedContentType) CreateContentTypes()
    {
        var logger = Mock.Of<ILogger<ProfilingLogger>>();
        var loggerFactory = NullLoggerFactory.Instance;
        var profiler = Mock.Of<IProfiler>();
        var proflog = new ProfilingLogger(logger, profiler);
        var localizationService = Mock.Of<ILocalizationService>();

        PropertyEditorCollection editors = null;
        var editor = new NestedContentPropertyEditor(Mock.Of<IDataValueEditorFactory>(), Mock.Of<IIOHelper>(), Mock.Of<IEditorConfigurationParser>());
        editors = new PropertyEditorCollection(new DataEditorCollection(() => new DataEditor[] { editor }));

        var serializer = new ConfigurationEditorJsonSerializer();

        var dataType1 = new DataType(editor, serializer)
        {
            Id = 1,
            Configuration = new NestedContentConfiguration
            {
                MinItems = 1,
                MaxItems = 1,
                ContentTypes = new[] { new NestedContentConfiguration.ContentType { Alias = "contentN1" } },
            },
        };

        var dataType2 = new DataType(editor, serializer)
        {
            Id = 2,
            Configuration = new NestedContentConfiguration
            {
                MinItems = 1,
                MaxItems = 99,
                ContentTypes = new[] { new NestedContentConfiguration.ContentType { Alias = "contentN1" } },
            },
        };

        var dataType3 =
            new DataType(
                new TextboxPropertyEditor(Mock.Of<IDataValueEditorFactory>(), Mock.Of<IIOHelper>(), Mock.Of<IEditorConfigurationParser>()), serializer)
            { Id = 3 };

        // mocked dataservice returns nested content preValues
        var dataTypeServiceMock = new Mock<IDataTypeService>();
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { dataType1, dataType2, dataType3 });

        var publishedModelFactory = new Mock<IPublishedModelFactory>();

        // mocked model factory returns model type
        var modelTypes = new Dictionary<string, Type> { { "contentN1", typeof(TestElementModel) } };
        publishedModelFactory
            .Setup(x => x.MapModelType(It.IsAny<Type>()))
            .Returns((Type type) => ModelType.Map(type, modelTypes));

        // mocked model factory creates models
        publishedModelFactory
            .Setup(x => x.CreateModel(It.IsAny<IPublishedElement>()))
            .Returns((IPublishedElement element) =>
            {
                if (element.ContentType.Alias.InvariantEquals("contentN1"))
                {
                    return new TestElementModel(element, Mock.Of<IPublishedValueFallback>());
                }

                return element;
            });

        // mocked model factory creates model lists
        publishedModelFactory
            .Setup(x => x.CreateModelList(It.IsAny<string>()))
            .Returns((string alias) =>
                alias == "contentN1"
                    ? new List<TestElementModel>()
                    : new List<IPublishedElement>());

        var contentCache = new Mock<IPublishedContentCache>();
        var publishedSnapshot = new Mock<IPublishedSnapshot>();

        // mocked published snapshot returns a content cache
        publishedSnapshot
            .Setup(x => x.Content)
            .Returns(contentCache.Object);

        var publishedSnapshotAccessor = new Mock<IPublishedSnapshotAccessor>();

        // mocked published snapshot accessor returns a facade
        var localPublishedSnapshot = publishedSnapshot.Object;
        publishedSnapshotAccessor
            .Setup(x => x.TryGetPublishedSnapshot(out localPublishedSnapshot))
            .Returns(true);

        var converters = new PropertyValueConverterCollection(() => new IPropertyValueConverter[]
        {
            new NestedContentSingleValueConverter(publishedSnapshotAccessor.Object, publishedModelFactory.Object, proflog),
            new NestedContentManyValueConverter(publishedSnapshotAccessor.Object, publishedModelFactory.Object, proflog),
        });

        var factory =
            new PublishedContentTypeFactory(publishedModelFactory.Object, converters, dataTypeServiceMock.Object);

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
        var contentTypeN1 =
            factory.CreateContentType(Guid.NewGuid(), 2, "contentN1", CreatePropertyTypesN1, isElement: true);

        // mocked content cache returns content types
        contentCache
            .Setup(x => x.GetContentType(It.IsAny<string>()))
            .Returns((string alias) =>
            {
                if (alias.InvariantEquals("contentN1"))
                {
                    return contentTypeN1;
                }

                return null;
            });

        return (contentType1, contentType2);
    }

    [Test]
    public void SingleNestedTest()
    {
        var (contentType1, _) = CreateContentTypes();

        // nested single converter returns the proper value clr type TestModel, and cache level
        Assert.AreEqual(typeof(TestElementModel), contentType1.GetPropertyType("property1").ClrType);
        Assert.AreEqual(PropertyCacheLevel.Element, contentType1.GetPropertyType("property1").CacheLevel);

        var key = Guid.NewGuid();
        var keyA = Guid.NewGuid();
        var content = new InternalPublishedContent(contentType1)
        {
            Key = key,
            Properties = new[]
            {
                new TestPublishedProperty(
                    contentType1.GetPropertyType("property1"), $@"[
                    {{ ""key"": ""{keyA}"", ""propertyN1"": ""foo"", ""ncContentTypeAlias"": ""contentN1"" }}
                ]"),
            },
        };
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "property1");

        // nested single converter returns proper TestModel value
        Assert.IsInstanceOf<TestElementModel>(value);
        var valueM = (TestElementModel)value;
        Assert.AreEqual("foo", valueM.PropValue);
        Assert.AreEqual(keyA, valueM.Key);
    }

    [Test]
    public void ManyNestedTest()
    {
        var (_, contentType2) = CreateContentTypes();

        // nested many converter returns the proper value clr type IEnumerable<TestModel>, and cache level
        Assert.AreEqual(typeof(IEnumerable<TestElementModel>), contentType2.GetPropertyType("property2").ClrType);
        Assert.AreEqual(PropertyCacheLevel.Element, contentType2.GetPropertyType("property2").CacheLevel);

        var key = Guid.NewGuid();
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var content = new InternalPublishedContent(contentType2)
        {
            Key = key,
            Properties = new[]
            {
                new TestPublishedProperty(contentType2.GetPropertyType("property2"), $@"[
                    {{ ""key"": ""{keyA}"", ""propertyN1"": ""foo"", ""ncContentTypeAlias"": ""contentN1"" }},
                    {{ ""key"": ""{keyB}"", ""propertyN1"": ""bar"", ""ncContentTypeAlias"": ""contentN1"" }}
                ]"),
            },
        };
        var value = content.Value(Mock.Of<IPublishedValueFallback>(), "property2");

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
        public TestElementModel(IPublishedElement content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public string PropValue => this.Value<string>(Mock.Of<IPublishedValueFallback>(), "propertyN1");
    }

    public class TestPublishedProperty : PublishedPropertyBase
    {
        private readonly bool _hasValue;
        private readonly bool _preview;
        private readonly object _sourceValue;
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

        internal void SetOwner(IPublishedElement owner) => _owner = owner;

        public override bool HasValue(string culture = null, string? segment = null) => _hasValue;

        public override object GetSourceValue(string culture = null, string? segment = null) => _sourceValue;

        public override object GetValue(string culture = null, string? segment = null) =>
            PropertyType.ConvertInterToObject(_owner, ReferenceCacheLevel, InterValue, _preview);

        public override object GetXPathValue(string culture = null, string? segment = null) =>
            throw new InvalidOperationException("This method won't be implemented.");
    }
}
