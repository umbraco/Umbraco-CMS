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

/// <summary>
/// Contains unit tests for the <see cref="PropertyCacheLevel"/> enumeration and its related functionality in the Umbraco CMS core.
/// </summary>
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
    /// <summary>
    /// Tests caching behavior of published snapshot with different property cache levels and converter cache levels.
    /// </summary>
    /// <param name="referenceCacheLevel">The cache level of the reference property.</param>
    /// <param name="converterCacheLevel">The cache level specified by the converter.</param>
    /// <param name="interConverts">The expected number of intermediate conversions.</param>
    /// <param name="elementsCount1">The expected count of elements in cache after first access.</param>
    /// <param name="elementsCount2">The expected count of elements in cache after subsequent accesses.</param>
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
            new VoidEditor(Mock.Of<IDataValueEditorFactory>()), new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory()))
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

    /// <summary>
    /// Tests that using a property cache level of Unknown throws an exception when creating a PublishedElement.
    /// </summary>
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
            var unused = new PublishedElement(setType1, Guid.NewGuid(), new Dictionary<string, object> { { "prop1", "1234" } }, false, new VariationContext());
        });
    }

    private class CacheConverter1 : IPropertyValueConverter
    {
        private readonly PropertyCacheLevel _cacheLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheConverter1"/> class.
    /// </summary>
    /// <param name="cacheLevel">The cache level to use.</param>
        public CacheConverter1(PropertyCacheLevel cacheLevel) => _cacheLevel = cacheLevel;

        /// <summary>
        /// Gets the number of times the source value has been converted by this <see cref="CacheConverter1"/> instance.
        /// </summary>
        public int SourceConverts { get; private set; }

    /// <summary>
    /// Gets the number of times this converter has performed inter-conversion operations.
    /// </summary>
        public int InterConverts { get; private set; }

    /// <summary>
    /// Determines whether the specified value is considered a valid value at the given property value level.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="level">The property value level to consider.</param>
    /// <returns>True if the value is valid; false if not; null if indeterminate.</returns>
        public bool? IsValue(object value, PropertyValueLevel level)
            => value != null && (!(value is string) || string.IsNullOrWhiteSpace((string)value) == false);

    /// <summary>
    /// Determines whether the specified property type can be converted by this converter.
    /// </summary>
    /// <param name="propertyType">The property type to check.</param>
    /// <returns><c>true</c> if the property type is convertible; otherwise, <c>false</c>.</returns>
        public bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals("Umbraco.Void");

    /// <summary>
    /// Gets the property value type for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>The type of the property value.</returns>
        public Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof(int);

    /// <summary>
    /// Gets the property cache level for the specified property type.
    /// </summary>
    /// <param name="propertyType">The property type to get the cache level for.</param>
    /// <returns>The cache level of the property.</returns>
        public PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => _cacheLevel;

    /// <summary>
    /// Converts the source value to an intermediate representation.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The published property type.</param>
    /// <param name="source">The source value to convert.</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>The intermediate representation of the source value.</returns>
        public object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            SourceConverts++;
            return int.TryParse(source as string, out var i) ? i : 0;
        }

    /// <summary>
    /// Converts an intermediate property value to its final object representation for a published property.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">Metadata describing the property type.</param>
    /// <param name="referenceCacheLevel">The cache level reference for the property value.</param>
    /// <param name="inter">The intermediate value to convert, expected to be an <see cref="int"/>.</param>
    /// <param name="preview">True if the conversion is for preview mode; otherwise, false.</param>
    /// <returns>The converted object value, as an <see cref="int"/>.</returns>
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

    /// <summary>
    /// Converts the intermediate value to its XPath string representation.
    /// </summary>
    /// <param name="owner">The published element that owns the property.</param>
    /// <param name="propertyType">The property type information.</param>
    /// <param name="referenceCacheLevel">The cache level reference.</param>
    /// <param name="inter">The intermediate value to convert (expected to be an <c>int</c>).</param>
    /// <param name="preview">Indicates whether the conversion is for preview mode.</param>
    /// <returns>An object containing the XPath string representation of the intermediate value.</returns>
        public object ConvertIntermediateToXPath(
            IPublishedElement owner,
            IPublishedPropertyType propertyType,
            PropertyCacheLevel referenceCacheLevel,
            object inter,
            bool preview)
            => ((int)inter).ToString();
    }
}
