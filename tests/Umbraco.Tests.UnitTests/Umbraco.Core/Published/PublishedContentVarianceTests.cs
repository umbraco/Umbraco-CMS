using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

[TestFixture]
public class PublishedContentVarianceTests
{
    private const string PropertyTypeAlias = "theProperty";
    private const string DaCulture = "da-DK";
    private const string EnCulture = "en-US";
    private const string Segment1 = "segment1";
    private const string Segment2 = "segment2";

    [Test]
    public void No_Content_Variation_Can_Get_Invariant_Property()
    {
        var content = CreatePublishedContent(ContentVariation.Nothing, ContentVariation.Nothing);
        var value = GetPropertyValue(content);
        Assert.AreEqual("Invariant property value", value);
    }

    [TestCase(DaCulture)]
    [TestCase(EnCulture)]
    [TestCase("")]
    public void Content_Culture_Variation_Can_Get_Invariant_Property(string culture)
    {
        var content = CreatePublishedContent(ContentVariation.Culture, ContentVariation.Nothing, variationContextCulture: culture);
        var value = GetPropertyValue(content);
        Assert.AreEqual("Invariant property value", value);
    }

    [TestCase(Segment1)]
    [TestCase(Segment2)]
    [TestCase("")]
    public void Content_Segment_Variation_Can_Get_Invariant_Property(string segment)
    {
        var content = CreatePublishedContent(ContentVariation.Culture, ContentVariation.Nothing, variationContextSegment: segment);
        var value = GetPropertyValue(content);
        Assert.AreEqual("Invariant property value", value);
    }

    [TestCase(DaCulture, "DaDk property value")]
    [TestCase(EnCulture, "EnUs property value")]
    public void Content_Culture_Variation_Can_Get_Culture_Variant_Property(string culture, string expectedValue)
    {
        var content = CreatePublishedContent(ContentVariation.Culture, ContentVariation.Culture, variationContextCulture: culture);
        var value = GetPropertyValue(content);
        Assert.AreEqual(expectedValue, value);
    }

    [TestCase(Segment1, "Segment1 property value")]
    [TestCase(Segment2, "Segment2 property value")]
    public void Content_Segment_Variation_Can_Get_Segment_Variant_Property(string segment, string expectedValue)
    {
        var content = CreatePublishedContent(ContentVariation.Segment, ContentVariation.Segment, variationContextSegment: segment);
        var value = GetPropertyValue(content);
        Assert.AreEqual(expectedValue, value);
    }

    [TestCase(DaCulture, Segment1, "DaDk Segment1 property value")]
    [TestCase(DaCulture, Segment2, "DaDk Segment2 property value")]
    [TestCase(EnCulture, Segment1, "EnUs Segment1 property value")]
    [TestCase(EnCulture, Segment2, "EnUs Segment2 property value")]
    public void Content_Culture_And_Segment_Variation_Can_Get_Culture_And_Segment_Variant_Property(string culture, string segment, string expectedValue)
    {
        var content = CreatePublishedContent(ContentVariation.CultureAndSegment, ContentVariation.CultureAndSegment, variationContextCulture: culture, variationContextSegment: segment);
        var value = GetPropertyValue(content);
        Assert.AreEqual(expectedValue, value);
    }

    private object? GetPropertyValue(IPublishedContent content) => content.GetProperty(PropertyTypeAlias)!.GetValue();

    private IPublishedContent CreatePublishedContent(ContentVariation contentTypeVariation, ContentVariation propertyTypeVariation, string? variationContextCulture = null, string? variationContextSegment = null)
    {
        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(p => p.Alias).Returns(PropertyTypeAlias);
        propertyType.SetupGet(p => p.CacheLevel).Returns(PropertyCacheLevel.None);
        propertyType.SetupGet(p => p.DeliveryApiCacheLevel).Returns(PropertyCacheLevel.None);
        propertyType.SetupGet(p => p.Variations).Returns(propertyTypeVariation);
        propertyType
            .Setup(p => p.ConvertSourceToInter(It.IsAny<IPublishedElement>(), It.IsAny<object?>(), It.IsAny<bool>()))
            .Returns((IPublishedElement _, object? source, bool _) => source);
        propertyType
            .Setup(p => p.ConvertInterToObject(It.IsAny<IPublishedElement>(), It.IsAny<PropertyCacheLevel>(), It.IsAny<object?>(), It.IsAny<bool>()))
            .Returns((IPublishedElement _, PropertyCacheLevel _, object? inter, bool _) => inter);

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.PropertyTypes).Returns(new[] { propertyType.Object });
        contentType.SetupGet(c => c.Variations).Returns(contentTypeVariation);

        var propertyData = new List<PropertyData>();

        switch (propertyTypeVariation)
        {
            case ContentVariation.Culture:
                propertyData.Add(CreatePropertyData("EnUs property value", culture: EnCulture));
                propertyData.Add(CreatePropertyData("DaDk property value", culture: DaCulture));
                break;
            case ContentVariation.Segment:
                propertyData.Add(CreatePropertyData("Segment1 property value", segment: Segment1));
                propertyData.Add(CreatePropertyData("Segment2 property value", segment: Segment2));
                break;
            case ContentVariation.CultureAndSegment:
                propertyData.Add(CreatePropertyData("EnUs Segment1 property value", culture: EnCulture, segment: Segment1));
                propertyData.Add(CreatePropertyData("EnUs Segment2 property value", culture: EnCulture, segment: Segment2));
                propertyData.Add(CreatePropertyData("DaDk Segment1 property value", culture: DaCulture, segment: Segment1));
                propertyData.Add(CreatePropertyData("DaDk Segment2 property value", culture: DaCulture, segment: Segment2));
                break;
            case ContentVariation.Nothing:
                propertyData.Add(CreatePropertyData("Invariant property value"));
                break;
        }

        var properties = new Dictionary<string, PropertyData[]> { { PropertyTypeAlias, propertyData.ToArray() } };

        var contentNode = new ContentNode(123, Guid.NewGuid(), contentType.Object, 1, string.Empty, 1, 1, DateTime.Now, 1);
        var contentData = new ContentData("bla", "bla", 1, DateTime.Now, 1, 1, true, properties, null);

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
            .Returns(() => new VariationContext(variationContextCulture, variationContextSegment));

        return new PublishedContent(
            contentNode,
            contentData,
            publishedSnapshotAccessor.Object,
            variationContextAccessorMock.Object,
            Mock.Of<IPublishedModelFactory>());

        PropertyData CreatePropertyData(string value, string? culture = null, string? segment = null)
            => new() { Culture = culture ?? string.Empty, Segment = segment ?? string.Empty, Value = value };
    }
}
