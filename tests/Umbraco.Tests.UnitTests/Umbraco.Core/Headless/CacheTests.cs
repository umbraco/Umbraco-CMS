using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

[TestFixture]
public class CacheTests : HeadlessTests
{
    [TestCase(PropertyCacheLevel.Snapshot, 1)]
    [TestCase(PropertyCacheLevel.Elements, 1)]
    [TestCase(PropertyCacheLevel.Element, 1)]
    [TestCase(PropertyCacheLevel.None, 4)]
    public void PublishedElementProperty_CachesHeadlessValueConversion(PropertyCacheLevel cacheLevel, int expectedConverterHits)
    {
        HeadlessPropertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        var element = new Mock<IPublishedElement>();

        var prop1 = new PublishedElementPropertyBase(HeadlessPropertyType, element.Object, false, cacheLevel);
        var result = prop1.GetHeadlessValue();

        Assert.NotNull(result);
        Assert.AreEqual("Headless value", result);

        result = prop1.GetHeadlessValue();
        result = prop1.GetHeadlessValue();
        result = prop1.GetHeadlessValue();

        HeadlessPropertyValueConverter.Verify(
            converter => converter.ConvertIntermediateToHeadlessObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>()),
            Times.Exactly(expectedConverterHits));
    }
}
