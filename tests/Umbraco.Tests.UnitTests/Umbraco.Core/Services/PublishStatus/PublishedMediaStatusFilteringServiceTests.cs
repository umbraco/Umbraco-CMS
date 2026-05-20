using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

[TestFixture]
public class PublishedMediaStatusFilteringServiceTests
{
    [Test]
    public void FilterAvailable_IsLazy_TakeOnlyFetchesRequestedItemsFromCache()
    {
        var (sut, items, cacheMock) = SetupCounting();

        IPublishedContent[] taken = sut.FilterAvailable(items.Keys, null).Take(3).ToArray();

        Assert.AreEqual(3, taken.Length);
        cacheMock.Verify(c => c.GetById(It.IsAny<Guid>()), Times.Exactly(3));
    }

    [Test]
    public void FilterAvailable_IsLazy_FirstOrDefaultOnlyFetchesOneItemFromCache()
    {
        var (sut, items, cacheMock) = SetupCounting();

        IPublishedContent? first = sut.FilterAvailable(items.Keys, null).FirstOrDefault();

        Assert.IsNotNull(first);
        cacheMock.Verify(c => c.GetById(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public void FilterAvailable_IsLazy_FullEnumerationFetchesAllItemsFromCache()
    {
        var (sut, items, cacheMock) = SetupCounting();

        IPublishedContent[] all = sut.FilterAvailable(items.Keys, null).ToArray();

        Assert.AreEqual(items.Count, all.Length);
        cacheMock.Verify(c => c.GetById(It.IsAny<Guid>()), Times.Exactly(items.Count));
    }

    private (
        PublishedMediaStatusFilteringService Service,
        Dictionary<Guid, IPublishedContent> Items,
        Mock<IPublishedMediaCache> CacheMock)
        SetupCounting()
    {
        var items = new Dictionary<Guid, IPublishedContent>();
        for (var i = 0; i < 10; i++)
        {
            var content = new Mock<IPublishedContent>();
            var key = Guid.NewGuid();
            content.SetupGet(c => c.Key).Returns(key);
            content.SetupGet(c => c.Id).Returns(i);
            items[key] = content.Object;
        }

        var cacheMock = new Mock<IPublishedMediaCache>();
        cacheMock
            .Setup(c => c.GetById(It.IsAny<Guid>()))
            .Returns((Guid key) => items.TryGetValue(key, out IPublishedContent? item) ? item : null);

        var service = new PublishedMediaStatusFilteringService(cacheMock.Object);
        return (service, items, cacheMock);
    }
}
