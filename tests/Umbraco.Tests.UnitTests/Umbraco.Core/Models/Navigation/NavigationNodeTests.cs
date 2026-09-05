using System.Collections.Concurrent;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Navigation;

/// <summary>
/// Tests for <see cref="NavigationNode"/>'s ordered-children cache. Functional behaviour
/// across the full navigation service is covered by the integration suite; this fixture
/// focuses on the lazy-built, mutation-invalidated cache contract.
/// </summary>
[TestFixture]
public class NavigationNodeTests
{
    [Test]
    public void GetOrderedChildren_NoChildren_ReturnsEmpty()
    {
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        NavigationNode node = AddNode(structure, contentTypeKey: Guid.NewGuid());

        IReadOnlyList<Guid> children = node.GetOrderedChildren(structure);

        Assert.That(children, Is.Empty);
    }

    [Test]
    public void GetOrderedChildren_ReturnsChildrenSortedBySortOrder()
    {
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);

        // Three children added via AddChild — each gets sort order 0, 1, 2 in turn.
        Guid first = AddChildOf(structure, parent, contentTypeKey);
        Guid second = AddChildOf(structure, parent, contentTypeKey);
        Guid third = AddChildOf(structure, parent, contentTypeKey);

        IReadOnlyList<Guid> ordered = parent.GetOrderedChildren(structure);

        Assert.That(ordered, Is.EqualTo(new[] { first, second, third }));
    }

    [Test]
    public void GetOrderedChildren_RepeatedCalls_ReturnSameInstanceWhenUnchanged()
    {
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);
        AddChildOf(structure, parent, contentTypeKey);
        AddChildOf(structure, parent, contentTypeKey);

        IReadOnlyList<Guid> first = parent.GetOrderedChildren(structure);
        IReadOnlyList<Guid> second = parent.GetOrderedChildren(structure);

        // The cache contract: until invalidated, repeated calls return the same array.
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void GetOrderedChildren_AddChild_InvalidatesCacheAndIncludesNewChild()
    {
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);
        AddChildOf(structure, parent, contentTypeKey);
        AddChildOf(structure, parent, contentTypeKey);

        IReadOnlyList<Guid> beforeAdd = parent.GetOrderedChildren(structure);
        Assume.That(beforeAdd.Count, Is.EqualTo(2));

        Guid added = AddChildOf(structure, parent, contentTypeKey);

        IReadOnlyList<Guid> afterAdd = parent.GetOrderedChildren(structure);

        Assert.That(afterAdd, Is.Not.SameAs(beforeAdd));
        Assert.That(afterAdd, Has.Count.EqualTo(3));
        Assert.That(afterAdd, Does.Contain(added));
    }

    [Test]
    public void GetOrderedChildren_RemoveChild_InvalidatesCacheAndExcludesChild()
    {
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);
        Guid childA = AddChildOf(structure, parent, contentTypeKey);
        Guid childB = AddChildOf(structure, parent, contentTypeKey);

        // Prime the cache.
        IReadOnlyList<Guid> primed = parent.GetOrderedChildren(structure);
        Assume.That(primed, Is.EqualTo(new[] { childA, childB }));

        parent.RemoveChild(structure, childA);

        IReadOnlyList<Guid> afterRemove = parent.GetOrderedChildren(structure);

        Assert.That(afterRemove, Is.Not.SameAs(primed));
        Assert.That(afterRemove, Is.EqualTo(new[] { childB }));
    }

    [Test]
    public void GetOrderedChildren_AfterInvalidate_ReturnsFreshSnapshotReflectingSortOrderChange()
    {
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);
        Guid childA = AddChildOf(structure, parent, contentTypeKey);
        Guid childB = AddChildOf(structure, parent, contentTypeKey);
        Guid childC = AddChildOf(structure, parent, contentTypeKey);

        IReadOnlyList<Guid> initial = parent.GetOrderedChildren(structure);
        Assume.That(initial, Is.EqualTo(new[] { childA, childB, childC }));

        // Reverse the sort order via UpdateSortOrder, then invalidate (this mirrors what
        // ContentNavigationServiceBase.UpdateSortOrder does after mutating a child).
        structure[childA].UpdateSortOrder(2);
        structure[childB].UpdateSortOrder(1);
        structure[childC].UpdateSortOrder(0);
        parent.InvalidateOrderedChildren();

        IReadOnlyList<Guid> reordered = parent.GetOrderedChildren(structure);

        Assert.That(reordered, Is.Not.SameAs(initial));
        Assert.That(reordered, Is.EqualTo(new[] { childC, childB, childA }));
    }

    [Test]
    public void GetOrderedChildren_WithoutInvalidation_StillReturnsOldOrderingAfterDirectSortOrderEdit()
    {
        // Documents the contract: NavigationNode.UpdateSortOrder on a child does NOT
        // automatically invalidate the parent's cache (the node has no parent reference).
        // Callers that mutate child SortOrder must call InvalidateOrderedChildren on the parent
        // — which is what ContentNavigationServiceBase.UpdateSortOrder does.
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);
        Guid childA = AddChildOf(structure, parent, contentTypeKey);
        Guid childB = AddChildOf(structure, parent, contentTypeKey);

        IReadOnlyList<Guid> primed = parent.GetOrderedChildren(structure);
        Assume.That(primed, Is.EqualTo(new[] { childA, childB }));

        // Flip the sort orders without invalidating.
        structure[childA].UpdateSortOrder(1);
        structure[childB].UpdateSortOrder(0);

        IReadOnlyList<Guid> stillCached = parent.GetOrderedChildren(structure);

        Assert.That(stillCached, Is.SameAs(primed), "Cache returns the previously-built array until invalidated");
    }

    [Test]
    public void GetOrderedChildren_StaleAfterDirectSortOrderEdit_SelfHealsOnNextAddChild()
    {
        // A third-party caller that mutates SortOrder directly on a NavigationNode (rather than
        // going through ContentNavigationServiceBase.UpdateSortOrder) leaves the parent's ordered-
        // children cache stale, but any subsequent structural mutation that flows through
        // AddChild/RemoveChild on that parent will discard the stale array and rebuild against
        // the current SortOrder values.
        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);
        Guid childA = AddChildOf(structure, parent, contentTypeKey);
        Guid childB = AddChildOf(structure, parent, contentTypeKey);

        // Prime, then mutate sort order directly (the misuse pattern).
        IReadOnlyList<Guid> primed = parent.GetOrderedChildren(structure);
        Assume.That(primed, Is.EqualTo(new[] { childA, childB }));
        structure[childA].UpdateSortOrder(5);
        structure[childB].UpdateSortOrder(1);

        IReadOnlyList<Guid> stillStale = parent.GetOrderedChildren(structure);
        Assume.That(stillStale, Is.SameAs(primed), "Cache must still be stale before the recovering mutation.");

        // Now an unrelated structural change runs through the parent — AddChild on a new sibling.
        // Its sort order (3) sits between the re-edited values, so a freshly-built ordering must
        // place it between the two existing children. If the rebuild were to honour the stale
        // cache, the new child would land at the end instead.
        var newSibling = new NavigationNode(Guid.NewGuid(), contentTypeKey, sortOrder: 3);
        structure[newSibling.Key] = newSibling;
        parent.AddChild(structure, newSibling.Key);

        IReadOnlyList<Guid> recovered = parent.GetOrderedChildren(structure);

        Assert.Multiple(() =>
        {
            Assert.That(recovered, Is.Not.SameAs(primed), "Cache must rebuild rather than reuse the stale array.");
            Assert.That(
                recovered,
                Is.EqualTo(new[] { childB, newSibling.Key, childA }),
                "Rebuilt ordering must honour the current SortOrder values, including the directly-edited ones.");
        });
    }

    [Test]
    public void GetOrderedChildren_ConcurrentFirstAccess_AllThreadsSeeSameInstance()
    {
        // The race we're guarding against: multiple threads reach BuildOrderedChildren before
        // any has finished. Without the lock + double-check they could each store a different
        // array; the contract requires every reader observes the same canonical instance.
        const int threadCount = 32;

        var structure = new ConcurrentDictionary<Guid, NavigationNode>();
        Guid contentTypeKey = Guid.NewGuid();
        NavigationNode parent = AddNode(structure, contentTypeKey);
        for (var i = 0; i < 50; i++)
        {
            AddChildOf(structure, parent, contentTypeKey);
        }

        var observed = new IReadOnlyList<Guid>[threadCount];
        var startGate = new ManualResetEventSlim(false);
        var threads = new Thread[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            var localT = t;
            threads[t] = new Thread(() =>
            {
                startGate.Wait();
                observed[localT] = parent.GetOrderedChildren(structure);
            });
            threads[t].Start();
        }

        startGate.Set();
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        IReadOnlyList<Guid> reference = observed[0];
        for (var i = 1; i < threadCount; i++)
        {
            Assert.That(observed[i], Is.SameAs(reference));
        }
    }

    private static NavigationNode AddNode(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        Guid contentTypeKey)
    {
        var node = new NavigationNode(Guid.NewGuid(), contentTypeKey);
        structure[node.Key] = node;
        return node;
    }

    private static Guid AddChildOf(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        NavigationNode parent,
        Guid contentTypeKey)
    {
        NavigationNode child = AddNode(structure, contentTypeKey);
        parent.AddChild(structure, child.Key);
        return child.Key;
    }
}
