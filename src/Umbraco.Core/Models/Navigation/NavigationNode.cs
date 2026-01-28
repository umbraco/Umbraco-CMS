using System.Collections.Concurrent;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Core.Models.Navigation;

/// <summary>
///     Represents a node in the content navigation structure.
/// </summary>
public sealed class NavigationNode
{
    private ConcurrentHashSet<Guid> _children;

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
    }
}
