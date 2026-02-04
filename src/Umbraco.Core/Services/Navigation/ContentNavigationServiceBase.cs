using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Navigation;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Abstract base class for content navigation services that provides common functionality
///     for managing hierarchical navigation structures for content and media items.
/// </summary>
/// <typeparam name="TContentType">The type of content type, must implement <see cref="IContentTypeComposition"/>.</typeparam>
/// <typeparam name="TContentTypeService">The type of content type service, must implement <see cref="IContentTypeBaseService{TContentType}"/>.</typeparam>
/// <remarks>
///     This class maintains two navigation structures: a main structure for active content
///     and a recycle bin structure for trashed content. Both structures use concurrent
///     dictionaries to ensure thread-safe operations.
/// </remarks>
internal abstract class ContentNavigationServiceBase<TContentType, TContentTypeService>
    where TContentType : class, IContentTypeComposition
    where TContentTypeService : IContentTypeBaseService<TContentType>
{
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly INavigationRepository _navigationRepository;
    private readonly TContentTypeService _typeService;
    private Lazy<Dictionary<string, Guid>> _contentTypeAliasToKeyMap;
    private ConcurrentDictionary<Guid, NavigationNode> _navigationStructure = new();
    private ConcurrentDictionary<Guid, NavigationNode> _recycleBinNavigationStructure = new();
    private HashSet<Guid> _roots = [];
    private HashSet<Guid> _recycleBinRoots = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentNavigationServiceBase{TContentType, TContentTypeService}"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The core scope provider for database operations.</param>
    /// <param name="navigationRepository">The repository for accessing navigation data.</param>
    /// <param name="typeService">The content type service for retrieving content type information.</param>
    protected ContentNavigationServiceBase(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository, TContentTypeService typeService)
    {
        _coreScopeProvider = coreScopeProvider;
        _navigationRepository = navigationRepository;
        _typeService = typeService;
        _contentTypeAliasToKeyMap = new Lazy<Dictionary<string, Guid>>(LoadContentTypes);
    }

    /// <summary>
    ///     Rebuilds the entire main navigation structure. Implementations should define how the structure is rebuilt.
    /// </summary>
    public abstract Task RebuildAsync();

    /// <summary>
    ///     Rebuilds the recycle bin navigation structure. Implementations should define how the bin structure is rebuilt.
    /// </summary>
    public abstract Task RebuildBinAsync();

    /// <summary>
    ///     Attempts to get the parent key of a child node in the main navigation structure.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node.</param>
    /// <param name="parentKey">
    ///     When this method returns, contains the parent's unique identifier if the child exists;
    ///     otherwise, <c>null</c>. The value will be <c>null</c> if the child is at root level.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the structure; otherwise, <c>false</c>.</returns>
    public bool TryGetParentKey(Guid childKey, out Guid? parentKey)
        => TryGetParentKeyFromStructure(_navigationStructure, childKey, out parentKey);

    /// <summary>
    ///     Attempts to get all root-level node keys from the main navigation structure.
    /// </summary>
    /// <param name="rootKeys">
    ///     When this method returns, contains the collection of root node keys ordered by sort order.
    /// </param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool TryGetRootKeys(out IEnumerable<Guid> rootKeys)
        => TryGetRootKeysFromStructure(_roots, out rootKeys);

    /// <summary>
    ///     Attempts to get all root-level node keys of a specific content type from the main navigation structure.
    /// </summary>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="rootKeys">
    ///     When this method returns, contains the collection of root node keys of the specified
    ///     content type, ordered by sort order; or an empty collection if the content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the content type exists and root keys were retrieved; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetRootKeysOfType(string contentTypeAlias, out IEnumerable<Guid> rootKeys)
    {
        if (TryGetContentTypeKey(contentTypeAlias, out Guid? contentTypeKey))
        {
            return TryGetRootKeysFromStructure(_roots, out rootKeys, contentTypeKey);
        }

        // Content type alias doesn't exist
        rootKeys = [];
        return false;
    }

    /// <summary>
    ///     Attempts to get all child node keys of a parent node in the main navigation structure.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="childrenKeys">
    ///     When this method returns, contains the collection of child node keys ordered by sort order;
    ///     or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the structure; otherwise, <c>false</c>.</returns>
    public bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys)
        => TryGetChildrenKeysFromStructure(_navigationStructure, parentKey, out childrenKeys);

    /// <summary>
    ///     Attempts to get all child node keys of a specific content type under a parent node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="childrenKeys">
    ///     When this method returns, contains the collection of child node keys of the specified
    ///     content type, ordered by sort order; or an empty collection if the parent or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the parent and content type exist and children were retrieved; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetChildrenKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> childrenKeys)
    {
        if (TryGetContentTypeKey(contentTypeAlias, out Guid? contentTypeKey))
        {
            return TryGetChildrenKeysFromStructure(_navigationStructure, parentKey, out childrenKeys, contentTypeKey);
        }

        // Content type alias doesn't exist
        childrenKeys = [];
        return false;
    }

    /// <summary>
    ///     Attempts to get all descendant node keys of a parent node in the main navigation structure.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="descendantsKeys">
    ///     When this method returns, contains the collection of all descendant node keys
    ///     (children, grandchildren, etc.) in depth-first order; or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the structure; otherwise, <c>false</c>.</returns>
    public bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys)
        => TryGetDescendantsKeysFromStructure(_navigationStructure, parentKey, out descendantsKeys);

    /// <summary>
    ///     Attempts to get all descendant node keys of a specific content type under a parent node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="descendantsKeys">
    ///     When this method returns, contains the collection of descendant node keys of the specified
    ///     content type; or an empty collection if the parent or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the parent and content type exist and descendants were retrieved; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetDescendantsKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> descendantsKeys)
    {
        if (TryGetContentTypeKey(contentTypeAlias, out Guid? contentTypeKey))
        {
            return TryGetDescendantsKeysFromStructure(_navigationStructure, parentKey, out descendantsKeys, contentTypeKey);
        }

        // Content type alias doesn't exist
        descendantsKeys = [];
        return false;
    }

    /// <summary>
    ///     Attempts to get all ancestor node keys of a child node in the main navigation structure.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node.</param>
    /// <param name="ancestorsKeys">
    ///     When this method returns, contains the collection of ancestor node keys
    ///     (parent, grandparent, etc.) starting from the immediate parent; or an empty collection if the child doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the structure; otherwise, <c>false</c>.</returns>
    public bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys)
        => TryGetAncestorsKeysFromStructure(_navigationStructure, childKey, out ancestorsKeys);

    /// <summary>
    ///     Attempts to get all ancestor node keys of a specific content type for a given node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the node to find ancestors for.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="ancestorsKeys">
    ///     When this method returns, contains the collection of ancestor node keys of the specified
    ///     content type; or an empty collection if the node or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node and content type exist and ancestors were retrieved; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetAncestorsKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> ancestorsKeys)
    {
        if (TryGetContentTypeKey(contentTypeAlias, out Guid? contentTypeKey))
        {
            return TryGetAncestorsKeysFromStructure(_navigationStructure, parentKey, out ancestorsKeys, contentTypeKey);
        }

        // Content type alias doesn't exist
        ancestorsKeys = [];
        return false;
    }

    /// <summary>
    ///     Attempts to get all sibling node keys of a node in the main navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node.</param>
    /// <param name="siblingsKeys">
    ///     When this method returns, contains the collection of sibling node keys
    ///     (nodes with the same parent, excluding the node itself), ordered by sort order;
    ///     or an empty collection if the node doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the node exists in the structure; otherwise, <c>false</c>.</returns>
    public bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys)
        => TryGetSiblingsKeysFromStructure(_navigationStructure, key, out siblingsKeys);

    /// <summary>
    ///     Attempts to get all sibling node keys of a specific content type for a given node.
    /// </summary>
    /// <param name="key">The unique identifier of the node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="siblingsKeys">
    ///     When this method returns, contains the collection of sibling node keys of the specified
    ///     content type, ordered by sort order; or an empty collection if the node or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node and content type exist and siblings were retrieved; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetSiblingsKeysOfType(Guid key, string contentTypeAlias, out IEnumerable<Guid> siblingsKeys)
    {
        if (TryGetContentTypeKey(contentTypeAlias, out Guid? contentTypeKey))
        {
            return TryGetSiblingsKeysFromStructure(_navigationStructure, key, out siblingsKeys, contentTypeKey);
        }

        // Content type alias doesn't exist
        siblingsKeys = [];
        return false;
    }

    /// <summary>
    ///     Attempts to get the parent key of a child node in the recycle bin navigation structure.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node in the recycle bin.</param>
    /// <param name="parentKey">
    ///     When this method returns, contains the parent's unique identifier if the child exists in the bin;
    ///     otherwise, <c>null</c>. The value will be <c>null</c> if the child is at bin root level.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    public bool TryGetParentKeyInBin(Guid childKey, out Guid? parentKey)
        => TryGetParentKeyFromStructure(_recycleBinNavigationStructure, childKey, out parentKey);

    /// <summary>
    ///     Attempts to get all child node keys of a parent node in the recycle bin navigation structure.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node in the recycle bin.</param>
    /// <param name="childrenKeys">
    ///     When this method returns, contains the collection of child node keys ordered by sort order;
    ///     or an empty collection if the parent doesn't exist in the bin.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    public bool TryGetChildrenKeysInBin(Guid parentKey, out IEnumerable<Guid> childrenKeys)
        => TryGetChildrenKeysFromStructure(_recycleBinNavigationStructure, parentKey, out childrenKeys);

    /// <summary>
    ///     Attempts to get all descendant node keys of a parent node in the recycle bin navigation structure.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node in the recycle bin.</param>
    /// <param name="descendantsKeys">
    ///     When this method returns, contains the collection of all descendant node keys in the bin;
    ///     or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    public bool TryGetDescendantsKeysInBin(Guid parentKey, out IEnumerable<Guid> descendantsKeys)
        => TryGetDescendantsKeysFromStructure(_recycleBinNavigationStructure, parentKey, out descendantsKeys);

    /// <summary>
    ///     Attempts to get all ancestor node keys of a child node in the recycle bin navigation structure.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node in the recycle bin.</param>
    /// <param name="ancestorsKeys">
    ///     When this method returns, contains the collection of ancestor node keys in the bin;
    ///     or an empty collection if the child doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    public bool TryGetAncestorsKeysInBin(Guid childKey, out IEnumerable<Guid> ancestorsKeys)
        => TryGetAncestorsKeysFromStructure(_recycleBinNavigationStructure, childKey, out ancestorsKeys);

    /// <summary>
    ///     Attempts to get all sibling node keys of a node in the recycle bin navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node in the recycle bin.</param>
    /// <param name="siblingsKeys">
    ///     When this method returns, contains the collection of sibling node keys in the bin,
    ///     ordered by sort order; or an empty collection if the node doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    public bool TryGetSiblingsKeysInBin(Guid key, out IEnumerable<Guid> siblingsKeys)
        => TryGetSiblingsKeysFromStructure(_recycleBinNavigationStructure, key, out siblingsKeys);

    /// <summary>
    ///     Attempts to get the hierarchical level of a content node in the main navigation structure.
    /// </summary>
    /// <param name="contentKey">The unique identifier of the content node.</param>
    /// <param name="level">
    ///     When this method returns, contains the level of the node (1 for root-level nodes,
    ///     2 for their children, etc.); or <c>null</c> if the node doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the node exists and its level was determined; otherwise, <c>false</c>.</returns>
    public bool TryGetLevel(Guid contentKey, [NotNullWhen(true)] out int? level)
    {
        level = 1;
        if (TryGetParentKey(contentKey, out Guid? parentKey) is false)
        {
            level = null;
            return false;
        }

        while (parentKey is not null)
        {
            if (TryGetParentKey(parentKey.Value, out parentKey) is false)
            {
                level = null;
                return false;
            }

            level++;
        }

        return true;
    }

    /// <summary>
    ///     Moves a node and all its descendants from the main navigation structure to the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the node to move to the recycle bin.</param>
    /// <returns>
    ///     <c>true</c> if the node and its descendants were successfully moved to the recycle bin;
    ///     otherwise, <c>false</c> if the node doesn't exist.
    /// </returns>
    public bool MoveToBin(Guid key)
    {
        if (TryRemoveNodeFromParentInStructure(_navigationStructure, key, out NavigationNode? nodeToRemove) is false || nodeToRemove is null)
        {
            return false; // Node doesn't exist
        }

        // Recursively remove all descendants and add them to recycle bin
        AddDescendantsToRecycleBinRecursively(nodeToRemove);

        // Reset the SortOrder based on its new position in the bin
        nodeToRemove.UpdateSortOrder(_recycleBinNavigationStructure.Count);
        return _recycleBinNavigationStructure.TryAdd(nodeToRemove.Key, nodeToRemove) &&
               _navigationStructure.TryRemove(key, out _);
    }

    /// <summary>
    ///     Adds a new node to the main navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the new node.</param>
    /// <param name="contentTypeKey">The unique identifier of the node's content type.</param>
    /// <param name="parentKey">
    ///     The unique identifier of the parent node. If <c>null</c>, the node is added at root level.
    /// </param>
    /// <param name="sortOrder">
    ///     The sort order for the new node. Required when adding nodes at root level.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node was successfully added; otherwise, <c>false</c> if the parent
    ///     doesn't exist or a node with the same key already exists.
    /// </returns>
    public bool Add(Guid key, Guid contentTypeKey, Guid? parentKey = null, int? sortOrder = null)
    {
        NavigationNode? parentNode = null;
        if (parentKey.HasValue)
        {
            if (_navigationStructure.TryGetValue(parentKey.Value, out parentNode) is false)
            {
                return false; // Parent node doesn't exist
            }
        }
        else
        {
            _roots.Add(key);
        }

        // Note: sortOrder can't be automatically determined for items at root level, so it needs to be passed in
        var newNode = new NavigationNode(key, contentTypeKey, sortOrder ?? 0);
        if (_navigationStructure.TryAdd(key, newNode) is false)
        {
            return false; // Node with this key already exists
        }

        parentNode?.AddChild(_navigationStructure, key);

        return true;
    }

    /// <summary>
    ///     Moves an existing node to a new location in the main navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node to move.</param>
    /// <param name="targetParentKey">
    ///     The unique identifier of the new parent node. If <c>null</c>, the node is moved to root level.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node was successfully moved; otherwise, <c>false</c> if the node doesn't exist,
    ///     the target parent doesn't exist, or the node is being moved to itself.
    /// </returns>
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

        _roots.Remove(key); // Just in case

        NavigationNode? targetParentNode = null;
        if (targetParentKey.HasValue)
        {
            if (_navigationStructure.TryGetValue(targetParentKey.Value, out targetParentNode) is false)
            {
                return false; // Target parent doesn't exist
            }
        }
        else
        {
            _roots.Add(key);
        }

        // Remove the node from its current parent's children list
        if (nodeToMove.Parent is not null && _navigationStructure.TryGetValue(nodeToMove.Parent.Value, out NavigationNode? currentParentNode))
        {
            currentParentNode.RemoveChild(_navigationStructure, key);
        }

        // Set the new parent for the node (if parent node is null - the node is moved to root)
        targetParentNode?.AddChild(_navigationStructure, key);

        return true;
    }

    /// <summary>
    ///     Updates the sort order of a node in the main navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node to update.</param>
    /// <param name="newSortOrder">The new sort order value.</param>
    /// <returns>
    ///     <c>true</c> if the sort order was successfully updated; otherwise, <c>false</c> if the node doesn't exist.
    /// </returns>
    public bool UpdateSortOrder(Guid key, int newSortOrder)
    {
        if (_navigationStructure.TryGetValue(key, out NavigationNode? node) is false)
        {
            return false; // Node doesn't exist
        }

        node.UpdateSortOrder(newSortOrder);

        return true;
    }

    /// <summary>
    ///     Permanently removes a node and all its descendants from the recycle bin navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node to remove from the recycle bin.</param>
    /// <returns>
    ///     <c>true</c> if the node and its descendants were successfully removed from the recycle bin;
    ///     otherwise, <c>false</c> if the node doesn't exist in the bin.
    /// </returns>
    public bool RemoveFromBin(Guid key)
    {
        if (TryRemoveNodeFromParentInStructure(_recycleBinNavigationStructure, key, out NavigationNode? nodeToRemove) is false || nodeToRemove is null)
        {
            return false; // Node doesn't exist
        }

        _recycleBinRoots.Remove(key);

        RemoveDescendantsRecursively(nodeToRemove);

        return _recycleBinNavigationStructure.TryRemove(key, out _);
    }

    /// <summary>
    ///     Restores a node and all its descendants from the recycle bin to the main navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node to restore.</param>
    /// <param name="targetParentKey">
    ///     The unique identifier of the target parent node in the main structure.
    ///     If <c>null</c>, the node is restored to root level.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node and its descendants were successfully restored;
    ///     otherwise, <c>false</c> if the node doesn't exist in the bin or the target parent doesn't exist.
    /// </returns>
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
        targetParentNode?.AddChild(_recycleBinNavigationStructure, key);

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
    protected Task HandleRebuildAsync(int readLock, Guid objectTypeKey, bool trashed)
    {
        // This is only relevant for items in the content and media trees
        if (readLock != Constants.Locks.ContentTree && readLock != Constants.Locks.MediaTree)
        {
            return Task.CompletedTask;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(readLock);

        // Build the corresponding navigation structure
        if (trashed)
        {
            _recycleBinRoots.Clear();
            IEnumerable<INavigationModel> navigationModels = _navigationRepository.GetTrashedContentNodesByObjectType(objectTypeKey);
            BuildNavigationDictionary(_recycleBinNavigationStructure, _recycleBinRoots,  navigationModels);
        }
        else
        {
            _roots.Clear();
            IEnumerable<INavigationModel> navigationModels = _navigationRepository.GetContentNodesByObjectType(objectTypeKey);
            BuildNavigationDictionary(_navigationStructure, _roots, navigationModels);
        }

        return Task.CompletedTask;
    }

    private static bool TryGetParentKeyFromStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid childKey, out Guid? parentKey)
    {
        if (structure.TryGetValue(childKey, out NavigationNode? childNode))
        {
            parentKey = childNode.Parent;
            return true;
        }

        // Child doesn't exist
        parentKey = null;
        return false;
    }

    private bool TryGetRootKeysFromStructure(
        HashSet<Guid> input,
        out IEnumerable<Guid> rootKeys,
        Guid? contentTypeKey = null)
    {
        var keysWithSortOrder = new List<(Guid Key, int SortOrder)>(input.Count);
        foreach (Guid key in input)
        {
            NavigationNode navigationNode = _navigationStructure[key];

            // Apply contentTypeKey filter
            if (contentTypeKey.HasValue && navigationNode.ContentTypeKey != contentTypeKey.Value)
            {
                continue;
            }

            keysWithSortOrder.Add((key, navigationNode.SortOrder));
        }

        // Sort by SortOrder
        keysWithSortOrder.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        rootKeys = keysWithSortOrder.ConvertAll(keyWithSortOrder => keyWithSortOrder.Key);

        return true;
    }

    private static bool TryGetChildrenKeysFromStructure(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        Guid parentKey,
        out IEnumerable<Guid> childrenKeys,
        Guid? contentTypeKey = null)
    {
        if (structure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            childrenKeys = [];
            return false;
        }

        // Keep children keys ordered based on their SortOrder
        childrenKeys = GetOrderedChildren(parentNode, structure, contentTypeKey);

        return true;
    }

    private static bool TryGetDescendantsKeysFromStructure(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        Guid parentKey,
        out IEnumerable<Guid> descendantsKeys,
        Guid? contentTypeKey = null)
    {
        var descendants = new List<Guid>();

        if (structure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            descendantsKeys = [];
            return false;
        }

        GetDescendantsRecursively(structure, parentNode, descendants, contentTypeKey);

        descendantsKeys = descendants;
        return true;
    }

    private static bool TryGetAncestorsKeysFromStructure(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        Guid childKey,
        out IEnumerable<Guid> ancestorsKeys,
        Guid? contentTypeKey = null)
    {
        var ancestors = new List<Guid>();

        if (structure.TryGetValue(childKey, out NavigationNode? node) is false)
        {
            // Child doesn't exist
            ancestorsKeys = [];
            return false;
        }

        while (node.Parent is not null && structure.TryGetValue(node.Parent.Value, out node))
        {
            // Apply contentTypeKey filter
            if (contentTypeKey.HasValue is false || node.ContentTypeKey == contentTypeKey.Value)
            {
                ancestors.Add(node.Key);
            }
        }

        ancestorsKeys = ancestors;
        return true;
    }

    private static bool TryGetSiblingsKeysFromStructure(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        Guid key,
        out IEnumerable<Guid> siblingsKeys,
        Guid? contentTypeKey = null)
    {
        siblingsKeys = [];

        if (structure.TryGetValue(key, out NavigationNode? node) is false)
        {
            return false; // Node doesn't exist
        }

        if (node.Parent is null)
        {
            // To find siblings of a node at root level, we need to iterate over all items and add those with null Parent
            IEnumerable<KeyValuePair<Guid, NavigationNode>> filteredSiblings = structure
                .Where(kv => kv.Value.Parent is null && kv.Key != key);

            // Apply contentTypeKey filter
            if (contentTypeKey.HasValue)
            {
                filteredSiblings = filteredSiblings.Where(kv => kv.Value.ContentTypeKey == contentTypeKey.Value);
            }

            siblingsKeys = filteredSiblings
                .OrderBy(kv => kv.Value.SortOrder)
                .Select(kv => kv.Key)
                .ToList();
            return true;
        }

        if (TryGetChildrenKeysFromStructure(structure, node.Parent.Value, out IEnumerable<Guid> childrenKeys, contentTypeKey) is false)
        {
            return false; // Couldn't retrieve children keys
        }

        // Filter out the node itself to get its siblings
        siblingsKeys = childrenKeys.Where(childKey => childKey != key).ToList();
        return true;
    }

    private static void GetDescendantsRecursively(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        NavigationNode node,
        List<Guid> descendants,
        Guid? contentTypeKey = null)
    {
        // Get all children regardless of contentType
        IReadOnlyList<Guid> childrenKeys = GetOrderedChildren(node, structure);
        foreach (Guid childKey in childrenKeys)
        {
            // Apply contentTypeKey filter
            if (contentTypeKey.HasValue is false || structure[childKey].ContentTypeKey == contentTypeKey.Value)
            {
                descendants.Add(childKey);
            }

            // Retrieve the child node and its descendants
            if (structure.TryGetValue(childKey, out NavigationNode? childNode))
            {
                GetDescendantsRecursively(structure, childNode, descendants, contentTypeKey);
            }
        }
    }

    private static bool TryRemoveNodeFromParentInStructure(ConcurrentDictionary<Guid, NavigationNode> structure, Guid key, out NavigationNode? nodeToRemove)
    {
        if (structure.TryGetValue(key, out nodeToRemove) is false)
        {
            return false; // Node doesn't exist
        }

        // Remove the node from its parent's children list
        if (nodeToRemove.Parent is not null && structure.TryGetValue(nodeToRemove.Parent.Value, out NavigationNode? parentNode))
        {
            parentNode.RemoveChild(structure, key);
        }

        return true;
    }

    private void AddDescendantsToRecycleBinRecursively(NavigationNode node)
    {
        _recycleBinRoots.Add(node.Key);
        _roots.Remove(node.Key);
        IReadOnlyList<Guid> childrenKeys = GetOrderedChildren(node, _navigationStructure);

        foreach (Guid childKey in childrenKeys)
        {
            if (_navigationStructure.TryGetValue(childKey, out NavigationNode? childNode) is false)
            {
                continue;
            }

            // Reset the SortOrder based on its new position in the bin
            childNode.UpdateSortOrder(_recycleBinNavigationStructure.Count);
            AddDescendantsToRecycleBinRecursively(childNode);

            // Only remove the child from the main structure if it was successfully added to the recycle bin
            if (_recycleBinNavigationStructure.TryAdd(childKey, childNode))
            {
                _navigationStructure.TryRemove(childKey, out _);
            }
        }
    }

    private void RemoveDescendantsRecursively(NavigationNode node)
    {
        IReadOnlyList<Guid> childrenKeys = GetOrderedChildren(node, _recycleBinNavigationStructure);
        foreach (Guid childKey in childrenKeys)
        {
            if (_recycleBinNavigationStructure.TryGetValue(childKey, out NavigationNode? childNode) is false)
            {
                continue;
            }

            RemoveDescendantsRecursively(childNode);
            _recycleBinNavigationStructure.TryRemove(childKey, out _);
        }
    }

    private void RestoreNodeAndDescendantsRecursively(NavigationNode node)
    {
        if (node.Parent is null)
        {
            _roots.Add(node.Key);
        }

        _recycleBinRoots.Remove(node.Key);
        IReadOnlyList<Guid> childrenKeys = GetOrderedChildren(node, _recycleBinNavigationStructure);

        foreach (Guid childKey in childrenKeys)
        {
            if (_recycleBinNavigationStructure.TryGetValue(childKey, out NavigationNode? childNode) is false)
            {
                continue;
            }

            RestoreNodeAndDescendantsRecursively(childNode);

            // Only remove the child from the recycle bin structure if it was successfully added to the main one
            if (_navigationStructure.TryAdd(childKey, childNode))
            {
                _recycleBinNavigationStructure.TryRemove(childKey, out _);
            }
        }
    }

    private static IReadOnlyList<Guid> GetOrderedChildren(
        NavigationNode node,
        ConcurrentDictionary<Guid, NavigationNode> structure,
        Guid? contentTypeKey = null)
    {
        if (node.Children.Count < 1)
        {
            return [];
        }

        var childrenWithSortOrder = new List<(Guid ChildNodeKey, int SortOrder)>(node.Children.Count);
        foreach (Guid childNodeKey in node.Children)
        {
            if (!structure.TryGetValue(childNodeKey, out NavigationNode? childNode))
            {
                continue;
            }

            // Apply contentTypeKey filter
            if (contentTypeKey.HasValue && childNode.ContentTypeKey != contentTypeKey.Value)
            {
                continue;
            }

            childrenWithSortOrder.Add((childNodeKey, childNode.SortOrder));
        }

        childrenWithSortOrder.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        return childrenWithSortOrder.ConvertAll(childWithSortOrder => childWithSortOrder.ChildNodeKey);
    }

    private bool TryGetContentTypeKey(string contentTypeAlias, out Guid? contentTypeKey)
    {
        Dictionary<string, Guid> aliasToKeyMap = _contentTypeAliasToKeyMap.Value;

        if (aliasToKeyMap.TryGetValue(contentTypeAlias, out Guid key))
        {
            contentTypeKey = key;
            return true;
        }

        TContentType? contentType = _typeService.Get(contentTypeAlias);
        if (contentType is null)
        {
            // Content type alias doesn't exist
            contentTypeKey = null;
            return false;
        }

        aliasToKeyMap.TryAdd(contentTypeAlias, contentType.Key);
        contentTypeKey = contentType.Key;
        return true;
    }

    private static void BuildNavigationDictionary(ConcurrentDictionary<Guid, NavigationNode> nodesStructure, HashSet<Guid> roots, IEnumerable<INavigationModel> entities)
    {
        var entityList = entities.ToList();
        var idToKeyMap = entityList.ToDictionary(x => x.Id, x => x.Key);

        foreach (INavigationModel entity in entityList)
        {
            var node = new NavigationNode(entity.Key, entity.ContentTypeKey, entity.SortOrder);
            nodesStructure[entity.Key] = node;

            // We don't set the parent for items under root, it will stay null
            if (entity.ParentId == -1)
            {
                roots.Add(entity.Key);
                continue;
            }

            if (idToKeyMap.TryGetValue(entity.ParentId, out Guid parentKey) is false)
            {
                continue;
            }

            // If the parent node exists in the nodesStructure, add the node to the parent's children (parent is set as well)
            if (nodesStructure.TryGetValue(parentKey, out NavigationNode? parentNode))
            {
                parentNode.AddChild(nodesStructure, entity.Key);
            }
        }
    }

    private Dictionary<string, Guid> LoadContentTypes()
        => _typeService.GetAll().ToDictionary(ct => ct.Alias, ct => ct.Key);
}
