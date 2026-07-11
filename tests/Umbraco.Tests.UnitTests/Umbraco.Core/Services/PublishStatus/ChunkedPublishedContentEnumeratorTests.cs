using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.PublishStatus;

[TestFixture]
public class ChunkedPublishedContentEnumeratorTests
{
    [Test]
    public void Enumerate_AllCached_ReturnsAllInOrder_WithoutMaterialising()
    {
        var (keys, items) = BuildItems(10);
        TryGetCachedDelegate tryGetCached = WarmFrom(items); // everything in L0
        var (materialise, calls) = Materialiser(items);

        IPublishedContent[] result = ChunkedPublishedContentEnumerator
            .Enumerate(keys, tryGetCached, materialise, predicate: null)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(10, result.Length);
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));

            // All served from the sync L0 probe — the batched materialiser is never invoked.
            Assert.IsEmpty(calls);
        });
    }

    [Test]
    public void Enumerate_NoneCached_MaterialisesAllInOrder_InAFewBatches()
    {
        var (keys, items) = BuildItems(10);
        TryGetCachedDelegate tryGetCached = AllMiss();
        var (materialise, calls) = Materialiser(items);

        IPublishedContent[] result = ChunkedPublishedContentEnumerator
            .Enumerate(keys, tryGetCached, materialise, predicate: null)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(10, result.Length);
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));

            // Every key materialised exactly once...
            Guid[] requested = calls.SelectMany(c => c).ToArray();
            Assert.AreEqual(10, requested.Length);
            Assert.AreEqual(10, requested.Distinct().Count());

            // ...but collapsed into a handful of batches (slow-start: 1, 2, 4, 3), not one per item.
            Assert.That(calls.Count, Is.LessThanOrEqualTo(5));
        });
    }

    [Test]
    public void Enumerate_FirstOnly_MaterialisesSingleItem()
    {
        var (keys, items) = BuildItems(10);
        var (materialise, calls) = Materialiser(items);

        IPublishedContent? first = ChunkedPublishedContentEnumerator
            .Enumerate(keys, AllMiss(), materialise, predicate: null)
            .FirstOrDefault();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(first);
            Assert.AreEqual(0, first!.Id);
            Assert.AreEqual(1, calls.SelectMany(c => c).Count());
        });
    }

    [Test]
    public void Enumerate_Take_MaterialisesOnlyRequested()
    {
        var (keys, items) = BuildItems(10);
        var (materialise, calls) = Materialiser(items);

        IPublishedContent[] taken = ChunkedPublishedContentEnumerator
            .Enumerate(keys, AllMiss(), materialise, predicate: null)
            .Take(3)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, taken.Length);

            // Chunks of 1 then 2 cover the three requested items; the rest are never materialised.
            Assert.AreEqual(3, calls.SelectMany(c => c).Count());
        });
    }

    [Test]
    public void Enumerate_AppliesPredicate()
    {
        var (keys, items) = BuildItems(10);
        var (materialise, _) = Materialiser(items);

        IPublishedContent[] result = ChunkedPublishedContentEnumerator
            .Enumerate(keys, WarmFrom(items), materialise, x => x.Id % 2 == 0)
            .ToArray();

        CollectionAssert.AreEqual(new[] { 0, 2, 4, 6, 8 }, result.Select(x => x.Id));
    }

    [Test]
    public void Enumerate_MixedCacheHits_PreservesInputOrder()
    {
        var (keys, items) = BuildItems(10);

        // Even-index items are warm in L0; odd-index items must be batch-materialised.
        var warm = items.Where(kvp => kvp.Value.Id % 2 == 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var (materialise, _) = Materialiser(items);

        IPublishedContent[] result = ChunkedPublishedContentEnumerator
            .Enumerate(keys, WarmFrom(warm), materialise, predicate: null)
            .ToArray();

        // Regardless of which tier served each item, the output stays in input order.
        CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));
    }

    [Test]
    public void Enumerate_OmitsKeysThatResolveToNothing()
    {
        var (keys, items) = BuildItems(10);

        // Only the first five keys exist in the backing store; the rest resolve to nothing.
        var backing = items.Where(kvp => kvp.Value.Id < 5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var (materialise, _) = Materialiser(backing);

        IPublishedContent[] result = ChunkedPublishedContentEnumerator
            .Enumerate(keys, AllMiss(), materialise, predicate: null)
            .ToArray();

        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, result.Select(x => x.Id));
    }

    [Test]
    public void Enumerate_EmptyInput_ReturnsEmpty()
    {
        var (materialise, calls) = Materialiser(new Dictionary<Guid, IPublishedContent>());

        IPublishedContent[] result = ChunkedPublishedContentEnumerator
            .Enumerate([], AllMiss(), materialise, predicate: null)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.IsEmpty(result);
            Assert.IsEmpty(calls);
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

    // A sync L0 probe backed by the given "warm" set.
    private static TryGetCachedDelegate WarmFrom(Dictionary<Guid, IPublishedContent> warm)
        => (Guid key, out IPublishedContent? content) =>
        {
            if (warm.TryGetValue(key, out IPublishedContent? cached))
            {
                content = cached;
                return true;
            }

            content = null;
            return false;
        };

    private static TryGetCachedDelegate AllMiss()
        => (Guid _, out IPublishedContent? content) =>
        {
            content = null;
            return false;
        };

    // A batched materialiser backed by the given store, recording the keys of each invocation so
    // tests can assert how much was materialised.
    private static (MaterialiseMissesDelegate Materialise, List<IReadOnlyList<Guid>> Calls) Materialiser(
        Dictionary<Guid, IPublishedContent> store)
    {
        var calls = new List<IReadOnlyList<Guid>>();
        MaterialiseMissesDelegate materialise = missKeys =>
        {
            calls.Add(missKeys.ToArray());
            return missKeys
                .Select(k => store.TryGetValue(k, out IPublishedContent? item) ? item : null)
                .Where(item => item is not null)
                .Select(item => item!)
                .ToArray();
        };

        return (materialise, calls);
    }
}
