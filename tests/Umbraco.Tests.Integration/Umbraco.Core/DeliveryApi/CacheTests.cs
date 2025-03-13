using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.DeliveryApi;

[TestFixture]
public class CacheTests
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
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.PropertyTypes).Returns(Array.Empty<IPublishedPropertyType>());

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

        var content = new PublishedContent(
            contentNode,
            contentData,
            publishedSnapshotAccessor.Object,
            Mock.Of<IVariationContextAccessor>(),
            Mock.Of<IPublishedModelFactory>());

        var propertyType = new Mock<IPublishedPropertyType>();
        var invocationCount = 0;
        propertyType.SetupGet(p => p.CacheLevel).Returns(cacheLevel);
        propertyType.SetupGet(p => p.DeliveryApiCacheLevel).Returns(cacheLevel);
        propertyType.SetupGet(p => p.DeliveryApiCacheLevelForExpansion).Returns(cacheLevel);
        propertyType
            .Setup(p => p.ConvertInterToDeliveryApiObject(It.IsAny<IPublishedElement>(), It.IsAny<PropertyCacheLevel>(), It.IsAny<object?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(() => $"Delivery API value: {++invocationCount}");

        var prop1 = new Property(propertyType.Object, content, publishedSnapshotAccessor.Object);
        var results = new List<string>();
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());
        results.Add(prop1.GetDeliveryApiValue(expanding)!.ToString());

        Assert.AreEqual("Delivery API value: 1", results.First());
        Assert.AreEqual(expectedConverterHits, results.Distinct().Count());

        propertyType.Verify(
            p => p.ConvertInterToDeliveryApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Exactly(expectedConverterHits));
    }
}
