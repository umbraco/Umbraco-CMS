using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class CacheTests : ContentApiTests
{
    [TestCase(PropertyCacheLevel.Snapshot, false, 1)]
    [TestCase(PropertyCacheLevel.Snapshot, true, 1)]
    [TestCase(PropertyCacheLevel.Elements, false, 1)]
    [TestCase(PropertyCacheLevel.Elements, true, 1)]
    [TestCase(PropertyCacheLevel.Element, false, 1)]
    [TestCase(PropertyCacheLevel.Element, true, 1)]
    [TestCase(PropertyCacheLevel.None, false, 4)]
    [TestCase(PropertyCacheLevel.None, true, 4)]
    public void PublishedElementProperty_CachesContentApiValueConversion(PropertyCacheLevel cacheLevel, bool expanding, int expectedConverterHits)
    {
        var propertyValueConverter = new Mock<IContentApiPropertyValueConverter>();
        var invocationCount = 0;
        propertyValueConverter.Setup(p => p.ConvertIntermediateToContentApiObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns(() => $"Content API value: {++invocationCount}");
        propertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        propertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetPropertyContentApiCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        var propertyType = SetupPublishedPropertyType(propertyValueConverter.Object, "something", "Some.Thing");

        var element = new Mock<IPublishedElement>();

        var prop1 = new PublishedElementPropertyBase(propertyType, element.Object, false, cacheLevel);

        var results = new List<string>();
        results.Add(prop1.GetContentApiValue(expanding)!.ToString());
        results.Add(prop1.GetContentApiValue(expanding)!.ToString());
        results.Add(prop1.GetContentApiValue(expanding)!.ToString());
        results.Add(prop1.GetContentApiValue(expanding)!.ToString());

        Assert.AreEqual("Content API value: 1", results.First());
        Assert.AreEqual(expectedConverterHits, results.Distinct().Count());

        propertyValueConverter.Verify(
            converter => converter.ConvertIntermediateToContentApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>()),
            Times.Exactly(expectedConverterHits));
    }
}
