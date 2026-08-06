using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class TieredResolverTests
{
    private static readonly int[] _firstFiveIds = [0, 1, 2, 3, 4];
    private static readonly int[] _duplicateKeyResultIds = [0, 1, 1, 2];

    [Test]
    public async Task ResolveAsync_AllCached_ReturnsAllInOrder_WithoutMaterialising()
    {
        var (keys, items) = BuildItems(10);
        ResolveItemsAsyncDelegate<Guid, IPublishedContent> warmTier = WarmFromAsync(items);
        var (materialiseTier, calls) = MaterialiserAsync(items);

        IReadOnlyList<IPublishedContent> result = await TieredResolver.ResolveAsync(keys, warmTier, materialiseTier);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(10, result.Count);
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));

            // All served from the first tier — the second tier is never invoked.
            Assert.IsEmpty(calls);
        });
    }

    [Test]
    public async Task ResolveAsync_NoneCached_MaterialisesEverythingInOneBatch()
    {
        var (keys, items) = BuildItems(10);
        var (materialiseTier, calls) = MaterialiserAsync(items);

        IReadOnlyList<IPublishedContent> result = await TieredResolver.ResolveAsync(keys, AllMissAsync(), materialiseTier);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(10, result.Count);
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));

            // No chunking here: the whole set the first tier missed goes to the second tier at once.
            Assert.AreEqual(1, calls.Count);
            Assert.AreEqual(10, calls[0].Count);
        });
    }

    [Test]
    public async Task ResolveAsync_MixedCacheHits_PreservesInputOrder()
    {
        var (keys, items) = BuildItems(10);

        var warm = items.Where(kvp => kvp.Value.Id % 2 == 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var (materialiseTier, _) = MaterialiserAsync(items);

        IReadOnlyList<IPublishedContent> result = await TieredResolver.ResolveAsync(keys, WarmFromAsync(warm), materialiseTier);

        CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));
    }

    [Test]
    public async Task ResolveAsync_OmitsKeysThatResolveToNothing()
    {
        var (keys, items) = BuildItems(10);

        var backing = items.Where(kvp => kvp.Value.Id < 5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var (materialiseTier, _) = MaterialiserAsync(backing);

        IReadOnlyList<IPublishedContent> result = await TieredResolver.ResolveAsync(keys, AllMissAsync(), materialiseTier);

        CollectionAssert.AreEqual(_firstFiveIds, result.Select(x => x.Id));
    }

    [Test]
    public async Task ResolveAsync_EmptyInput_ReturnsEmpty()
    {
        var (materialiseTier, calls) = MaterialiserAsync(new Dictionary<Guid, IPublishedContent>());

        IReadOnlyList<IPublishedContent> result = await TieredResolver.ResolveAsync(Array.Empty<Guid>(), AllMissAsync(), materialiseTier);

        Assert.Multiple(() =>
        {
            Assert.IsEmpty(result);
            Assert.IsEmpty(calls);
        });
    }

    [Test]
    public async Task ResolveAsync_DuplicateKeys_ResolveToSameItemAtEveryOccurrence()
    {
        var (keys, items) = BuildItems(3);
        Guid repeatedKey = keys[1];
        Guid[] keysWithDuplicate = [keys[0], repeatedKey, repeatedKey, keys[2]];

        // A batched lookup API is expected to preserve input multiplicity, not collapse duplicates.
        IReadOnlyList<IPublishedContent> result = await TieredResolver.ResolveAsync(keysWithDuplicate, WarmFromAsync(items));

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, result.Count);
            CollectionAssert.AreEqual(_duplicateKeyResultIds, result.Select(x => x.Id));
        });
    }

    private static (List<Guid> Keys, Dictionary<Guid, IPublishedContent> Items) BuildItems(int count)
    {
        var keys = new List<Guid>(count);
        var items = new Dictionary<Guid, IPublishedContent>(count);
        for (var i = 0; i < count; i++)
        {
            Guid key = Guid.NewGuid();
            var mock = new Mock<IPublishedContent>();
            mock.SetupGet(x => x.Key).Returns(key);
            mock.SetupGet(x => x.Id).Returns(i);
            keys.Add(key);
            items[key] = mock.Object;
        }

        return (keys, items);
    }

    private static ResolveItemsAsyncDelegate<Guid, IPublishedContent> WarmFromAsync(Dictionary<Guid, IPublishedContent> warm)
        => (keys, results) =>
        {
            foreach (Guid key in keys)
            {
                if (warm.TryGetValue(key, out IPublishedContent? cached))
                {
                    results[key] = cached;
                }
            }

            return Task.CompletedTask;
        };

    private static ResolveItemsAsyncDelegate<Guid, IPublishedContent> AllMissAsync()
        => (_, _) => Task.CompletedTask;

    // A batched materialising tier backed by the given store, recording the keys of each invocation so
    // tests can assert how much was materialised.
    private static (ResolveItemsAsyncDelegate<Guid, IPublishedContent> Materialise, List<IReadOnlyList<Guid>> Calls) MaterialiserAsync(
        Dictionary<Guid, IPublishedContent> store)
    {
        var calls = new List<IReadOnlyList<Guid>>();
        ResolveItemsAsyncDelegate<Guid, IPublishedContent> materialise = (keys, results) =>
        {
            calls.Add(keys.ToArray());

            foreach (Guid key in keys)
            {
                if (store.TryGetValue(key, out IPublishedContent? item))
                {
                    results[key] = item;
                }
            }

            return Task.CompletedTask;
        };

        return (materialise, calls);
    }
}
