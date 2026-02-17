// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

[TestFixture]
public class PropertyCacheLevelTests
{
    [TestCase(PropertyCacheLevel.None, 2)]
    [TestCase(PropertyCacheLevel.Element, 1)]
    [TestCase(PropertyCacheLevel.Elements, 1)]
    [TestCase(PropertyCacheLevel.Snapshot, 2)]
    public void CacheLevelTest(PropertyCacheLevel cacheLevel, int interConverts)
    {
        var converter = new CacheConverter1(cacheLevel);

        var converters = new PropertyValueConverterCollection(() => new IPropertyValueConverter[] { converter });

        var configurationEditorJsonSerializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var dataTypeServiceMock = new Mock<IDataTypeService>();
        var dataType = new DataType(
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), configurationEditorJsonSerializer)
        { Id = 1 };
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

        var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return publishedContentTypeFactory.CreatePropertyType(contentType, "prop1", dataType.Id);
        }

        var setType1 = publishedContentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "set1", CreatePropertyTypes);

        var elementsCache = new ElementsDictionaryAppCache();
        var variationContextAccessor = new TestVariationContextAccessor { VariationContext = new() };
        var contentNode = CreateContentNode("Set 1", 1234, setType1, new Dictionary<string, object> { { "prop1", "1234" } });

        var set1 = new PublishedElement(contentNode, false, elementsCache, variationContextAccessor);

        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);
        Assert.AreEqual(1, converter.InterConverts);

        // source is always converted once and cached per content
        // inter conversion depends on the specified cache level
        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);
        Assert.AreEqual(interConverts, converter.InterConverts);
    }

    [TestCase(PropertyCacheLevel.None, 2, 0, 0)]
    [TestCase(PropertyCacheLevel.Element, 1, 0, 0)]
    [TestCase(PropertyCacheLevel.Elements, 1, 1, 1)]
    public void CachePublishedSnapshotTest(
        PropertyCacheLevel converterCacheLevel,
        int interConverts,
        int elementsCount1,
        int elementsCount2)
    {
        var converter = new CacheConverter1(converterCacheLevel);

        var converters = new PropertyValueConverterCollection(() => new IPropertyValueConverter[] { converter });

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        var dataType = new DataType(
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory()))
        { Id = 1 };
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

        var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return publishedContentTypeFactory.CreatePropertyType(contentType, "prop1", 1);
        }

        var setType1 = publishedContentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "set1", CreatePropertyTypes);

        var elementsCache = new ElementsDictionaryAppCache();

        var cacheManager = new Mock<ICacheManager>();
        cacheManager.Setup(x => x.ElementsCache).Returns(elementsCache);

        var variationContextAccessor = new TestVariationContextAccessor { VariationContext = new() };
        var contentNode = CreateContentNode("Set 1", 1234, setType1, new Dictionary<string, object> { { "prop1", "1234" } });

        var set1 = new PublishedElement(contentNode, false, elementsCache, variationContextAccessor);

        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);
        Assert.AreEqual(1, converter.InterConverts);

        Assert.AreEqual(elementsCount1, elementsCache.Count);
        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);
        Assert.AreEqual(interConverts, converter.InterConverts);

        Assert.AreEqual(elementsCount2, elementsCache.Count);

        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);

        Assert.AreEqual(elementsCount2, elementsCache.Count);

        var oldElementsCache = elementsCache;
        elementsCache.Clear();

        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);

        Assert.AreEqual(elementsCount2, elementsCache.Count);
        Assert.AreEqual(elementsCount2, oldElementsCache.Count);
    }

    [Test]
    public void CacheUnknownTest()
    {
        var converter = new CacheConverter1(PropertyCacheLevel.Unknown);

        var converters = new PropertyValueConverterCollection(() => new IPropertyValueConverter[] { converter });

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        var dataType = new DataType(
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory()))
        { Id = 1 };
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

        var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return publishedContentTypeFactory.CreatePropertyType(contentType, "prop1", 1);
        }

        var setType1 = publishedContentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "set1", CreatePropertyTypes);

        Assert.Throws<Exception>(() =>
        {
            var elementsCache = new ElementsDictionaryAppCache();
            var variationContextAccessor = new TestVariationContextAccessor { VariationContext = new() };

            var contentNode = CreateContentNode("Set 1", 1234, setType1, new Dictionary<string, object> { { "prop1", "1234" } });
            var unused = new PublishedElement(contentNode, false, elementsCache, variationContextAccessor);
        });
    }

    private class CacheConverter1 : IPropertyValueConverter
    {
        private readonly PropertyCacheLevel _cacheLevel;

        public CacheConverter1(PropertyCacheLevel cacheLevel) => _cacheLevel = cacheLevel;

        public int SourceConverts { get; private set; }

        public int InterConverts { get; private set; }

        public bool? IsValue(object value, PropertyValueLevel level)
            => value != null && (!(value is string) || string.IsNullOrWhiteSpace((string)value) == false);

        public bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

        public Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(int);

        public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => _cacheLevel;

        public object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            SourceConverts++;
            return int.TryParse(source as string, out var i) ? i : 0;
        }

        public object ConvertIntermediateToObject(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
        {
            InterConverts++;
            return (int)inter;
        }

        public object ConvertIntermediateToXPath(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
            => ((int)inter).ToString();
    }

    private ContentNode CreateContentNode(string name, int id, IPublishedContentType contentType, Dictionary<string, object> properties)
    {
        var contentData = new ContentData(
            name: name,
            urlSegment: name.ToLowerInvariant().Replace(" ", "-"),
            versionId: 1,
            versionDate: DateTime.Today,
            writerId: -1,
            templateId: null,
            published: true,
            properties: properties
                .ToDictionary(
                    p => p.Key,
                    p => new PropertyData[] { new() { Value = p.Value, Culture = string.Empty, Segment = string.Empty } }),
            cultureInfos: null);
        return new ContentNode(id, Guid.NewGuid(), 1, DateTime.Today, -1, contentType, contentData, contentData);
    }
}
