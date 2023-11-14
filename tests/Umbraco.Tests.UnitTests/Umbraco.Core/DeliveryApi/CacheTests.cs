using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class CacheTests : DeliveryApiTests
{
    [TestCase(PropertyCacheLevel.Snapshot, false, 1)]
    [TestCase(PropertyCacheLevel.Snapshot, true, 1)]
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
            It.IsAny<bool>())
        ).Returns(() => $"Delivery API value: {++invocationCount}");
        propertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        propertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevelForExpansion(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        var propertyType = SetupPublishedPropertyType(propertyValueConverter.Object, "something", "Some.Thing");

        var element = new Mock<IPublishedElement>();

        var prop1 = new PublishedElementPropertyBase(propertyType, element.Object, false, cacheLevel);

        var results = new List<string>();
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());

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
}
