using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

[TestFixture]
public class ChunkedTieredResolverTests
{
    // Held as static readonly fields (rather than inline array literals) so the assertion calls do not
    // allocate a fresh array each time.
    private static readonly int[] _evenIds = [0, 2, 4, 6, 8];
    private static readonly int[] _firstFiveIds = [0, 1, 2, 3, 4];
    private static readonly int[] _duplicateKeyResultIds = [1, 0, 0];

    [Test]
    public void Resolve_AllCached_ReturnsAllInOrder_WithoutMaterialising()
    {
        var (keys, items) = BuildItems(10);
        ResolveItemsDelegate<Guid, IPublishedContent> warmTier = WarmFrom(items); // everything in L0
        var (materialiseTier, calls) = Materialiser(items);

        IPublishedContent[] result = ChunkedTieredResolver
            .Resolve(keys, warmTier, materialiseTier)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(10, result.Length);
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));

            // All served from the sync L0 tier — the batched materialiser is never invoked.
            Assert.IsEmpty(calls);
        });
    }

    [Test]
    public void Resolve_NoneCached_MaterialisesAllInOrder_InAFewBatches()
    {
        var (keys, items) = BuildItems(10);
        var (materialiseTier, calls) = Materialiser(items);

        IPublishedContent[] result = ChunkedTieredResolver
            .Resolve(keys, AllMiss(), materialiseTier)
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
    public void Resolve_FirstOnly_MaterialisesSingleItem()
    {
        var (keys, items) = BuildItems(10);
        var (materialiseTier, calls) = Materialiser(items);

        IPublishedContent? first = ChunkedTieredResolver
            .Resolve(keys, AllMiss(), materialiseTier)
            .FirstOrDefault();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(first);
            Assert.AreEqual(0, first!.Id);
            Assert.AreEqual(1, calls.SelectMany(c => c).Count());
        });
    }

    [Test]
    public void Resolve_Take_MaterialisesOnlyRequested()
    {
        var (keys, items) = BuildItems(10);
        var (materialiseTier, calls) = Materialiser(items);

        IPublishedContent[] taken = ChunkedTieredResolver
            .Resolve(keys, AllMiss(), materialiseTier)
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
    public void Resolve_FilteringIsLeftToTheCaller()
    {
        var (keys, items) = BuildItems(10);
        var (materialiseTier, _) = Materialiser(items);

        // Resolve itself applies no filtering — callers chain .Where() on the (lazy) result.
        IPublishedContent[] result = ChunkedTieredResolver
            .Resolve(keys, WarmFrom(items), materialiseTier)
            .Where(item => item.Id % 2 == 0)
            .ToArray();

        CollectionAssert.AreEqual(_evenIds, result.Select(x => x.Id));
    }

    [Test]
    public void Resolve_MixedCacheHits_PreservesInputOrder()
    {
        var (keys, items) = BuildItems(10);

        // Even-index items are warm in L0; odd-index items must be batch-materialised.
        var warm = items.Where(kvp => kvp.Value.Id % 2 == 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var (materialiseTier, _) = Materialiser(items);

        IPublishedContent[] result = ChunkedTieredResolver
            .Resolve(keys, WarmFrom(warm), materialiseTier)
            .ToArray();

        // Regardless of which tier served each item, the output stays in input order.
        CollectionAssert.AreEqual(Enumerable.Range(0, 10), result.Select(x => x.Id));
    }

    [Test]
    public void Resolve_OmitsKeysThatResolveToNothing()
    {
        var (keys, items) = BuildItems(10);

        // Only the first five keys exist in the backing store; the rest resolve to nothing.
        var backing = items.Where(kvp => kvp.Value.Id < 5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var (materialiseTier, _) = Materialiser(backing);

        IPublishedContent[] result = ChunkedTieredResolver
            .Resolve(keys, AllMiss(), materialiseTier)
            .ToArray();

        CollectionAssert.AreEqual(_firstFiveIds, result.Select(x => x.Id));
    }

    [Test]
    public void Resolve_DuplicateKeyWithinAChunk_ResolvesToSameItemAtEveryOccurrence_WithoutDuplicateWork()
    {
        var (keys, items) = BuildItems(2);
        Guid keyA = keys[0];
        Guid keyB = keys[1];

        // Chunk sizes grow 1, 2, 4, ... — so chunk 1 is [keyB] and chunk 2 is [keyA, keyA], the
        // duplicate landing entirely within one chunk rather than split across the boundary.
        Guid[] input = [keyB, keyA, keyA];
        var (materialiseTier, calls) = Materialiser(items);

        IPublishedContent[] result = ChunkedTieredResolver
            .Resolve(input, AllMiss(), materialiseTier)
            .ToArray();

        Assert.Multiple(() =>
        {
            // The repeated key resolves to the same item at both occurrences.
            CollectionAssert.AreEqual(_duplicateKeyResultIds, result.Select(x => x.Id));

            // ...but the second chunk's tier call only asks about keyA once, not twice.
            Assert.AreEqual(2, calls.Count);
            CollectionAssert.AreEqual(new[] { keyA }, calls[1]);
        });
    }

    [Test]
    public void Resolve_EmptyInput_ReturnsEmpty()
    {
        var (materialiseTier, calls) = Materialiser(new Dictionary<Guid, IPublishedContent>());

        IPublishedContent[] result = ChunkedTieredResolver
            .Resolve([], AllMiss(), materialiseTier)
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

    // A sync L0 tier backed by the given "warm" set.
    private static ResolveItemsDelegate<Guid, IPublishedContent> WarmFrom(Dictionary<Guid, IPublishedContent> warm)
        => (keys, results) =>
        {
            foreach (Guid key in keys)
            {
                if (warm.TryGetValue(key, out IPublishedContent? cached))
                {
                    results[key] = cached;
                }
            }
        };

    private static ResolveItemsDelegate<Guid, IPublishedContent> AllMiss()
        => (_, _) => { };

    // A batched materialising tier backed by the given store, recording the keys of each invocation so
    // tests can assert how much was materialised.
    private static (ResolveItemsDelegate<Guid, IPublishedContent> Materialise, List<IReadOnlyList<Guid>> Calls) Materialiser(
        Dictionary<Guid, IPublishedContent> store)
    {
        var calls = new List<IReadOnlyList<Guid>>();
        ResolveItemsDelegate<Guid, IPublishedContent> materialise = (keys, results) =>
        {
            calls.Add(keys.ToArray());

            foreach (Guid key in keys)
            {
                if (store.TryGetValue(key, out IPublishedContent? item))
                {
                    results[key] = item;
                }
            }
        };

        return (materialise, calls);
    }
}
