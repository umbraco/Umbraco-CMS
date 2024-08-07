using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

internal class ContentNavigationService : INavigationService
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly INavigationRepository _navigationRepository;
    private ConcurrentDictionary<Guid, NavigationNode> _navigationStructure = new();

    public ContentNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository)
    {
        _coreScopeProvider = coreScopeProvider;
        _navigationRepository = navigationRepository;
    }

    public async Task RebuildAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.ContentTree);
        _navigationStructure = _navigationRepository.GetContentNodesByObjectType(Constants.ObjectTypes.Document);
    }

    public bool TryGetParentKey(Guid childKey, out Guid? parentKey)
    {
        if (_navigationStructure.TryGetValue(childKey, out NavigationNode? childNode))
        {
            parentKey = childNode.Parent?.Key;
            return true;
        }

        // Child doesn't exist
        parentKey = null;
        return false;
    }

    public bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys)
    {
        if (_navigationStructure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            childrenKeys = [];
            return false;
        }

        childrenKeys = parentNode.Children.Select(child => child.Key);
        return true;
    }

    public bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys)
    {
        var descendants = new List<Guid>();

        if (_navigationStructure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            descendantsKeys = [];
            return false;
        }

        GetDescendantsRecursively(parentNode, descendants);

        descendantsKeys = descendants;
        return true;
    }

    public bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys)
    {
        var ancestors = new List<Guid>();

        if (_navigationStructure.TryGetValue(childKey, out NavigationNode? childNode) is false)
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

    public bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys)
    {
        siblingsKeys = [];

        if (_navigationStructure.TryGetValue(key, out NavigationNode? node) is false)
        {
            return false; // Node doesn't exist
        }

        if (node.Parent is null)
        {
            // To find siblings of a node at root level, we need to iterate over all items and add those with null Parent
            siblingsKeys = _navigationStructure
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

    public bool Remove(Guid key)
    {
        if (_navigationStructure.TryGetValue(key, out NavigationNode? nodeToRemove) is false)
        {
            return false; // Node doesn't exist
        }

        // Remove the node from its parent's children list
        if (nodeToRemove.Parent is not null && _navigationStructure.TryGetValue(nodeToRemove.Parent.Key, out NavigationNode? parentNode))
        {
            parentNode.RemoveChild(nodeToRemove);
        }

        // Recursively remove all descendants
        RemoveDescendantsRecursively(nodeToRemove);

        // Remove the node itself
        return _navigationStructure.TryRemove(key, out _);
    }

    public bool Add(Guid key, Guid? parentKey = null)
    {
        if (_navigationStructure.ContainsKey(key))
        {
            return false; // Node with this key already exists
        }

        NavigationNode? parentNode = null;
        if (parentKey.HasValue)
        {
            if (_navigationStructure.TryGetValue(parentKey.Value, out parentNode) is false)
            {
                return false; // Parent node doesn't exist
            }
        }

        var newNode = new NavigationNode(key);
        _navigationStructure[key] = newNode;

        parentNode?.AddChild(newNode);

        return true;
    }

    public bool Move(Guid nodeKey, Guid? targetParentKey = null)
    {
        if (_navigationStructure.TryGetValue(nodeKey, out NavigationNode? nodeToMove) is false)
        {
            return false; // Node doesn't exist
        }

        if (nodeKey == targetParentKey)
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

        // Create a new node with the same key, to update the parent
        var newNode = new NavigationNode(nodeToMove.Key);

        // Set the new parent for the node (if parent node is null - the node is moved to root)
        targetParentNode?.AddChild(newNode);

        // Copy children to the new node
        CopyChildren(nodeToMove, newNode);

        _navigationStructure[nodeToMove.Key] = newNode;

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

    private void RemoveDescendantsRecursively(NavigationNode node)
    {
        foreach (NavigationNode child in node.Children)
        {
            RemoveDescendantsRecursively(child);
            _navigationStructure.TryRemove(child.Key, out _);
        }
    }

    private void CopyChildren(NavigationNode originalNode, NavigationNode newNode)
    {
        foreach (NavigationNode child in originalNode.Children)
        {
            newNode.AddChild(child);
        }
    }
}
