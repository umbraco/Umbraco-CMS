using System.Collections.Concurrent;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Core.Models.Navigation;

/// <summary>
///     Represents a node in the content navigation structure.
/// </summary>
public sealed class NavigationNode
{
    private static readonly Comparison<(Guid Key, int SortOrder)> _sortBySortOrder =
        static (a, b) => a.SortOrder.CompareTo(b.SortOrder);

    private readonly ConcurrentHashSet<Guid> _children;

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
    {
        if (navigationStructure.TryGetValue(childKey, out NavigationNode? child) is false)
        {
            throw new KeyNotFoundException($"Item with key '{childKey}' was not found in the navigation structure.");
        }

        child.Parent = Key;

        // Add it as the last item
        child.SortOrder = _children.Count;

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

    /// <summary>
    ///     Invalidates the cached ordered-children snapshot.
    /// </summary>
    /// <remarks>
    ///     Called by <see cref="AddChild"/> and <see cref="RemoveChild"/> automatically. Must be
    ///     called externally when a child's <c>SortOrder</c> changes (the parent's cache sorts by
    ///     child <c>SortOrder</c> and so is stale after such an update).
    /// </remarks>
    internal void InvalidateOrderedChildren()
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
}
