namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Provides static methods and nested types for creating topologically sortable graph nodes.
/// </summary>
public class TopoGraph
{
    /// <summary>
    ///     Error message used when a cyclic dependency is detected.
    /// </summary>
    internal const string CycleDependencyError = "Cyclic dependency.";

    /// <summary>
    ///     Error message used when a required dependency is missing.
    /// </summary>
    internal const string MissingDependencyError = "Missing dependency.";

    /// <summary>
    ///     Creates a new graph node with the specified key, item, and dependencies.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key that uniquely identifies the node.</param>
    /// <param name="item">The item stored in the node.</param>
    /// <param name="dependencies">The keys of nodes that this node depends on.</param>
    /// <returns>A new <see cref="Node{TKey, TItem}" /> instance.</returns>
    public static Node<TKey, TItem> CreateNode<TKey, TItem>(TKey key, TItem item, IEnumerable<TKey> dependencies) =>
        new(key, item, dependencies);

    /// <summary>
    ///     Represents a node in a topological graph with a key, item, and dependencies.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class Node<TKey, TItem>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Node{TKey, TItem}" /> class.
        /// </summary>
        /// <param name="key">The key that uniquely identifies the node.</param>
        /// <param name="item">The item stored in the node.</param>
        /// <param name="dependencies">The keys of nodes that this node depends on.</param>
        public Node(TKey key, TItem item, IEnumerable<TKey> dependencies)
        {
            Key = key;
            Item = item;
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Gets the key that uniquely identifies this node.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        ///     Gets the item stored in this node.
        /// </summary>
        public TItem Item { get; }

        /// <summary>
        ///     Gets the keys of nodes that this node depends on.
        /// </summary>
        public IEnumerable<TKey> Dependencies { get; }
    }
}

/// <summary>
///     Represents a generic DAG that can be topologically sorted.
/// </summary>
/// <typeparam name="TKey">The type of the keys.</typeparam>
/// <typeparam name="TItem">The type of the items.</typeparam>
public class TopoGraph<TKey, TItem> : TopoGraph
    where TKey : notnull
{
    private readonly Func<TItem, IEnumerable<TKey>?> _getDependencies;
    private readonly Func<TItem, TKey> _getKey;
    private readonly Dictionary<TKey, TItem> _items = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TopoGraph{TKey, TItem}" /> class.
    /// </summary>
    /// <param name="getKey">A method that returns the key of an item.</param>
    /// <param name="getDependencies">A method that returns the dependency keys of an item.</param>
    public TopoGraph(Func<TItem, TKey> getKey, Func<TItem, IEnumerable<TKey>?> getDependencies)
    {
        _getKey = getKey;
        _getDependencies = getDependencies;
    }

    /// <summary>
    ///     Adds an item to the graph.
    /// </summary>
    /// <param name="item">The item.</param>
    public void AddItem(TItem item)
    {
        TKey key = _getKey(item);
        _items[key] = item;
    }

    /// <summary>
    ///     Adds items to the graph.
    /// </summary>
    /// <param name="items">The items.</param>
    public void AddItems(IEnumerable<TItem> items)
    {
        foreach (TItem item in items)
        {
            AddItem(item);
        }
    }

    /// <summary>
    ///     Gets the sorted items.
    /// </summary>
    /// <param name="throwOnCycle">A value indicating whether to throw on cycles, or just ignore the branch.</param>
    /// <param name="throwOnMissing">A value indicating whether to throw on missing dependency, or just ignore the dependency.</param>
    /// <param name="reverse">A value indicating whether to reverse the order.</param>
    /// <returns>The (topologically) sorted items.</returns>
    public IEnumerable<TItem> GetSortedItems(bool throwOnCycle = true, bool throwOnMissing = true, bool reverse = false)
    {
        var sorted = new TItem[_items.Count];
        var visited = new HashSet<TItem>();
        var index = reverse ? _items.Count - 1 : 0;
        var incr = reverse ? -1 : +1;

        foreach (TItem item in _items.Values)
        {
            Visit(item, visited, sorted, ref index, incr, throwOnCycle, throwOnMissing);
        }

        return sorted;
    }

    /// <summary>
    ///     Determines whether the specified item exists in the array within the given range.
    /// </summary>
    /// <param name="items">The array to search.</param>
    /// <param name="item">The item to locate.</param>
    /// <param name="start">The starting index of the search range.</param>
    /// <param name="count">The number of elements to search.</param>
    /// <returns><c>true</c> if the item is found; otherwise, <c>false</c>.</returns>
    private static bool Contains(TItem[] items, TItem item, int start, int count) =>
        Array.IndexOf(items, item, start, count) >= 0;

    /// <summary>
    ///     Recursively visits a node and its dependencies to perform topological sorting.
    /// </summary>
    /// <param name="item">The item to visit.</param>
    /// <param name="visited">The set of already visited items.</param>
    /// <param name="sorted">The array to store sorted items.</param>
    /// <param name="index">The current index in the sorted array.</param>
    /// <param name="incr">The increment direction (+1 for forward, -1 for reverse).</param>
    /// <param name="throwOnCycle">Whether to throw an exception on cyclic dependencies.</param>
    /// <param name="throwOnMissing">Whether to throw an exception on missing dependencies.</param>
    private void Visit(TItem item, ISet<TItem> visited, TItem[] sorted, ref int index, int incr, bool throwOnCycle, bool throwOnMissing)
    {
        if (visited.Contains(item))
        {
            // visited but not sorted yet = cycle
            var start = incr > 0 ? 0 : index;
            var count = incr > 0 ? index : sorted.Length - index;
            if (throwOnCycle && Contains(sorted, item, start, count) == false)
            {
                throw new Exception(CycleDependencyError + ": " + item);
            }

            return;
        }

        visited.Add(item);

        IEnumerable<TKey>? keys = _getDependencies(item);
        IEnumerable<TItem>? dependencies = keys == null ? null : FindDependencies(keys, throwOnMissing);

        if (dependencies != null)
        {
            foreach (TItem dep in dependencies)
            {
                Visit(dep, visited, sorted, ref index, incr, throwOnCycle, throwOnMissing);
            }
        }

        sorted[index] = item;
        index += incr;
    }

    /// <summary>
    ///     Finds and returns the items corresponding to the specified dependency keys.
    /// </summary>
    /// <param name="keys">The keys of the dependencies to find.</param>
    /// <param name="throwOnMissing">Whether to throw an exception if a dependency is not found.</param>
    /// <returns>An enumerable of items corresponding to the keys.</returns>
    private IEnumerable<TItem> FindDependencies(IEnumerable<TKey> keys, bool throwOnMissing)
    {
        foreach (TKey key in keys)
        {
            if (_items.TryGetValue(key, out TItem? value))
            {
                yield return value;
            }
            else if (throwOnMissing)
            {
                throw new Exception($"{MissingDependencyError} Error in type {typeof(TItem).Name}, with key {key}");
            }
        }
    }
}
