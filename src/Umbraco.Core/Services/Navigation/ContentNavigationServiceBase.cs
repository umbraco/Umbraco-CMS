using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
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

    // Concurrent because TryGetContentTypeKey lazily adds aliases resolved after the initial load,
    // on live request threads; a non-concurrent map corrupts under parallel misses. See #23518.
    private readonly Lazy<ConcurrentDictionary<string, Guid>> _contentTypeAliasToKeyMap;

#pragma warning disable CS0419 // Ambiguous reference in cref attribute
    /// <summary>
    ///     Bundles a navigation structure dictionary and its root keys into a single reference so that
    ///     <see cref="HandleRebuildAsync"/> can swap both atomically with one <see cref="Interlocked.Exchange{T}"/>
    ///     call and readers always observe a consistent pair. Also carries the per-snapshot
    ///     descendants cache populated by <see cref="TryGetDescendantsKeysFromStructure"/>.
    /// </summary>
#pragma warning restore CS0419 // Ambiguous reference in cref attribute
    private sealed record NavigationSnapshot(
        ConcurrentDictionary<Guid, NavigationNode> Structure,
        ConcurrentHashSet<Guid> Roots)
    {
        private long _generation;

        /// <summary>
        ///     Cache of descendants <c>Guid[]</c> keyed by parent and an optional content-type
        ///     filter. Populated lazily by <see cref="TryGetDescendantsKeysFromStructure"/> and
        ///     cleared by <see cref="Invalidate"/> on any structural mutation.
        /// </summary>
        /// <remarks>
        ///     The composite key allows both <c>TryGetDescendantsKeys</c> (content-type =
        ///     <c>null</c>) and <c>TryGetDescendantsKeysOfType</c> (content-type = the resolved
        ///     <c>Guid</c>) to share one cache without their results contaminating each other.
        ///     Realistic per-parent fan-out is bounded by the "allowed types" content model
        ///     (typically 1-5 types per parent), and the cache is populated only for queries
        ///     that actually run, so memory grows with the templates exercised rather than the
        ///     theoretical product of (parents × content types).
        /// </remarks>
        public ConcurrentDictionary<(Guid Parent, Guid? ContentType), Guid[]> DescendantsCache { get; } = new();

        /// <summary>
        ///     A monotonic counter incremented on every mutation. Used by readers to detect a
        ///     concurrent mutation that occurred during their compute, so they can avoid writing
        ///     a now-stale result back to <see cref="DescendantsCache"/>.
        /// </summary>
        public long Generation => Interlocked.Read(ref _generation);

        /// <summary>
        ///     Clears the descendants cache and bumps the generation. Call after any mutation to
        ///     this snapshot's <see cref="Structure"/> or <see cref="Roots"/>.
        /// </summary>
        public void Invalidate()
        {
            Interlocked.Increment(ref _generation);
            DescendantsCache.Clear();
        }
    }

    private NavigationSnapshot _navigation = new(new(), []);
    private NavigationSnapshot _recycleBinNavigation = new(new(), []);

    /// <summary>
    ///     Gets the approximate number of nodes currently held in memory across the active navigation
    ///     structure and the recycle bin structure, for diagnostics. Each snapshot reference is read once,
    ///     so the count is consistent per structure even if a rebuild swaps a snapshot concurrently.
    /// </summary>
    private protected long GetNavigationNodeCount()
        => _navigation.Structure.Count + _recycleBinNavigation.Structure.Count;

    /// <summary>
    ///     Gets an approximate retained size, in bytes, of the navigation structures (active tree plus
    ///     recycle bin), for diagnostics. Sampled and structural — a coarse estimate, not a heap measurement.
    /// </summary>
    private protected long GetNavigationApproximateBytes()
        => EstimateStructureBytes(_navigation.Structure) + EstimateStructureBytes(_recycleBinNavigation.Structure);

    // The dictionary is enumerated directly (not via .Values, which snapshot-copies the whole collection).
    // Per-node estimate: fixed fields (key, content-type key, parent, sort order, lock) + dictionary bucket,
    // plus an allowance per child key (held in the child set and the cached ordered array).
    private static long EstimateStructureBytes(ConcurrentDictionary<Guid, NavigationNode> structure)
        => SampledSizeEstimator.Estimate(structure.Count, structure, static kvp => 120 + (40L * kvp.Value.Children.Count));

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
        _contentTypeAliasToKeyMap = new Lazy<ConcurrentDictionary<string, Guid>>(LoadContentTypes);
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
        => TryGetParentKeyFromStructure(_navigation.Structure, childKey, out parentKey);

    /// <summary>
    ///     Attempts to get all root-level node keys from the main navigation structure.
    /// </summary>
    /// <param name="rootKeys">
    ///     When this method returns, contains the collection of root node keys ordered by sort order.
    /// </param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool TryGetRootKeys(out IEnumerable<Guid> rootKeys)
    {
        // On subscriber/CD servers in a load-balanced setup, cache refresh notifications trigger a full navigation
        // rebuild (via RebuildAsync → HandleRebuildAsync), which replaces the NavigationSnapshot with a new instance.
        // Reading the snapshot into a local guarantees Structure and Roots are from the same rebuild.
        //
        // Verified by: DocumentNavigationServiceTests.Concurrent_Rebuild_And_Queries_Never_Transiently_Lose_Content
        NavigationSnapshot snapshot = _navigation;
        return TryGetRootKeysFromStructure(snapshot.Roots, snapshot.Structure, out rootKeys);
    }

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
            // See TryGetRootKeys for why we snapshot into a local.
            NavigationSnapshot snapshot = _navigation;
            return TryGetRootKeysFromStructure(snapshot.Roots, snapshot.Structure, out rootKeys, contentTypeKey);
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
        => TryGetChildrenKeysFromStructure(_navigation.Structure, parentKey, out childrenKeys);

    /// <summary>
    ///    Attempts to determine if a parent node has any children in the main navigation structure.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="hasChildren">
    ///     When this method returns, contains a value indicating whether the parent node has any children.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the structure; otherwise, <c>false</c>.</returns>
    public bool TryGetHasChildren(Guid parentKey, out bool hasChildren)
        => TryGetHasChildrenFromStructure(_navigation.Structure, parentKey, out hasChildren);

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
            return TryGetChildrenKeysFromStructure(_navigation.Structure, parentKey, out childrenKeys, contentTypeKey);
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
    {
        // Snapshot to a local so cache lookups, the structure walk, and the generation check
        // all see the same NavigationSnapshot instance even if a rebuild swaps it in mid-call.
        NavigationSnapshot snapshot = _navigation;
        return TryGetDescendantsKeysFromStructure(snapshot.Structure, parentKey, out descendantsKeys, contentTypeKey: null, cachingSnapshot: snapshot);
    }

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
            // Snapshot to a local so cache lookups, the structure walk, and the generation
            // check all see the same NavigationSnapshot instance even if a rebuild swaps it
            // in mid-call.
            NavigationSnapshot snapshot = _navigation;
            return TryGetDescendantsKeysFromStructure(snapshot.Structure, parentKey, out descendantsKeys, contentTypeKey, cachingSnapshot: snapshot);
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
        => TryGetAncestorsKeysFromStructure(_navigation.Structure, childKey, out ancestorsKeys);

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
            return TryGetAncestorsKeysFromStructure(_navigation.Structure, parentKey, out ancestorsKeys, contentTypeKey);
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
        => TryGetSiblingsKeysFromStructure(_navigation.Structure, key, out siblingsKeys);

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
            return TryGetSiblingsKeysFromStructure(_navigation.Structure, key, out siblingsKeys, contentTypeKey);
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
        => TryGetParentKeyFromStructure(_recycleBinNavigation.Structure, childKey, out parentKey);

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
        => TryGetChildrenKeysFromStructure(_recycleBinNavigation.Structure, parentKey, out childrenKeys);

    /// <summary>
    ///    Attempts to determine if a parent node has any children in the recycle bin navigation structure.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node in the recycle bin.</param>
    /// <param name="hasChildren">
    ///     When this method returns, contains a value indicating whether the parent node has any children.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    public bool TryGetHasChildrenInBin(Guid parentKey, out bool hasChildren)
        => TryGetHasChildrenFromStructure(_recycleBinNavigation.Structure, parentKey, out hasChildren);

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
    {
        NavigationSnapshot snapshot = _recycleBinNavigation;
        return TryGetDescendantsKeysFromStructure(snapshot.Structure, parentKey, out descendantsKeys, contentTypeKey: null, cachingSnapshot: snapshot);
    }

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
        => TryGetAncestorsKeysFromStructure(_recycleBinNavigation.Structure, childKey, out ancestorsKeys);

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
        => TryGetSiblingsKeysFromStructure(_recycleBinNavigation.Structure, key, out siblingsKeys);

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
        // Snapshot references are read once here and passed down, so every step of the move — including
        // the recursive descendant walk — acts on one coherent pair of structures. If a rebuild swaps a
        // field mid-operation, the work lands on the snapshot being replaced and is discarded with it,
        // which is what we want: the rebuild has already read the same state from the database. The
        // mutators below follow the same convention; see TryGetRootKeys for the reader side.
        NavigationSnapshot navigation = _navigation;
        NavigationSnapshot recycleBinNavigation = _recycleBinNavigation;

        if (TryRemoveNodeFromParentInStructure(navigation.Structure, key, out NavigationNode? nodeToRemove) is false || nodeToRemove is null)
        {
            return false; // Node doesn't exist
        }

        // Recursively remove all descendants and add them to recycle bin
        AddDescendantsToRecycleBinRecursively(navigation, recycleBinNavigation, nodeToRemove);

        // Reset the SortOrder based on its new position in the bin
        nodeToRemove.UpdateSortOrder(recycleBinNavigation.Structure.Count);
        var moved = recycleBinNavigation.Structure.TryAdd(nodeToRemove.Key, nodeToRemove) &&
                    navigation.Structure.TryRemove(key, out _);

        // Both snapshots' descendant lists are now potentially stale.
        navigation.Invalidate();
        recycleBinNavigation.Invalidate();

        return moved;
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
        NavigationSnapshot navigation = _navigation;

        NavigationNode? parentNode = null;
        if (parentKey.HasValue)
        {
            if (navigation.Structure.TryGetValue(parentKey.Value, out parentNode) is false)
            {
                return false; // Parent node doesn't exist
            }
        }

        // Note: sortOrder can't be automatically determined for items at root level, so it needs to be passed in
        var newNode = new NavigationNode(key, contentTypeKey, sortOrder ?? 0);
        if (navigation.Structure.TryAdd(key, newNode) is false)
        {
            return false; // Node with this key already exists
        }

        // Registered as a root only once the key is known to be new. A key rejected above is already in
        // the structure, so registering it here would report the existing node as a root regardless of
        // the parent it actually has.
        if (parentKey.HasValue is false)
        {
            navigation.Roots.Add(key);
        }

        // If sortOrder supplied → caller is asserting the position, preserve it; otherwise append last.
        parentNode?.AddChild(navigation.Structure, key, appendAsLastItem: sortOrder is null);

        navigation.Invalidate();
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
        NavigationSnapshot navigation = _navigation;

        if (navigation.Structure.TryGetValue(key, out NavigationNode? nodeToMove) is false)
        {
            return false; // Node doesn't exist
        }

        if (key == targetParentKey)
        {
            return false; // Cannot move a node to itself
        }

        NavigationNode? targetParentNode = null;
        if (targetParentKey.HasValue)
        {
            if (navigation.Structure.TryGetValue(targetParentKey.Value, out targetParentNode) is false)
            {
                return false; // Target parent doesn't exist
            }
        }

        // Updated only once the move is known to go ahead, so a node that fails the checks above keeps
        // the place it already had. One operation per destination: a node moving to root is added, and
        // one that is already a root stays a root throughout rather than being briefly removed first.
        if (targetParentNode is null)
        {
            navigation.Roots.Add(key);
        }
        else
        {
            navigation.Roots.Remove(key);
        }

        // Remove the node from its current parent's children list
        if (nodeToMove.Parent is not null && navigation.Structure.TryGetValue(nodeToMove.Parent.Value, out NavigationNode? currentParentNode))
        {
            currentParentNode.RemoveChild(navigation.Structure, key);
        }

        // Set the new parent for the node (if parent node is null - the node is moved to root)
        targetParentNode?.AddChild(navigation.Structure, key);

        navigation.Invalidate();
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
        NavigationSnapshot navigation = _navigation;

        if (navigation.Structure.TryGetValue(key, out NavigationNode? node) is false)
        {
            return false; // Node doesn't exist
        }

        node.UpdateSortOrder(newSortOrder);

        // The parent's cached ordered-children snapshot sorts by child SortOrder and is now
        // stale — invalidate so the next read rebuilds against the new value.
        if (node.Parent is not null
            && navigation.Structure.TryGetValue(node.Parent.Value, out NavigationNode? parentNode))
        {
            parentNode.InvalidateOrderedChildren();
        }

        // Descendants lists are sort-order-presorted (depth-first using each parent's
        // ordered children), so re-ordering a child re-orders any cached ancestor descendants.
        navigation.Invalidate();

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
        NavigationSnapshot recycleBinNavigation = _recycleBinNavigation;

        if (TryRemoveNodeFromParentInStructure(recycleBinNavigation.Structure, key, out NavigationNode? nodeToRemove) is false || nodeToRemove is null)
        {
            return false; // Node doesn't exist
        }

        recycleBinNavigation.Roots.Remove(key);

        RemoveDescendantsRecursively(recycleBinNavigation, nodeToRemove);

        var removed = recycleBinNavigation.Structure.TryRemove(key, out _);
        recycleBinNavigation.Invalidate();
        return removed;
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
        NavigationSnapshot navigation = _navigation;
        NavigationSnapshot recycleBinNavigation = _recycleBinNavigation;

        if (recycleBinNavigation.Structure.TryGetValue(key, out NavigationNode? nodeToRestore) is false)
        {
            return false; // Node doesn't exist
        }

        // If a target parent is specified, try to find it in the main structure
        NavigationNode? targetParentNode = null;
        if (targetParentKey.HasValue && navigation.Structure.TryGetValue(targetParentKey.Value, out targetParentNode) is false)
        {
            return false; // Target parent doesn't exist
        }

        // Set the new parent for the node (if parent node is null - the node is moved to root)
        targetParentNode?.AddChild(recycleBinNavigation.Structure, key);

        // Restore the node and its descendants from the recycle bin to the main structure
        RestoreNodeAndDescendantsRecursively(navigation, recycleBinNavigation, nodeToRestore);

        var restored = navigation.Structure.TryAdd(nodeToRestore.Key, nodeToRestore) &&
                       recycleBinNavigation.Structure.TryRemove(key, out _);

        // Both snapshots' descendant lists are now potentially stale.
        navigation.Invalidate();
        recycleBinNavigation.Invalidate();

        return restored;
    }

    /// <summary>
    ///     Rebuilds the navigation structure based on the specified object type key and whether the items are trashed.
    ///     Only relevant for items in the content and media trees (which have readLock values of -333 or -334).
    /// </summary>
    /// <param name="readLock">The read lock value, should be -333 or -334 for content and media trees.</param>
    /// <param name="objectTypeKey">The key of the object type to rebuild.</param>
    /// <param name="trashed">Indicates whether the items are in the recycle bin.</param>
    protected Task HandleRebuildAsync(int readLock, Guid objectTypeKey, bool trashed)
        => HandleRebuildAsync(readLock, [objectTypeKey], trashed);

    /// <summary>
    ///     Rebuilds the navigation structure for multiple object types.
    ///     Used when the tree contains mixed node types (e.g. elements and element containers).
    /// </summary>
    /// <param name="readLock">The lock identifier to acquire during the rebuild.</param>
    /// <param name="objectTypeKeys">The object type keys to include in the navigation structure.</param>
    /// <param name="trashed">Indicates whether the items are in the recycle bin.</param>
    protected Task HandleRebuildAsync(int readLock, IEnumerable<Guid> objectTypeKeys, bool trashed)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(readLock);

        // Build into new structures, then swap the snapshot atomically so that concurrent
        // readers never observe a transiently empty navigation state or a mismatched pair
        // of Structure and Roots.
        var newStructure = new ConcurrentDictionary<Guid, NavigationNode>();
        var newRoots = new ConcurrentHashSet<Guid>();

        if (trashed)
        {
            IEnumerable<INavigationModel> navigationModels = _navigationRepository.GetTrashedContentNodesByObjectType(objectTypeKeys);
            BuildNavigationDictionary(newStructure, newRoots, navigationModels);
            Interlocked.Exchange(ref _recycleBinNavigation, new NavigationSnapshot(newStructure, newRoots));
        }
        else
        {
            IEnumerable<INavigationModel> navigationModels = _navigationRepository.GetContentNodesByObjectType(objectTypeKeys);
            BuildNavigationDictionary(newStructure, newRoots, navigationModels);
            Interlocked.Exchange(ref _navigation, new NavigationSnapshot(newStructure, newRoots));
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

    private static bool TryGetRootKeysFromStructure(
        ConcurrentHashSet<Guid> input,
        ConcurrentDictionary<Guid, NavigationNode> structure,
        out IEnumerable<Guid> rootKeys,
        Guid? contentTypeKey = null)
    {
        var keysWithSortOrder = new List<(Guid Key, int SortOrder)>(input.Count);
        foreach (Guid key in input)
        {
            if (structure.TryGetValue(key, out NavigationNode? navigationNode) is false)
            {
                continue;
            }

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

    private static bool TryGetHasChildrenFromStructure(
        ConcurrentDictionary<Guid, NavigationNode> structure,
        Guid parentKey,
        out bool hasChildren)
    {
        if (structure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            hasChildren = false;
            return false;
        }

        // Deliberately not via GetOrderedChildren, which builds and caches a sorted array of the
        // child keys - needless work when only their existence matters.
        hasChildren = parentNode.Children.Count > 0;
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
        Guid? contentTypeKey = null,
        NavigationSnapshot? cachingSnapshot = null)
    {
        if (structure.TryGetValue(parentKey, out NavigationNode? parentNode) is false)
        {
            // Parent doesn't exist
            descendantsKeys = [];
            return false;
        }

        // Both unfiltered and content-type-filtered queries are cached, distinguished by the
        // optional contentTypeKey in the composite key. Realistic per-parent fan-out is bounded
        // by the "allowed types" model (a few types per parent), and entries are populated
        // lazily for queries that actually run — so memory tracks the templates exercised, not
        // the theoretical product of (parents × types).
        var useCache = cachingSnapshot is not null;
#pragma warning disable IDE0008 // Use explicit type (in this case using var improves the readability of the tuple key).
        var cacheKey = (parentKey, contentTypeKey);
#pragma warning restore IDE0008 // Use explicit type

        if (useCache && cachingSnapshot!.DescendantsCache.TryGetValue(cacheKey, out Guid[]? cached))
        {
            descendantsKeys = cached;
            return true;
        }

        // Capture the snapshot's mutation generation BEFORE walking. If a mutation invalidates
        // between here and the cache write, the result we computed may be stale relative to
        // the now-current Structure; we still hand it to the caller (it was correct at the
        // moment we read), but skip the cache write so future readers don't see stale data.
        var startGeneration = useCache ? cachingSnapshot!.Generation : 0;

        var descendants = new List<Guid>();
        GetDescendantsRecursively(structure, parentNode, descendants, contentTypeKey);

        if (useCache)
        {
            Guid[] result = [.. descendants];

            // Only install if no mutation happened during compute, and skip caching empty
            // results — they're cheap to recompute and caching them bloats the dictionary with
            // one entry per (parent, type) pair queried with no measurable benefit.
            if (result.Length > 0 && cachingSnapshot!.Generation == startGeneration)
            {
                cachingSnapshot.DescendantsCache[cacheKey] = result;
            }

            descendantsKeys = result;
        }
        else
        {
            descendantsKeys = descendants;
        }

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

    private static void AddDescendantsToRecycleBinRecursively(
        NavigationSnapshot navigation,
        NavigationSnapshot recycleBinNavigation,
        NavigationNode node)
    {
        recycleBinNavigation.Roots.Add(node.Key);
        navigation.Roots.Remove(node.Key);
        IReadOnlyList<Guid> childrenKeys = GetOrderedChildren(node, navigation.Structure);

        foreach (Guid childKey in childrenKeys)
        {
            if (navigation.Structure.TryGetValue(childKey, out NavigationNode? childNode) is false)
            {
                continue;
            }

            // Reset the SortOrder based on its new position in the bin
            childNode.UpdateSortOrder(recycleBinNavigation.Structure.Count);
            AddDescendantsToRecycleBinRecursively(navigation, recycleBinNavigation, childNode);

            // Only remove the child from the main structure if it was successfully added to the recycle bin
            if (recycleBinNavigation.Structure.TryAdd(childKey, childNode))
            {
                navigation.Structure.TryRemove(childKey, out _);
            }
        }
    }

    private static void RemoveDescendantsRecursively(NavigationSnapshot recycleBinNavigation, NavigationNode node)
    {
        IReadOnlyList<Guid> childrenKeys = GetOrderedChildren(node, recycleBinNavigation.Structure);
        foreach (Guid childKey in childrenKeys)
        {
            if (recycleBinNavigation.Structure.TryGetValue(childKey, out NavigationNode? childNode) is false)
            {
                continue;
            }

            RemoveDescendantsRecursively(recycleBinNavigation, childNode);
            recycleBinNavigation.Structure.TryRemove(childKey, out _);
        }
    }

    private static void RestoreNodeAndDescendantsRecursively(
        NavigationSnapshot navigation,
        NavigationSnapshot recycleBinNavigation,
        NavigationNode node)
    {
        if (node.Parent is null)
        {
            navigation.Roots.Add(node.Key);
        }

        recycleBinNavigation.Roots.Remove(node.Key);
        IReadOnlyList<Guid> childrenKeys = GetOrderedChildren(node, recycleBinNavigation.Structure);

        foreach (Guid childKey in childrenKeys)
        {
            if (recycleBinNavigation.Structure.TryGetValue(childKey, out NavigationNode? childNode) is false)
            {
                continue;
            }

            RestoreNodeAndDescendantsRecursively(navigation, recycleBinNavigation, childNode);

            // Only remove the child from the recycle bin structure if it was successfully added to the main one
            if (navigation.Structure.TryAdd(childKey, childNode))
            {
                recycleBinNavigation.Structure.TryRemove(childKey, out _);
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

        // Unfiltered case uses the cached snapshot maintained on the node — returns the same
        // sorted Guid[] across calls until the children set or a child's SortOrder changes.
        if (contentTypeKey.HasValue is false)
        {
            return node.GetOrderedChildren(structure);
        }

        // Filtered-by-content-type case stays uncached: it would need a composite (node, type)
        // key to memoise, and the call site is rare enough not to be worth it.
        var childrenWithSortOrder = new List<(Guid ChildNodeKey, int SortOrder)>(node.Children.Count);
        foreach (Guid childNodeKey in node.Children)
        {
            if (!structure.TryGetValue(childNodeKey, out NavigationNode? childNode))
            {
                continue;
            }

            if (childNode.ContentTypeKey != contentTypeKey.Value)
            {
                continue;
            }

            childrenWithSortOrder.Add((childNodeKey, childNode.SortOrder));
        }

        // Shares NavigationNode's canonical sibling ordering (SortOrder, then key tie-break) so the
        // content-type-filtered path stays in sync with the unfiltered, cached path.
        childrenWithSortOrder.Sort((a, b) =>
            NavigationNode.CompareBySortOrderThenKey(a.SortOrder, a.ChildNodeKey, b.SortOrder, b.ChildNodeKey));
        return childrenWithSortOrder.ConvertAll(childWithSortOrder => childWithSortOrder.ChildNodeKey);
    }

    private bool TryGetContentTypeKey(string contentTypeAlias, out Guid? contentTypeKey)
    {
        ConcurrentDictionary<string, Guid> aliasToKeyMap = _contentTypeAliasToKeyMap.Value;

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

    private static void BuildNavigationDictionary(ConcurrentDictionary<Guid, NavigationNode> nodesStructure, ConcurrentHashSet<Guid> roots, IEnumerable<INavigationModel> entities)
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

            // If the parent node exists in the nodesStructure, add the node to the parent's children (parent is set as well).
            // The node already carries its persisted SortOrder (set on construction above), so the child is linked without
            // reassigning it — the load order here is by path (parent-first), not sort order, and must not redefine it.
            if (nodesStructure.TryGetValue(parentKey, out NavigationNode? parentNode))
            {
                parentNode.AddChild(nodesStructure, entity.Key, appendAsLastItem: false);
            }
        }
    }

    private ConcurrentDictionary<string, Guid> LoadContentTypes()
        => new(_typeService.GetAll().Select(ct => new KeyValuePair<string, Guid>(ct.Alias, ct.Key)));
}
