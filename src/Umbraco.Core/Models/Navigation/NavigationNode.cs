using System.Collections.Concurrent;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Core.Models.Navigation;

/// <summary>
///     Represents a node in the content navigation structure.
/// </summary>
public sealed class NavigationNode
{
    private static readonly Comparison<(Guid Key, int SortOrder)> _sortBySortOrder =
        static (a, b) => CompareBySortOrderThenKey(a.SortOrder, a.Key, b.SortOrder, b.Key);

    private readonly ConcurrentHashSet<Guid> _children;


#pragma warning disable CS0419 // Ambiguous reference in cref attribute
    /// <summary>
    /// Cached snapshot of <see cref="Children"/> ordered by each child's <c>SortOrder</c>.
    /// </summary>
    /// <remarks>
    /// Built lazily by <see cref="GetOrderedChildren"/> on first access and invalidated
    /// (set to <c>null</c>) by <see cref="AddChild"/> / <see cref="RemoveChild"/> /
    /// <see cref="InvalidateOrderedChildren"/>. Reads are lock-free on the fast path; the
    /// build and invalidation paths take <see cref="_orderedChildrenLock"/> so concurrent
    /// first-access threads agree on a single canonical array and an in-flight build
    /// cannot finish after a concurrent invalidation has cleared it.
    /// </remarks>
    private Guid[]? _orderedChildren;
#pragma warning restore CS0419 // Ambiguous reference in cref attribute

    private readonly Lock _orderedChildrenLock = new();

    /// <summary>
    ///     Gets the unique key of this navigation node.
    /// </summary>
    public Guid Key { get; private set; }

    /// <summary>
    ///     Gets the unique key of the content type for this node.
    /// </summary>
    public Guid ContentTypeKey { get; private set; }

    /// <summary>
    ///     Gets the sort order of this node among its siblings.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    ///     Gets the key of the parent node, or null if this is a root node.
    /// </summary>
    public Guid? Parent { get; private set; }

    /// <summary>
    ///     Gets the set of child node keys.
    /// </summary>
    public ISet<Guid> Children => _children;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NavigationNode" /> class.
    /// </summary>
    /// <param name="key">The unique key of the node.</param>
    /// <param name="contentTypeKey">The unique key of the content type.</param>
    /// <param name="sortOrder">The sort order of the node.</param>
    public NavigationNode(Guid key, Guid contentTypeKey, int sortOrder = 0)
    {
        Key = key;
        ContentTypeKey = contentTypeKey;
        SortOrder = sortOrder;
        _children = new ConcurrentHashSet<Guid>();
    }

    /// <summary>
    ///     Updates the sort order of this node.
    /// </summary>
    /// <param name="newSortOrder">The new sort order value.</param>
    /// <remarks>
    ///     The parent node's cached ordered-children list (if any) is now stale because it sorts
    ///     by child <c>SortOrder</c>. Callers that hold a reference to the parent should call
    ///     <see cref="InvalidateOrderedChildren"/> on it; <see cref="NavigationNode"/> does not
    ///     hold a reference to its parent <see cref="NavigationNode"/> so cannot invalidate it
    ///     itself.
    /// </remarks>
    // TODO (V19): Make internal. The contract requires the caller to invalidate the parent's
    // ordered-children cache (InvalidateOrderedChildren is internal, so external callers cannot
    // satisfy that contract and would silently observe stale ordering on subsequent reads).
    // Internal callers in ContentNavigationServiceBase already do the invalidation correctly.
    public void UpdateSortOrder(int newSortOrder) => SortOrder = newSortOrder;

    /// <summary>
    ///     Adds a child node to this node.
    /// </summary>
    /// <param name="navigationStructure">The navigation structure dictionary containing all nodes.</param>
    /// <param name="childKey">The key of the child node to add.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the child key is not found in the navigation structure.</exception>
    public void AddChild(ConcurrentDictionary<Guid, NavigationNode> navigationStructure, Guid childKey)
        => AddChild(navigationStructure, childKey, appendAsLastItem: true);

    /// <summary>
    ///     Adds a child node to this node, optionally preserving the child's existing <see cref="SortOrder"/>.
    /// </summary>
    /// <param name="navigationStructure">The navigation structure dictionary containing all nodes.</param>
    /// <param name="childKey">The key of the child node to add.</param>
    /// <param name="appendAsLastItem">
    ///     When <c>true</c>, the child's <see cref="SortOrder"/> is set so it sorts after its existing
    ///     siblings (the behaviour required when a node is newly created or moved). When <c>false</c>,
    ///     the child's <see cref="SortOrder"/> is left untouched — used when rebuilding the structure
    ///     from persisted data, where each node already carries the sort order loaded from the database
    ///     and the load order (parent-first, by path) must not be allowed to redefine it.
    /// </param>
    /// <exception cref="KeyNotFoundException">Thrown when the child key is not found in the navigation structure.</exception>
    internal void AddChild(ConcurrentDictionary<Guid, NavigationNode> navigationStructure, Guid childKey, bool appendAsLastItem)
    {
        if (navigationStructure.TryGetValue(childKey, out NavigationNode? child) is false)
        {
            throw new KeyNotFoundException($"Item with key '{childKey}' was not found in the navigation structure.");
        }

        child.Parent = Key;

        if (appendAsLastItem)
        {
            child.SortOrder = _children.Count;
        }

        _children.Add(childKey);

        InvalidateOrderedChildren();
    }

    /// <summary>
    ///     Removes a child node from this node.
    /// </summary>
    /// <param name="navigationStructure">The navigation structure dictionary containing all nodes.</param>
    /// <param name="childKey">The key of the child node to remove.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the child key is not found in the navigation structure.</exception>
    public void RemoveChild(ConcurrentDictionary<Guid, NavigationNode> navigationStructure, Guid childKey)
    {
        if (navigationStructure.TryGetValue(childKey, out NavigationNode? child) is false)
        {
            throw new KeyNotFoundException($"Item with key '{childKey}' was not found in the navigation structure.");
        }

        _children.Remove(childKey);
        child.Parent = null;

        InvalidateOrderedChildren();
    }

    /// <summary>
    ///     Returns this node's children ordered by <c>SortOrder</c>.
    /// </summary>
    /// <param name="navigationStructure">The navigation structure dictionary containing all nodes; needed to look up each child's current <c>SortOrder</c>.</param>
    /// <returns>An immutable, sort-order-presorted snapshot of the children. The result is cached and reused across calls until the children set or a child's <c>SortOrder</c> is mutated.</returns>
    /// <remarks>
    ///     Lock-free fast path: a non-null cached array is returned without acquiring the lock.
    ///     If the cache is empty, <see cref="BuildOrderedChildren"/> is called under the lock to
    ///     build (with double-checked re-read) and store the canonical array.
    /// </remarks>
    internal IReadOnlyList<Guid> GetOrderedChildren(ConcurrentDictionary<Guid, NavigationNode> navigationStructure)
    {
        // Volatile.Read provides the acquire fence that pairs with the release fence on the
        // lock-protected stores in BuildOrderedChildren / InvalidateOrderedChildren. On weak
        // memory architectures (e.g. ARM64) a plain read can observe writes out of order with
        // the lock release, so without this barrier a reader could in principle see a torn or
        // unpublished reference; on x86/x64 the TSO model already gives acquire semantics so
        // this compiles to a normal load. Matches the lock-free read idiom in System.Lazy<T>
        // and LazyInitializer.EnsureInitialized.
        Guid[]? cached = Volatile.Read(ref _orderedChildren);
        if (cached is not null)
        {
            return cached;
        }

        return BuildOrderedChildren(navigationStructure);
    }


#pragma warning disable CS0419 // Ambiguous reference in cref attribute
    /// <summary>
    ///     Invalidates the cached ordered-children snapshot.
    /// </summary>
    /// <remarks>
    ///     Called by <see cref="AddChild"/> and <see cref="RemoveChild"/> automatically. Must be
    ///     called externally when a child's <c>SortOrder</c> changes (the parent's cache sorts by
    ///     child <c>SortOrder</c> and so is stale after such an update).
    /// </remarks>
    internal void InvalidateOrderedChildren()
#pragma warning restore CS0419 // Ambiguous reference in cref attribute
    {
        lock (_orderedChildrenLock)
        {
            _orderedChildren = null;
        }
    }

    private Guid[] BuildOrderedChildren(ConcurrentDictionary<Guid, NavigationNode> navigationStructure)
    {
        lock (_orderedChildrenLock)
        {
            // Double-check under the lock — another thread may have built the cache while we
            // were waiting to acquire it.
            Guid[]? cached = _orderedChildren;
            if (cached is not null)
            {
                return cached;
            }

            if (_children.Count == 0)
            {
                _orderedChildren = [];
                return _orderedChildren;
            }

            var sorted = new List<(Guid Key, int SortOrder)>(_children.Count);
            foreach (Guid childKey in _children)
            {
                if (navigationStructure.TryGetValue(childKey, out NavigationNode? childNode))
                {
                    sorted.Add((childKey, childNode.SortOrder));
                }
            }

            sorted.Sort(_sortBySortOrder);

            var result = new Guid[sorted.Count];
            for (var i = 0; i < sorted.Count; i++)
            {
                result[i] = sorted[i].Key;
            }

            _orderedChildren = result;
            return result;
        }
    }

    /// <summary>
    ///     Defines the canonical sibling ordering: by <c>SortOrder</c>, then by key as a tie-break.
    /// </summary>
    /// <param name="sortOrderA">The sort order of the first node.</param>
    /// <param name="keyA">The key of the first node, used as the tie-break when sort orders are equal.</param>
    /// <param name="sortOrderB">The sort order of the second node.</param>
    /// <param name="keyB">The key of the second node, used as the tie-break when sort orders are equal.</param>
    /// <returns>
    ///     A negative value if the first node sorts before the second, a positive value if it sorts
    ///     after, and zero only when both the sort order and key are equal.
    /// </returns>
    /// <remarks>
    ///     The key tie-break keeps ordering deterministic across rebuilds when sibling sort orders
    ///     collide — which well-formed Umbraco data avoids, but corrupt/legacy data can produce.
    ///     The <see cref="List{T}"/> sort used to order children is not a stable sort, so without a
    ///     total order the relative position of tied siblings would be arbitrary and could differ
    ///     from one rebuild to the next. Shared so every sibling-ordering call site stays in sync.
    /// </remarks>
    internal static int CompareBySortOrderThenKey(int sortOrderA, Guid keyA, int sortOrderB, Guid keyB)
    {
        var bySortOrder = sortOrderA.CompareTo(sortOrderB);
        return bySortOrder != 0 ? bySortOrder : keyA.CompareTo(keyB);
    }
}
