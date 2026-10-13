using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

[TestFixture]
public class PublishedMediaStatusFilteringServiceTests
{
    // Matches IMediaCacheService.TryGetCached so Moq can bind the out parameter.
    private delegate bool TryGetCachedCallback(Guid key, out IPublishedContent? content);

    [Test]
    public void FilterAvailable_IsLazy_WarmCache_ShortCircuitsWithoutBatching()
    {
        var (sut, items, batchedKeys, serviceMock) = SetupCounting(warm: true);

        IPublishedContent? first = sut.FilterAvailable(items.Keys, null).FirstOrDefault();

        Assert.IsNotNull(first);

        // An all-L0-hit chunk must be served synchronously — the batched read is never engaged.
        Assert.IsEmpty(batchedKeys);
        serviceMock.Verify(s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>()), Times.Never);
    }

    [Test]
    public void FilterAvailable_IsLazy_FirstOrDefaultMaterialisesOnlyOneItem()
    {
        var (sut, items, batchedKeys, _) = SetupCounting(warm: false);

        IPublishedContent? first = sut.FilterAvailable(items.Keys, null).FirstOrDefault();

        Assert.IsNotNull(first);

        // Slow-start's first chunk is a single key, so exactly one item is materialised.
        Assert.AreEqual(1, batchedKeys.Count);
    }

    [Test]
    public void FilterAvailable_IsLazy_TakeMaterialisesOnlyRequestedItems()
    {
        var (sut, items, batchedKeys, _) = SetupCounting(warm: false);

        IPublishedContent[] taken = sut.FilterAvailable(items.Keys, null).Take(3).ToArray();

        Assert.AreEqual(3, taken.Length);

        // Chunks of 1 then 2 cover the three requested items; the remaining seven are never materialised.
        Assert.AreEqual(3, batchedKeys.Count);
    }

    [Test]
    public void FilterAvailable_IsLazy_FullEnumerationMaterialisesAllItemsInFewBatches()
    {
        var (sut, items, batchedKeys, serviceMock) = SetupCounting(warm: false);

        IPublishedContent[] all = sut.FilterAvailable(items.Keys, null).ToArray();

        Assert.AreEqual(items.Count, all.Length);

        // Every item is materialised exactly once...
        Assert.AreEqual(items.Count, batchedKeys.Count);
        Assert.AreEqual(items.Count, batchedKeys.Distinct().Count());

        // ...but collapsed into a handful of batched reads rather than one per item.
        serviceMock.Verify(s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>()), Times.AtMost(5));
    }

    // When warm, TryGetCached serves every key from L0; otherwise every key misses L0 and is routed
    // through the batched GetByKeysAsync, whose requested keys are recorded in the returned list.
    private static (
        PublishedMediaStatusFilteringService Service,
        Dictionary<Guid, IPublishedContent> Items,
        List<Guid> BatchedKeys,
        Mock<IMediaCacheService> ServiceMock)
        SetupCounting(bool warm)
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

        var batchedKeys = new List<Guid>();
        var serviceMock = new Mock<IMediaCacheService>();
        serviceMock
            .Setup(s => s.TryGetCached(It.IsAny<Guid>(), out It.Ref<IPublishedContent?>.IsAny))
            .Returns(new TryGetCachedCallback((Guid key, out IPublishedContent? content) =>
            {
                if (warm && items.TryGetValue(key, out IPublishedContent? item))
                {
                    content = item;
                    return true;
                }

                content = null;
                return false;
            }));
        serviceMock
            .Setup(s => s.GetByKeysAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync((IReadOnlyCollection<Guid> keys) =>
            {
                batchedKeys.AddRange(keys);
                return (IReadOnlyList<IPublishedContent>)keys
                    .Select(k => items.TryGetValue(k, out IPublishedContent? item) ? item : null)
                    .WhereNotNull()
                    .ToArray();
            });

        var service = new PublishedMediaStatusFilteringService(Mock.Of<IPublishedMediaCache>(), serviceMock.Object);
        return (service, items, batchedKeys, serviceMock);
    }
}
