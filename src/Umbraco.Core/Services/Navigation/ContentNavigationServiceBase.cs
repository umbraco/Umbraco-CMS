using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

internal abstract class ContentNavigationServiceBase
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly INavigationRepository _navigationRepository;
    private ConcurrentDictionary<Guid, NavigationNode> _navigationStructure = new();
    private ConcurrentDictionary<Guid, NavigationNode> _recycleBinNavigationStructure = new();

    protected ContentNavigationServiceBase(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository)
    {
        _coreScopeProvider = coreScopeProvider;
        _navigationRepository = navigationRepository;
    }

    /// <summary>
    ///     Rebuilds the entire main navigation structure. Implementations should define how the structure is rebuilt.
    /// </summary>
    public abstract Task RebuildAsync();

    /// <summary>
    ///     Rebuilds the recycle bin navigation structure. Implementations should define how the bin structure is rebuilt.
    /// </summary>
    public abstract Task RebuildBinAsync();

    public bool TryGetParentKey(Guid childKey, out Guid? parentKey)
        => TryGetParentKeyFromStructure(_navigationStructure, childKey, out parentKey);

    public bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys)
        => TryGetChildrenKeysFromStructure(_navigationStructure, parentKey, out childrenKeys);

    public bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys)
        => TryGetDescendantsKeysFromStructure(_navigationStructure, parentKey, out descendantsKeys);

    public bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys)
        => TryGetAncestorsKeysFromStructure(_navigationStructure, childKey, out ancestorsKeys);

    public bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys)
        => TryGetSiblingsKeysFromStructure(_navigationStructure, key, out siblingsKeys);

    public bool TryGetParentKeyInBin(Guid childKey, out Guid? parentKey)
        => TryGetParentKeyFromStructure(_recycleBinNavigationStructure, childKey, out parentKey);

    public bool TryGetChildrenKeysInBin(Guid parentKey, out IEnumerable<Guid> childrenKeys)
        => TryGetChildrenKeysFromStructure(_recycleBinNavigationStructure, parentKey, out childrenKeys);

    public bool TryGetDescendantsKeysInBin(Guid parentKey, out IEnumerable<Guid> descendantsKeys)
        => TryGetDescendantsKeysFromStructure(_recycleBinNavigationStructure, parentKey, out descendantsKeys);

    public bool TryGetAncestorsKeysInBin(Guid childKey, out IEnumerable<Guid> ancestorsKeys)
        => TryGetAncestorsKeysFromStructure(_recycleBinNavigationStructure, childKey, out ancestorsKeys);

    public bool TryGetSiblingsKeysInBin(Guid key, out IEnumerable<Guid> siblingsKeys)
        => TryGetSiblingsKeysFromStructure(_recycleBinNavigationStructure, key, out siblingsKeys);

    public bool Remove(Guid key)
    {
        if (TryRemoveNodeFromParentInStructure(_navigationStructure, key, out NavigationNode? nodeToRemove) is false || nodeToRemove is null)
        {
            return false; // Node doesn't exist
        }

        // Recursively remove all descendants and add them to recycle bin
        AddDescendantsToRecycleBinRecursively(nodeToRemove);

        return _recycleBinNavigationStructure.TryAdd(nodeToRemove.Key, nodeToRemove) &&
               _navigationStructure.TryRemove(key, out _);
    }

    public bool Add(Guid key, Guid? parentKey = null)
    {
        NavigationNode? parentNode = null;
        if (parentKey.HasValue)
        {
            if (_navigationStructure.TryGetValue(parentKey.Value, out parentNode) is false)
            {
                return false; // Parent node doesn't exist
            }
        }

        var newNode = new NavigationNode(key);
        if (_navigationStructure.TryAdd(key, newNode) is false)
        {
            return false; // Node with this key already exists
        }

        parentNode?.AddChild(newNode);

        return true;
    }

    public bool Move(Guid key, Guid? targetParentKey = null)
    {
        if (_navigationStructure.TryGetValue(key, out NavigationNode? nodeToMove) is false)
        {
            return false; // Node doesn't exist
        }

        if (key == targetParentKey)
        {
            return false; // Cannot move a node to itself
        }

        NavigationNode? targetParentNode = null;
        if (targetParentKey.HasValue && _navigationStructure.TryGetValue(targetParentKey.Value, out targetParentNode) is false)
        {
            return false; // Target parent doesn't exist
        }

        // Remove the node from its current parent's children list
        if (nodeToMove.Parent is not null && _navigationStructure.TryGetValue(nodeToMove.Parent.Key, out var currentParentNode))
        {
            currentParentNode.RemoveChild(nodeToMove);
        }

        // Set the new parent for the node (if parent node is null - the node is moved to root)
        targetParentNode?.AddChild(nodeToMove);

        return true;
    }

    public bool RemoveFromBin(Guid key)
    {
        if (TryRemoveNodeFromParentInStructure(_recycleBinNavigationStructure, key, out NavigationNode? nodeToRemove) is false || nodeToRemove is null)
        {
            return false; // Node doesn't exist
        }

        RemoveDescendantsRecursively(nodeToRemove);

        return _recycleBinNavigationStructure.TryRemove(key, out _);
    }

    public bool RestoreFromBin(Guid key, Guid? targetParentKey = null)
    {
        if (_recycleBinNavigationStructure.TryGetValue(key, out NavigationNode? nodeToRestore) is false)
        {
            return false; // Node doesn't exist
        }

        // If a target parent is specified, try to find it in the main structure
        NavigationNode? targetParentNode = null;
        if (targetParentKey.HasValue && _navigationStructure.TryGetValue(targetParentKey.Value, out targetParentNode) is false)
        {
            return false; // Target parent doesn't exist
        }

        // Set the new parent for the node (if parent node is null - the node is moved to root)
        targetParentNode?.AddChild(nodeToRestore);

        // Restore the node and its descendants from the recycle bin to the main structure
        RestoreNodeAndDescendantsRecursively(nodeToRestore);

        return _navigationStructure.TryAdd(nodeToRestore.Key, nodeToRestore) &&
               _recycleBinNavigationStructure.TryRemove(key, out _);
    }

    /// <summary>
    ///     Rebuilds the navigation structure based on the specified object type key and whether the items are trashed.
    ///     Only relevant for items in the content and media trees (which have readLock values of -333 or -334).
    /// </summary>
    /// <param name="readLock">The read lock value, should be -333 or -334 for content and media trees.</param>
    /// <param name="objectTypeKey">The key of the object type to rebuild.</param>
    /// <param name="trashed">Indicates whether the items are in the recycle bin.</param>
    protected async Task HandleRebuildAsync(int readLock, Guid objectTypeKey, bool trashed)
    {
        // This is only relevant for items in the content and media trees
        if (readLock != Constants.Locks.ContentTree && readLock != Constants.Locks.MediaTree)
        {
            return;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(readLock);

        if (trashed)
        {
            _recycleBinNavigationStructure = _navigationRepository.GetTrashedContentNodesByObjectType(objectTypeKey);
        }
        else
        {
            _navigationStructure = _navigationRepository.GetContentNodesByObjectType(objectTypeKey);
        }
    }

    private bool TryGetParentKeyFromStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid childKey, out Guid? parentKey)
    {
        if (structure.TryGetValue(childKey, out NavigationNode? childNode))
        {
            parentKey = childNode.Parent?.Key;
            return true;
        }

        // Child doesn't exist
        parentKey = null;
        return false;
    }

    private bool TryGetChildrenKeysFromStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid parentKey, out IEnumerable<Guid> childrenKeys)
    {
        if (structure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            childrenKeys = [];
            return false;
        }

        childrenKeys = parentNode.Children.Select(child => child.Key);
        return true;
    }

    private bool TryGetDescendantsKeysFromStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid parentKey, out IEnumerable<Guid> descendantsKeys)
    {
        var descendants = new List<Guid>();

        if (structure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            descendantsKeys = [];
            return false;
        }

        GetDescendantsRecursively(parentNode, descendants);

        descendantsKeys = descendants;
        return true;
    }

    private bool TryGetAncestorsKeysFromStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid childKey, out IEnumerable<Guid> ancestorsKeys)
    {
        var ancestors = new List<Guid>();

        if (structure.TryGetValue(childKey, out NavigationNode? childNode) is false)
        {
            // Child doesn't exist
            ancestorsKeys = [];
            return false;
        }

        while (childNode?.Parent is not null)
        {
            ancestors.Add(childNode.Parent.Key);
            childNode = childNode.Parent;
        }

        ancestorsKeys = ancestors;
        return true;
    }

    private bool TryGetSiblingsKeysFromStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid key, out IEnumerable<Guid> siblingsKeys)
    {
        siblingsKeys = [];

        if (structure.TryGetValue(key, out NavigationNode? node) is false)
        {
            return false; // Node doesn't exist
        }

        if (node.Parent is null)
        {
            // To find siblings of a node at root level, we need to iterate over all items and add those with null Parent
            siblingsKeys = structure
                .Where(kv => kv.Value.Parent is null && kv.Key != key)
                .Select(kv => kv.Key)
                .ToList();
            return true;
        }

        if (TryGetChildrenKeys(node.Parent.Key, out IEnumerable<Guid> childrenKeys) is false)
        {
            return false; // Couldn't retrieve children keys
        }

        // Filter out the node itself to get its siblings
        siblingsKeys = childrenKeys.Where(childKey => childKey != key).ToList();
        return true;
    }

    private void GetDescendantsRecursively(NavigationNode node, List<Guid> descendants)
    {
        foreach (NavigationNode child in node.Children)
        {
            descendants.Add(child.Key);
            GetDescendantsRecursively(child, descendants);
        }
    }

    private bool TryRemoveNodeFromParentInStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid key, out NavigationNode? nodeToRemove)
    {
        if (structure.TryGetValue(key, out nodeToRemove) is false)
        {
            return false; // Node doesn't exist
        }

        // Remove the node from its parent's children list
        if (nodeToRemove.Parent is not null && structure.TryGetValue(nodeToRemove.Parent.Key, out NavigationNode? parentNode))
        {
            parentNode.RemoveChild(nodeToRemove);
        }

        return true;
    }

    private void AddDescendantsToRecycleBinRecursively(NavigationNode node)
    {
        foreach (NavigationNode child in node.Children)
        {
            AddDescendantsToRecycleBinRecursively(child);

            // Only remove the child from the main structure if it was successfully added to the recycle bin
            if (_recycleBinNavigationStructure.TryAdd(child.Key, child))
            {
                _navigationStructure.TryRemove(child.Key, out _);
            }
        }
    }

    private void RemoveDescendantsRecursively(NavigationNode node)
    {
        foreach (NavigationNode child in node.Children)
        {
            RemoveDescendantsRecursively(child);
            _recycleBinNavigationStructure.TryRemove(child.Key, out _);
        }
    }

    private void RestoreNodeAndDescendantsRecursively(NavigationNode node)
    {
        foreach (NavigationNode child in node.Children)
        {
            RestoreNodeAndDescendantsRecursively(child);

            // Only remove the child from the recycle bin structure if it was successfully added to the main one
            if (_navigationStructure.TryAdd(child.Key, child))
            {
                _recycleBinNavigationStructure.TryRemove(child.Key, out _);
            }
        }
    }
}
