using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class CacheTests : DeliveryApiTests
{
    [TestCase(PropertyCacheLevel.Elements, false, 1)]
    [TestCase(PropertyCacheLevel.Elements, true, 1)]
    [TestCase(PropertyCacheLevel.Element, false, 1)]
    [TestCase(PropertyCacheLevel.Element, true, 1)]
    [TestCase(PropertyCacheLevel.None, false, 4)]
    [TestCase(PropertyCacheLevel.None, true, 4)]
    public void PublishedElementProperty_CachesDeliveryApiValueConversion(PropertyCacheLevel cacheLevel, bool expanding, int expectedConverterHits)
    {
        var propertyValueConverter = new Mock<IDeliveryApiPropertyValueConverter>();
        var invocationCount = 0;
        propertyValueConverter.Setup(p => p.ConvertIntermediateToDeliveryApiObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>(),
            It.IsAny<bool>())).Returns(() => $"Delivery API value: {++invocationCount}");
        propertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        propertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevelForExpansion(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        var propertyType = SetupPublishedPropertyType(propertyValueConverter.Object, "something", "Some.Thing");

        var elementType = new Mock<IPublishedContentType>();
        elementType.SetupGet(e => e.ItemType).Returns(PublishedItemType.Element);
        var element = new Mock<IPublishedElement>();
        element.SetupGet(e => e.ContentType).Returns(elementType.Object);

        var propertyData = new PropertyData { Culture = "abc", Segment = string.Empty, Value = "n/a" };

        var prop1 = new PublishedProperty(propertyType, element.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), cacheLevel);

        var results = new List<string>
        {
            prop1.GetDeliveryApiValue(expanding)!.ToString(),
            prop1.GetDeliveryApiValue(expanding)!.ToString(),
            prop1.GetDeliveryApiValue(expanding)!.ToString(),
            prop1.GetDeliveryApiValue(expanding)!.ToString()
        };

        Assert.AreEqual("Delivery API value: 1", results.First());
        Assert.AreEqual(expectedConverterHits, results.Distinct().Count());

        propertyValueConverter.Verify(
            converter => converter.ConvertIntermediateToDeliveryApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Exactly(expectedConverterHits));
    }

    [TestCase(PropertyCacheLevel.Elements, "value: en-US", "value: en-US")]
    [TestCase(PropertyCacheLevel.Element, "value: en-US", "value: da-DK")]
    [TestCase(PropertyCacheLevel.None, "value: en-US", "value: da-DK")]
    public void PublishedElementProperty_DeliveryApiValue_MustNotLeakAmbientCultureAcrossSeparateInstances_WhenConverterDependsOnAmbientContext(
        PropertyCacheLevel cacheLevel, string expectedFirstResult, string expectedSecondResult)
    {
        // Simulates a converter (like the Rich Text block converter) whose Delivery API output depends on the
        // ambient VariationContext even though the property itself is invariant - e.g. it embeds culture-variant
        // blocks. Two SEPARATE PublishedProperty instances (as a fresh request would build) share the same
        // backing elements cache, mirroring how the elements cache persists across real, separate HTTP requests.
        var variationContextAccessor = new TestVariationContextAccessor();

        var propertyValueConverter = new Mock<IDeliveryApiPropertyValueConverter>();
        propertyValueConverter
            .Setup(p => p.ConvertIntermediateToDeliveryApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(() => $"value: {variationContextAccessor.VariationContext?.Culture}");
        propertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        propertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevelForExpansion(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        // Both the property and its owning element type are invariant, matching a Rich Text property nested
        // inside an invariant Block List element.
        var propertyType = SetupPublishedPropertyType(propertyValueConverter.Object, "richText", "Rich.Text");

        var elementType = new Mock<IPublishedContentType>();
        elementType.SetupGet(e => e.ItemType).Returns(PublishedItemType.Element);
        elementType.SetupGet(e => e.Variations).Returns(ContentVariation.Nothing);
        var element = new Mock<IPublishedElement>();
        element.SetupGet(e => e.ContentType).Returns(elementType.Object);
        element.SetupGet(e => e.Key).Returns(Guid.NewGuid());

        var propertyData = new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = "n/a" };
        var elementsCache = new ElementsDictionaryAppCache();

        // First "request": ambient culture is en-US, resolved via a fresh PublishedProperty instance, exactly
        // as a fresh HTTP request would build a fresh object graph.
        variationContextAccessor.VariationContext = new VariationContext("en-US");
        var firstRequestProperty = new PublishedProperty(propertyType, element.Object, variationContextAccessor, CreatePropertyRenderingContextAccessor(), false, [propertyData], elementsCache, cacheLevel);
        var firstResult = firstRequestProperty.GetDeliveryApiValue(false)!.ToString();

        // A second, separate request for a different ambient culture must not see the first request's value.
        variationContextAccessor.VariationContext = new VariationContext("da-DK");
        var secondRequestProperty = new PublishedProperty(propertyType, element.Object, variationContextAccessor, CreatePropertyRenderingContextAccessor(), false, [propertyData], elementsCache, cacheLevel);
        var secondResult = secondRequestProperty.GetDeliveryApiValue(false)!.ToString();

        Assert.AreEqual(expectedFirstResult, firstResult);
        Assert.AreEqual(expectedSecondResult, secondResult);
    }
}
