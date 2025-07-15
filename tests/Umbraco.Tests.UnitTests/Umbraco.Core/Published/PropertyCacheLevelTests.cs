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
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

[TestFixture]
public class PropertyCacheLevelTests
{
    [TestCase(PropertyCacheLevel.None, 2)]
    [TestCase(PropertyCacheLevel.Element, 1)]
    [TestCase(PropertyCacheLevel.Elements, 1)]
    [TestCase(PropertyCacheLevel.Snapshot, 1)]
    public void CacheLevelTest(PropertyCacheLevel cacheLevel, int interConverts)
    {
        var converter = new CacheConverter1(cacheLevel);

        var converters = new PropertyValueConverterCollection(() => new IPropertyValueConverter[] { converter });

        var configurationEditorJsonSerializer = new SystemTextConfigurationEditorJsonSerializer();
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

        // PublishedElementPropertyBase.GetCacheLevels:
        //
        //   if property level is > reference level, or both are None
        //     use None for property & new reference
        //   else
        //     use Content for property, & keep reference
        //
        // PublishedElement creates properties with reference being None
        // if converter specifies None, keep using None
        // anything else is not > None, use Content
        //
        // for standalone elements, it's only None or Content
        var set1 = new PublishedElement(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false, new VariationContext());

        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);
        Assert.AreEqual(1, converter.InterConverts);

        // source is always converted once and cached per content
        // inter conversion depends on the specified cache level
        Assert.AreEqual(1234, set1.Value(Mock.Of<IPublishedValueFallback>(), "prop1"));
        Assert.AreEqual(1, converter.SourceConverts);
        Assert.AreEqual(interConverts, converter.InterConverts);
    }

    // property is not cached, converted cached at Content, exept
    //  /None = not cached at all
    [TestCase(PropertyCacheLevel.None, PropertyCacheLevel.None, 2, 0, 0)]
    [TestCase(PropertyCacheLevel.None, PropertyCacheLevel.Element, 1, 0, 0)]
    [TestCase(PropertyCacheLevel.None, PropertyCacheLevel.Elements, 1, 0, 0)]

    // property is cached at element level, converted cached at
    //  /None = not at all
    //  /Element = in element
    //  /Snapshot = in snapshot
    //  /Elements = in elements
    [TestCase(PropertyCacheLevel.Element, PropertyCacheLevel.None, 2, 0, 0)]
    [TestCase(PropertyCacheLevel.Element, PropertyCacheLevel.Element, 1, 0, 0)]
    [TestCase(PropertyCacheLevel.Element, PropertyCacheLevel.Elements, 1, 1, 1)]

    // property is cached at elements level, converted cached at Element, exept
    //  /None = not cached at all
    //  /Snapshot = cached in snapshot
    [TestCase(PropertyCacheLevel.Elements, PropertyCacheLevel.None, 2, 0, 0)]
    [TestCase(PropertyCacheLevel.Elements, PropertyCacheLevel.Element, 1, 0, 0)]
    [TestCase(PropertyCacheLevel.Elements, PropertyCacheLevel.Elements, 1, 0, 0)]
    public void CachePublishedSnapshotTest(
        PropertyCacheLevel referenceCacheLevel,
        PropertyCacheLevel converterCacheLevel,
        int interConverts,
        int elementsCount1,
        int elementsCount2)
    {
        var converter = new CacheConverter1(converterCacheLevel);

        var converters = new PropertyValueConverterCollection(() => new IPropertyValueConverter[] { converter });

        var dataTypeServiceMock = new Mock<IDataTypeService>();
        var dataType = new DataType(
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), new SystemTextConfigurationEditorJsonSerializer())
        { Id = 1 };
        dataTypeServiceMock.Setup(x => x.GetAll()).Returns(dataType.Yield);

        var publishedContentTypeFactory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), converters, dataTypeServiceMock.Object);

        IEnumerable<IPublishedPropertyType> CreatePropertyTypes(IPublishedContentType contentType)
        {
            yield return publishedContentTypeFactory.CreatePropertyType(contentType, "prop1", 1);
        }

        var setType1 = publishedContentTypeFactory.CreateContentType(Guid.NewGuid(), 1000, "set1", CreatePropertyTypes);

        var elementsCache = new FastDictionaryAppCache();

        var cacheManager = new Mock<ICacheManager>();
        cacheManager.Setup(x => x.ElementsCache).Returns(elementsCache);

        // pretend we're creating this set as a value for a property
        // referenceCacheLevel is the cache level for this fictious property
        // converterCacheLevel is the cache level specified by the converter
        var set1 = new PublishedElement(
            setType1,
            Guid.NewGuid(),
            new Dictionary<string, object>
            {
                { "prop1", "1234" },
            },
            false,
            referenceCacheLevel,
            new VariationContext(),
            cacheManager.Object);

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
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), new SystemTextConfigurationEditorJsonSerializer())
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
            var unused = new PublishedElement(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false, new VariationContext());
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
}
