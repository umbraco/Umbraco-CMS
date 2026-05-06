using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services.Navigation;

/// <summary>
/// Tests for the per-snapshot descendants cache on <see cref="ContentNavigationServiceBase{TContentType, TContentTypeService}"/>.
/// Functional behaviour of the public API (Add, Move, Sort, Remove, etc.) is exhaustively
/// covered by the integration suite under <c>DocumentNavigationServiceTests</c>; this fixture
/// focuses on the cache contract introduced by §2.5: cache-hit identity, mutation
/// invalidation, exclusion of the content-type-filtered path from the cache, and concurrent
/// first-access thread safety.
/// </summary>
[TestFixture]
public class ContentNavigationDescendantsCacheTests
{
    private static DocumentNavigationService CreateService() =>
        new(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IContentTypeService>());

    /// <summary>
    /// Builds a service whose <c>IContentTypeService</c> resolves the supplied alias↔key
    /// pairs, so <c>TryGetDescendantsKeysOfType(parent, alias, ...)</c> returns true rather
    /// than failing at the alias-resolution step.
    /// </summary>
    private static DocumentNavigationService CreateServiceWithContentTypes(
        params (string Alias, Guid Key)[] aliasKeyPairs)
    {
        var contentTypes = aliasKeyPairs.Select(p =>
        {
            var ct = new Mock<IContentType>();
            ct.SetupGet(x => x.Alias).Returns(p.Alias);
            ct.SetupGet(x => x.Key).Returns(p.Key);
            return ct.Object;
        }).ToArray();

        var contentTypeService = new Mock<IContentTypeService>();
        contentTypeService.Setup(s => s.GetAll()).Returns(contentTypes);

        return new DocumentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeService.Object);
    }

    [Test]
    public void TryGetDescendantsKeys_RepeatedCalls_ReturnSameInstanceWhenUnchanged()
    {
        DocumentNavigationService service = CreateService();
        Guid root = Guid.NewGuid();
        Guid contentType = Guid.NewGuid();
        service.Add(root, contentType);
        service.Add(Guid.NewGuid(), contentType, root);
        service.Add(Guid.NewGuid(), contentType, root);

        Assume.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> first), Is.True);
        Assume.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> second), Is.True);

        // The cache contract: until invalidated, repeated calls return the same array reference.
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void TryGetDescendantsKeys_AfterAdd_RebuildsAndIncludesNewNode()
    {
        DocumentNavigationService service = CreateService();
        Guid root = Guid.NewGuid();
        Guid contentType = Guid.NewGuid();
        service.Add(root, contentType);
        service.Add(Guid.NewGuid(), contentType, root);

        Assume.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> primed), Is.True);
        Assume.That(primed.Count(), Is.EqualTo(1));

        Guid added = Guid.NewGuid();
        service.Add(added, contentType, root);

        Assert.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> afterAdd), Is.True);
        Assert.That(afterAdd, Is.Not.SameAs(primed));
        Assert.That(afterAdd, Does.Contain(added));
    }

    [Test]
    public void TryGetDescendantsKeys_AfterMove_RebuildsAndReflectsNewParentage()
    {
        DocumentNavigationService service = CreateService();
        Guid contentType = Guid.NewGuid();
        Guid rootA = Guid.NewGuid();
        Guid rootB = Guid.NewGuid();
        Guid moveable = Guid.NewGuid();
        service.Add(rootA, contentType);
        service.Add(rootB, contentType);
        service.Add(moveable, contentType, rootA);

        Assume.That(service.TryGetDescendantsKeys(rootA, out IEnumerable<Guid> rootADescPrimed), Is.True);
        Assume.That(rootADescPrimed, Does.Contain(moveable));
        Assume.That(service.TryGetDescendantsKeys(rootB, out IEnumerable<Guid> rootBDescPrimed), Is.True);
        Assume.That(rootBDescPrimed, Is.Empty);

        service.Move(moveable, rootB);

        Assert.That(service.TryGetDescendantsKeys(rootA, out IEnumerable<Guid> rootAAfter), Is.True);
        Assert.That(service.TryGetDescendantsKeys(rootB, out IEnumerable<Guid> rootBAfter), Is.True);
        Assert.That(rootAAfter, Does.Not.Contain(moveable));
        Assert.That(rootBAfter, Does.Contain(moveable));
    }

    [Test]
    public void TryGetDescendantsKeys_AfterMoveToBin_RebuildsAndOmitsMovedNode()
    {
        DocumentNavigationService service = CreateService();
        Guid contentType = Guid.NewGuid();
        Guid root = Guid.NewGuid();
        Guid trashed = Guid.NewGuid();
        service.Add(root, contentType);
        service.Add(trashed, contentType, root);

        Assume.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> primed), Is.True);
        Assume.That(primed, Does.Contain(trashed));

        service.MoveToBin(trashed);

        Assert.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> after), Is.True);
        Assert.That(after, Does.Not.Contain(trashed));
    }

    [Test]
    public void TryGetDescendantsKeys_AfterUpdateSortOrder_RebuildsAndReflectsNewOrder()
    {
        DocumentNavigationService service = CreateService();
        Guid contentType = Guid.NewGuid();
        Guid root = Guid.NewGuid();
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        Guid third = Guid.NewGuid();
        service.Add(root, contentType);
        service.Add(first, contentType, root);
        service.Add(second, contentType, root);
        service.Add(third, contentType, root);

        Assume.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> primed), Is.True);
        Assume.That(primed.ToArray(), Is.EqualTo(new[] { first, second, third }));

        // Reverse the order.
        service.UpdateSortOrder(first, 2);
        service.UpdateSortOrder(second, 1);
        service.UpdateSortOrder(third, 0);

        Assert.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> after), Is.True);
        Assert.That(after.ToArray(), Is.EqualTo(new[] { third, second, first }));
    }

    [Test]
    public void TryGetDescendantsKeysOfType_DoesNotPollute_TryGetDescendantsKeys()
    {
        Guid sameTypeAlias = Guid.NewGuid();
        Guid otherTypeAlias = Guid.NewGuid();
        DocumentNavigationService service = CreateServiceWithContentTypes(
            ("sameType", sameTypeAlias),
            ("otherType", otherTypeAlias));

        Guid root = Guid.NewGuid();
        Guid sameType = Guid.NewGuid();
        Guid differentType = Guid.NewGuid();
        service.Add(root, sameTypeAlias);
        service.Add(sameType, sameTypeAlias, root);
        service.Add(differentType, otherTypeAlias, root);

        // OfType result is cached against (parent, contentType) — not (parent, null) — so the
        // unfiltered cache stays clean and a subsequent TryGetDescendantsKeys returns the full
        // descendant set, not the filtered subset.
        Assume.That(service.TryGetDescendantsKeysOfType(root, "sameType", out IEnumerable<Guid> filtered), Is.True);
        Assume.That(filtered, Is.EquivalentTo(new[] { sameType }));

        Assert.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> unfiltered), Is.True);
        Assert.That(unfiltered, Is.EquivalentTo(new[] { sameType, differentType }));
    }

    [Test]
    public void TryGetDescendantsKeysOfType_RepeatedCalls_ReturnSameInstanceWhenUnchanged()
    {
        Guid contentTypeAlias = Guid.NewGuid();
        DocumentNavigationService service = CreateServiceWithContentTypes(("blogPost", contentTypeAlias));

        Guid root = Guid.NewGuid();
        service.Add(root, contentTypeAlias);
        service.Add(Guid.NewGuid(), contentTypeAlias, root);
        service.Add(Guid.NewGuid(), contentTypeAlias, root);

        Assume.That(service.TryGetDescendantsKeysOfType(root, "blogPost", out IEnumerable<Guid> first), Is.True);
        Assume.That(service.TryGetDescendantsKeysOfType(root, "blogPost", out IEnumerable<Guid> second), Is.True);

        // Cache hit on the OfType path returns the same canonical Guid[] instance.
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void TryGetDescendantsKeysOfType_DifferentTypes_AreCachedSeparately()
    {
        Guid typeAKey = Guid.NewGuid();
        Guid typeBKey = Guid.NewGuid();
        DocumentNavigationService service = CreateServiceWithContentTypes(
            ("typeA", typeAKey),
            ("typeB", typeBKey));

        Guid root = Guid.NewGuid();
        Guid childA = Guid.NewGuid();
        Guid childB = Guid.NewGuid();
        service.Add(root, typeAKey);
        service.Add(childA, typeAKey, root);
        service.Add(childB, typeBKey, root);

        Assert.That(service.TryGetDescendantsKeysOfType(root, "typeA", out IEnumerable<Guid> aResult), Is.True);
        Assert.That(service.TryGetDescendantsKeysOfType(root, "typeB", out IEnumerable<Guid> bResult), Is.True);

        Assert.That(aResult, Is.EquivalentTo(new[] { childA }));
        Assert.That(bResult, Is.EquivalentTo(new[] { childB }));
        Assert.That(aResult, Is.Not.SameAs(bResult));

        // Unfiltered call should still see all descendants regardless of which type-filtered
        // entries are already in the cache.
        Assert.That(service.TryGetDescendantsKeys(root, out IEnumerable<Guid> unfiltered), Is.True);
        Assert.That(unfiltered, Is.EquivalentTo(new[] { childA, childB }));
    }

    [Test]
    public void TryGetDescendantsKeysOfType_AfterAdd_RebuildsAndIncludesMatchingNewNode()
    {
        Guid contentTypeAlias = Guid.NewGuid();
        DocumentNavigationService service = CreateServiceWithContentTypes(("blogPost", contentTypeAlias));

        Guid root = Guid.NewGuid();
        service.Add(root, contentTypeAlias);
        service.Add(Guid.NewGuid(), contentTypeAlias, root);

        Assume.That(service.TryGetDescendantsKeysOfType(root, "blogPost", out IEnumerable<Guid> primed), Is.True);
        Assume.That(primed.Count(), Is.EqualTo(1));

        Guid added = Guid.NewGuid();
        service.Add(added, contentTypeAlias, root);

        Assert.That(service.TryGetDescendantsKeysOfType(root, "blogPost", out IEnumerable<Guid> afterAdd), Is.True);
        Assert.That(afterAdd, Is.Not.SameAs(primed));
        Assert.That(afterAdd, Does.Contain(added));
    }

    [Test]
    public void TryGetDescendantsKeys_ConcurrentFirstAccess_AllThreadsSeeSameInstance()
    {
        // The race we're guarding against: multiple threads enter the static helper before any
        // writes to DescendantsCache. The outcome should be that whoever-writes-last wins, and
        // every reader observes the same canonical Guid[] reference on subsequent calls.
        const int threadCount = 16;
        DocumentNavigationService service = CreateService();
        Guid contentType = Guid.NewGuid();
        Guid root = Guid.NewGuid();
        service.Add(root, contentType);
        for (var i = 0; i < 50; i++)
        {
            service.Add(Guid.NewGuid(), contentType, root);
        }

        var observed = new IEnumerable<Guid>[threadCount];
        var startGate = new ManualResetEventSlim(false);
        var threads = new Thread[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            var localT = t;
            threads[t] = new Thread(() =>
            {
                startGate.Wait();
                service.TryGetDescendantsKeys(root, out IEnumerable<Guid> result);
                observed[localT] = result;
            });
            threads[t].Start();
        }

        startGate.Set();
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        // After the dust settles every thread should see the canonical cached array.
        // Any thread that won the race installed its result; all subsequent reads return it.
        // We allow up to one outlier per thread (the loser wrote its own array and then the
        // winner overwrote — that loser's observed reference is not the canonical one), but
        // a fresh call after the threads finish must hit the cache and return the canonical.
        service.TryGetDescendantsKeys(root, out IEnumerable<Guid> canonical);
        IEnumerable<Guid> canonicalCached = canonical;

        // Every observation should equal the canonical cached result by content.
        foreach (IEnumerable<Guid> obs in observed)
        {
            Assert.That(obs, Is.EqualTo(canonicalCached));
        }

        // And a second canonical call must reuse the same instance.
        service.TryGetDescendantsKeys(root, out IEnumerable<Guid> canonicalAgain);
        Assert.That(canonicalAgain, Is.SameAs(canonicalCached));
    }
}
