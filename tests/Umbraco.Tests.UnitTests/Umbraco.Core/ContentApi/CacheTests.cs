using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class CacheTests : ContentApiTests
{
    [TestCase(PropertyCacheLevel.Snapshot, 1)]
    [TestCase(PropertyCacheLevel.Elements, 1)]
    [TestCase(PropertyCacheLevel.Element, 1)]
    [TestCase(PropertyCacheLevel.None, 4)]
    public void PublishedElementProperty_CachesContentApiValueConversion(PropertyCacheLevel cacheLevel, int expectedConverterHits)
    {
        ContentApiPropertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        var element = new Mock<IPublishedElement>();

        var prop1 = new PublishedElementPropertyBase(ContentApiPropertyType, element.Object, false, cacheLevel);
        var result = prop1.GetContentApiValue();

        Assert.NotNull(result);
        Assert.AreEqual("Content API value", result);

        result = prop1.GetContentApiValue();
        result = prop1.GetContentApiValue();
        result = prop1.GetContentApiValue();

        ContentApiPropertyValueConverter.Verify(
            converter => converter.ConvertIntermediateToContentApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>()),
            Times.Exactly(expectedConverterHits));
    }
}
