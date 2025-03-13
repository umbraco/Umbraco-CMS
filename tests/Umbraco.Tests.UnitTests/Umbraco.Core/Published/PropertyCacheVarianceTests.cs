using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Property = Umbraco.Cms.Infrastructure.PublishedCache.Property;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

[TestFixture]
public class PropertyCacheVarianceTests
{
    // This class tests various permutations of property value calculation across variance types and cache levels.
    //
    // Properties contain different "value levels", all of which are cached:
    // 1. The source value               => the "raw" value from the client side editor (it can be different, but it's easiest to think of it like that).
    // 2. The intermediate value         => a "temporary" value that is used to calculate the various "final" values.
    // 3. The object value               => the "final" object value that is exposed in an IPublishedElement output.
    // 4. The XPath value                => a legacy "final" value, don't think too hard on it.
    // 3. The delivery API object value  => the "final" object value that is exposed in the Delivery API.
    //
    // Property values are cached based on a few rules:
    // 1. The property type variation and the parent content type variation determines how the intermediate value is cached.
    //    The effective property variation is a product of both variations, meaning the property type and the content type
    //    variations are combined in an OR.
    //    The rules are as follows:
    //    - ContentVariation.Nothing            => the intermediate value is calculated once and reused across all variants (cultures and segments).
    //    - ContentVariation.Culture            => the intermediate value is calculated per culture and reused across all segments.
    //    - ContentVariation.Segment            => the intermediate value is calculated per segment and reused across all cultures.
    //    - ContentVariation.CultureAndSegment  => the intermediate value is calculated for all invoked culture and segment combinations.
    // 2. The property type cache level (which is usually derived from the property value converter).
    //   - PropertyCacheLevel.Element   => the final values are cached until the parent content item is updated.
    //   - PropertyCacheLevel.Elements  => the final values are cached until the _any_ content item is updated.
    //   - PropertyCacheLevel.Snapshot  => the final values are cached for the duration of the active cache snapshot (i.e. until the end of the current request).
    //   - PropertyCacheLevel.None      => the final values are never cached and will be re-calculated each time they're requested.

    // ### Invariant content type + invariant property type ###
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Nothing,
        PropertyCacheLevel.Element,
        // no variation => the intermediate value is calculated only once
        // cache level => the final value is calculated only once
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Nothing,
        PropertyCacheLevel.Elements,
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Nothing,
        PropertyCacheLevel.Snapshot,
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Nothing,
        PropertyCacheLevel.None,
        // no variation => the intermediate value is calculated once
        // no cache => the final value is calculated for each request (reflects both changes in culture and segments)
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (da-DK:segment1)",
        "en-US:segment2 (da-DK:segment1)",
        "da-DK:segment2 (da-DK:segment1)")]
    // ### Culture variant content type + invariant property type ###
    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Nothing,
        PropertyCacheLevel.Element,
        // culture variation => the intermediate value is calculated per culture (ignores segment changes until a culture changes)
        // cache level => the final value is calculated only once per culture (ignores segment changes until a culture changes)
        // NOTE: in this test, culture changes before segment, so the updated segment is never reflected here
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Nothing,
        PropertyCacheLevel.Elements,
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Nothing,
        PropertyCacheLevel.Snapshot,
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Nothing,
        PropertyCacheLevel.None,
        // culture variation => the intermediate value is calculated per culture (ignores segment changes until a culture changes)
        // no cache => the final value is calculated for each request (reflects both changes in culture and segments)
        // NOTE: in this test, culture changes before segment, so the updated segment is never reflected in the intermediate value here
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment1)",
        "da-DK:segment2 (da-DK:segment1)")]
    // NOTE: As the tests above show, cache levels Element, Elements and Snapshot all yield the same values in this
    //       test, because we are efficiently executing the test in a snapshot. From here on out we're only building
    //       test cases for Element and None.
    // ### Segment variant content type + invariant property type ###
    [TestCase(
        ContentVariation.Segment,
        ContentVariation.Nothing,
        PropertyCacheLevel.Element,
        // segment variation => the intermediate value is calculated per segment (ignores culture changes until a segment changes)
        // cache level => the final value is calculated only once per segment (ignores culture changes until a segment changes)
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "en-US:segment2 (en-US:segment2)")]
    [TestCase(
        ContentVariation.Segment,
        ContentVariation.Nothing,
        PropertyCacheLevel.None,
        // segment variation => the intermediate value is calculated per segment (ignores culture changes until a segment changes)
        // no cache => the final value is calculated for each request (reflects both changes in culture and segments)
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (da-DK:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (en-US:segment2)")]
    // ### Culture and segment variant content type + invariant property type ###
    [TestCase(
        ContentVariation.CultureAndSegment,
        ContentVariation.Nothing,
        PropertyCacheLevel.Element,
        // culture and segment variation => the intermediate value is calculated per culture and segment
        // cache level => the final value is calculated only once per culture and segment (efficiently on every request in this test)
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    [TestCase(
        ContentVariation.CultureAndSegment,
        ContentVariation.Nothing,
        PropertyCacheLevel.None,
        // culture and segment variation => the intermediate value is calculated per culture and segment
        // no cache => the final value is calculated for each request
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    // ### Invariant content type + culture variant property type ###
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Culture,
        PropertyCacheLevel.Element,
        // same behaviour as culture variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Culture,
        PropertyCacheLevel.None,
        // same behaviour as culture variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment1)",
        "da-DK:segment2 (da-DK:segment1)")]
    // ### Invariant content type + segment variant property type ###
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Segment,
        PropertyCacheLevel.Element,
        // same behaviour as segment variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "en-US:segment2 (en-US:segment2)")]
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.Segment,
        PropertyCacheLevel.None,
        // same behaviour as segment variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (da-DK:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (en-US:segment2)")]
    // ### Invariant content type + culture and segment variant property type ###
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.CultureAndSegment,
        PropertyCacheLevel.Element,
        // same behaviour as culture and segment variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    [TestCase(
        ContentVariation.Nothing,
        ContentVariation.CultureAndSegment,
        PropertyCacheLevel.None,
        // same behaviour as culture and segment variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    // ### Culture variant content type + segment variant property type ###
    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Segment,
        PropertyCacheLevel.Element,
        // same behaviour as culture and segment variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Segment,
        PropertyCacheLevel.None,
        // same behaviour as culture and segment variation on content type + no variation on property type, see comments above
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    public void ContentType_PropertyType_Variation_Cache_Values(
        ContentVariation contentTypeVariation,
        ContentVariation propertyTypeVariation,
        PropertyCacheLevel propertyCacheLevel,
        string expectedValue1DaDkSegment1,
        string expectedValue2EnUsSegment1,
        string expectedValue3EnUsSegment2,
        string expectedValue4DaDkSegment2)
    {
        var variationContextCulture = "da-DK";
        var variationContextSegment = "segment1";
        var property = CreateProperty(
            contentTypeVariation,
            propertyTypeVariation,
            propertyCacheLevel,
            () => variationContextCulture,
            () => variationContextSegment);

        Assert.AreEqual(expectedValue1DaDkSegment1, property.GetValue());

        variationContextCulture = "en-US";
        Assert.AreEqual(expectedValue2EnUsSegment1, property.GetValue());

        variationContextSegment = "segment2";
        Assert.AreEqual(expectedValue3EnUsSegment2, property.GetValue());

        variationContextCulture = "da-DK";
        Assert.AreEqual(expectedValue4DaDkSegment2, property.GetValue());
    }

    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Nothing,
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "da-DK:segment1 (da-DK:segment1)")]
    [TestCase(
        ContentVariation.Segment,
        ContentVariation.Nothing,
        "da-DK:segment1 (da-DK:segment1)",
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "en-US:segment2 (en-US:segment2)")]
    [TestCase(
        ContentVariation.Culture,
        ContentVariation.Segment,
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    [TestCase(
        ContentVariation.CultureAndSegment,
        ContentVariation.Nothing,
        "da-DK:segment1 (da-DK:segment1)",
        "en-US:segment1 (en-US:segment1)",
        "en-US:segment2 (en-US:segment2)",
        "da-DK:segment2 (da-DK:segment2)")]
    public void ContentType_PropertyType_Variation_Are_Interchangeable(
        ContentVariation variation1,
        ContentVariation variation2,
        string expectedValue1DaDkSegment1,
        string expectedValue2EnUsSegment1,
        string expectedValue3EnUsSegment2,
        string expectedValue4DaDkSegment2)
    {
        var scenarios = new[]
        {
            new { ContentTypeVariation = variation1, PropertyTypeVariation = variation2 },
            new { ContentTypeVariation = variation2, PropertyTypeVariation = variation1 }
        };

        foreach (var scenario in scenarios)
        {
            var variationContextCulture = "da-DK";
            var variationContextSegment = "segment1";
            var property = CreateProperty(
                scenario.ContentTypeVariation,
                scenario.PropertyTypeVariation,
                PropertyCacheLevel.Element,
                () => variationContextCulture,
                () => variationContextSegment);

            Assert.AreEqual(expectedValue1DaDkSegment1, property.GetValue());

            variationContextCulture = "en-US";
            Assert.AreEqual(expectedValue2EnUsSegment1, property.GetValue());

            variationContextSegment = "segment2";
            Assert.AreEqual(expectedValue3EnUsSegment2, property.GetValue());

            variationContextCulture = "da-DK";
            Assert.AreEqual(expectedValue4DaDkSegment2, property.GetValue());
        }
    }

    /// <summary>
    /// Creates a new property with a mocked publishedSnapshotAccessor that uses a VariationContext that reads culture and segment information from the passed in functions.
    /// </summary>
    private Property CreateProperty(ContentVariation contentTypeVariation, ContentVariation propertyTypeVariation, PropertyCacheLevel propertyTypeCacheLevel, Func<string> getCulture, Func<string> getSegment)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.PropertyTypes).Returns(Array.Empty<IPublishedPropertyType>());
        contentType.SetupGet(c => c.Variations).Returns(contentTypeVariation);

        var contentNode = new ContentNode(123, Guid.NewGuid(), contentType.Object, 1, string.Empty, 1, 1, DateTime.Now, 1);
        var contentData = new ContentData("bla", "bla", 1, DateTime.Now, 1, 1, true, new Dictionary<string, PropertyData[]>(), null);

        var elementCache = new FastDictionaryAppCache();
        var snapshotCache = new FastDictionaryAppCache();
        var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
        publishedSnapshotMock.SetupGet(p => p.ElementsCache).Returns(elementCache);
        publishedSnapshotMock.SetupGet(p => p.SnapshotCache).Returns(snapshotCache);

        var publishedSnapshot = publishedSnapshotMock.Object;
        var publishedSnapshotAccessor = new Mock<IPublishedSnapshotAccessor>();
        publishedSnapshotAccessor.Setup(p => p.TryGetPublishedSnapshot(out publishedSnapshot)).Returns(true);

        var variationContextAccessorMock = new Mock<IVariationContextAccessor>();
        variationContextAccessorMock
            .SetupGet(mock => mock.VariationContext)
            .Returns(() => new VariationContext(getCulture(), getSegment()));

        var content = new PublishedContent(
            contentNode,
            contentData,
            publishedSnapshotAccessor.Object,
            variationContextAccessorMock.Object,
            Mock.Of<IPublishedModelFactory>());

        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(p => p.CacheLevel).Returns(propertyTypeCacheLevel);
        propertyType.SetupGet(p => p.DeliveryApiCacheLevel).Returns(propertyTypeCacheLevel);
        propertyType.SetupGet(p => p.Variations).Returns(propertyTypeVariation);
        propertyType
            .Setup(p => p.ConvertSourceToInter(It.IsAny<IPublishedElement>(), It.IsAny<object?>(), It.IsAny<bool>()))
            .Returns(() => $"{getCulture()}:{getSegment()}");
        propertyType
            .Setup(p => p.ConvertInterToObject(It.IsAny<IPublishedElement>(), It.IsAny<PropertyCacheLevel>(), It.IsAny<object?>(), It.IsAny<bool>()))
            .Returns((IPublishedElement _, PropertyCacheLevel _, object? inter, bool _) => $"{getCulture()}:{getSegment()} ({inter})" );

        return new Property(propertyType.Object, content, publishedSnapshotAccessor.Object);
    }
}
