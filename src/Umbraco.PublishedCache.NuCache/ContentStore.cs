using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CSharpTest.Net.Collections;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.PublishedCache.Snap;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

/// <summary>
///     Stores content in memory and persists it back to disk
/// </summary>
/// <remarks>
///     <para>
///         Methods in this class suffixed with the term "Locked" means that those methods can only be called within a
///         WriteLock. A WriteLock
///         is acquired by the GetScopedWriteLock method. Locks are not allowed to be recursive.
///     </para>
///     <para>
///         This class's logic is based on the <see cref="SnapDictionary{TKey, TValue}" /> class but has been slightly
///         modified to suit these purposes.
///     </para>
/// </remarks>
public class ContentStore
{
    // TODO: collection trigger (ok for now)
    // see SnapDictionary notes
    private const long CollectMinGenDelta = 8;
    private readonly ConcurrentDictionary<Guid, int> _contentKeyToIdMap;
    private readonly ConcurrentDictionary<int, LinkedNode<ContentNode?>> _contentNodes;
    private readonly ConcurrentDictionary<Guid, int> _contentTypeKeyToIdMap;
    private readonly ConcurrentDictionary<string, LinkedNode<IPublishedContentType?>> _contentTypesByAlias;

    // We must keep separate dictionaries for by id and by alias because we track these in snapshot/layers
    // and it is possible that the alias of a content type can be different for the same id in another layer
    // whereas the GUID -> INT cross reference can never be different
    private readonly ConcurrentDictionary<int, LinkedNode<IPublishedContentType?>> _contentTypesById;
    private readonly ConcurrentQueue<GenObj> _genObjs;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly IPublishedModelFactory _publishedModelFactory;

    // this class is an extended version of SnapDictionary
    // most of the snapshots management code, etc is an exact copy
    // SnapDictionary has unit tests to ensure it all works correctly
    // For locking information, see SnapDictionary
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly object _rlocko = new();
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly object _wlocko = new();
    private Task? _collectTask;
    private GenObj? _genObj;
    private long _liveGen;
    private long _floorGen;
    private BPlusTree<int, ContentNodeKit>? _localDb;
    private bool _nextGen;
    private bool _collectAuto;
    private LinkedNode<ContentNode> _root;
    private List<KeyValuePair<int, ContentNodeKit>>? _wchanges;

    #region Ctor

    public ContentStore(
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IVariationContextAccessor variationContextAccessor,
        ILogger logger,
        ILoggerFactory loggerFactory,
        IPublishedModelFactory publishedModelFactory,
        BPlusTree<int, ContentNodeKit>? localDb = null)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _variationContextAccessor = variationContextAccessor;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _publishedModelFactory = publishedModelFactory;
        _localDb = localDb;

        _contentNodes = new ConcurrentDictionary<int, LinkedNode<ContentNode?>>();
        _root = new LinkedNode<ContentNode>(new ContentNode(), 0);
        _contentTypesById = new ConcurrentDictionary<int, LinkedNode<IPublishedContentType?>>();
        _contentTypesByAlias =
            new ConcurrentDictionary<string, LinkedNode<IPublishedContentType?>>(StringComparer
                .InvariantCultureIgnoreCase);
        _contentTypeKeyToIdMap = new ConcurrentDictionary<Guid, int>();
        _contentKeyToIdMap = new ConcurrentDictionary<Guid, int>();

        _genObjs = new ConcurrentQueue<GenObj>();
        _genObj = null; // no initial gen exists
        _liveGen = _floorGen = 0;
        _nextGen = false; // first time, must create a snapshot
        _collectAuto = true; // collect automatically by default
    }

    #endregion

    #region Classes

    public class Snapshot : IDisposable
    {
        private readonly GenRef? _genRef;
#if DEBUG
        private readonly ILogger<Snapshot> _logger;
#endif
        private readonly ContentStore _store;
        private long _gen;

        // private static int _count;
        // private readonly int _thisCount;
        internal Snapshot(ContentStore store, GenRef genRef
#if DEBUG
            , ILogger<Snapshot> logger
#endif
        )
        {
            _store = store;
            _genRef = genRef;
            _gen = genRef.Gen;
            Interlocked.Increment(ref genRef.GenObj.Count);

            // _thisCount = _count++;
#if DEBUG
            _logger = logger;
            _logger.LogDebug("Creating snapshot.");
#endif
        }

        internal Snapshot(ContentStore store, long gen
#if DEBUG
            , ILogger<Snapshot> logger
#endif
        )
        {
            _store = store;
            _gen = gen;

#if DEBUG
            _logger = logger;
            _logger.LogDebug("Creating live.");
#endif
        }

        // this code is here just so you don't try to implement it
        // the only way we can iterate over "all" without locking the entire cache forever
        // is by shallow cloning the cache, which is quite expensive, so we should probably not do it,
        // and implement cache-level indexes
        // public IEnumerable<ContentNode> GetAll()
        // {
        //    if (_gen < 0)
        //        throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
        //    return _store.GetAll(_gen);
        // }

        public bool IsEmpty
        {
            get
            {
                if (_gen < 0)
                {
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                }

                return _store.IsEmpty(_gen);
            }
        }

        public long Gen
        {
            get
            {
                if (_gen < 0)
                {
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                }

                return _gen;
            }
        }

        public void Dispose()
        {
            if (_gen < 0)
            {
                return;
            }
#if DEBUG
            _logger.LogDebug("Dispose snapshot ({Snapshot})", _genRef?.GenObj.Count.ToString() ?? "live");
#endif
            _gen = -1;
            if (_genRef != null)
            {
                Interlocked.Decrement(ref _genRef.GenObj.Count);
            }

            GC.SuppressFinalize(this);
        }

        public ContentNode? Get(int id)
        {
            if (_gen < 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }

            return _store.Get(id, _gen);
        }

        public ContentNode? Get(Guid id)
        {
            if (_gen < 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }

            return _store.Get(id, _gen);
        }

        public IEnumerable<ContentNode> GetAtRoot()
        {
            if (_gen < 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }

            return _store.GetAtRoot(_gen).WhereNotNull();
        }

        public IEnumerable<ContentNode> GetAll()
        {
            if (_gen < 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }

            return _store.GetAll(_gen);
        }

        public IPublishedContentType? GetContentType(int id)
        {
            if (_gen < 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }

            return _store.GetContentType(id, _gen);
        }

        public IPublishedContentType? GetContentType(string alias)
        {
            if (_gen < 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }

            return _store.GetContentType(alias, _gen);
        }

        public IPublishedContentType? GetContentType(Guid key)
        {
            if (_gen < 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }

            return _store.GetContentType(key, _gen);
        }
    }

    #endregion

    #region Locking

    // see notes on SnapDictionary
    private readonly string _instanceId = Guid.NewGuid().ToString("N");

    private class WriteLockInfo
    {
#pragma warning disable IDE1006 // Naming Styles

        // This is a field that is used for ref operations
        public bool Taken;
#pragma warning restore IDE1006 // Naming Styles
    }

    // a scope contextual that represents a locked writer to the dictionary
    private class ScopedWriteLock : ScopeContextualBase
    {
        private readonly WriteLockInfo _lockinfo = new();
        private readonly ContentStore _store;
        private int _released;

        public ScopedWriteLock(ContentStore store, bool scoped)
        {
            _store = store;
            store.Lock(_lockinfo, scoped);
        }

        public override void Release(bool completed)
        {
            if (Interlocked.CompareExchange(ref _released, 1, 0) != 0)
            {
                return;
            }

            _store.Release(_lockinfo, completed);
        }
    }

    // gets a scope contextual representing a locked writer to the dictionary
    // TODO: GetScopedWriter? should the dict have a ref onto the scope provider?
    public IDisposable? GetScopedWriteLock(ICoreScopeProvider scopeProvider) =>
        ScopeContextualBase.Get(scopeProvider, _instanceId, scoped => new ScopedWriteLock(this, scoped));

    private void EnsureLocked()
    {
        if (!Monitor.IsEntered(_wlocko))
        {
            throw new InvalidOperationException("Write lock must be acquried.");
        }
    }

    private void Lock(WriteLockInfo lockInfo, bool forceGen = false)
    {
        if (Monitor.IsEntered(_wlocko))
        {
            throw new InvalidOperationException("Recursive locks not allowed");
        }

        Monitor.Enter(_wlocko, ref lockInfo.Taken);

        lock (_rlocko)
        {
            // see SnapDictionary
            try
            {
            }
            finally
            {
                if (_nextGen == false || forceGen)
                {
                    // because we are changing things, a new generation
                    // is created, which will trigger a new snapshot
                    if (_nextGen)
                    {
                        _genObjs.Enqueue(_genObj = new GenObj(_liveGen));
                    }

                    _liveGen += 1;
                    _nextGen = true;
                }
            }
        }
    }

    private void Release(WriteLockInfo lockInfo, bool commit = true)
    {
        try
        {
            if (commit == false)
            {
                lock (_rlocko)
                {
                    // see SnapDictionary
                    try
                    {
                    }
                    finally
                    {
                        _nextGen = false;
                        _liveGen -= 1;
                    }
                }

                Rollback(_contentNodes);
                RollbackRoot();
                Rollback(_contentTypesById);
                Rollback(_contentTypesByAlias);
            }
            else if (_localDb != null && _wchanges != null)
            {
                foreach (KeyValuePair<int, ContentNodeKit> change in _wchanges)
                {
                    if (change.Value.IsNull)
                    {
                        _localDb.TryRemove(change.Key, out ContentNodeKit unused);
                    }
                    else
                    {
                        _localDb[change.Key] = change.Value;
                    }
                }

                _wchanges = null;
                _localDb.Commit();
            }
        }
        finally
        {
            if (lockInfo.Taken)
            {
                Monitor.Exit(_wlocko);
            }
        }
    }

    private void RollbackRoot()
    {
        if (_root.Gen <= _liveGen)
        {
            return;
        }

        if (_root.Next != null)
        {
            _root = _root.Next;
        }
    }

    private void Rollback<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dictionary)
        where TValue : class?
        where TKey : notnull
    {
        foreach (KeyValuePair<TKey, LinkedNode<TValue>> item in dictionary)
        {
            LinkedNode<TValue>? link = item.Value;
            if (link.Gen <= _liveGen)
            {
                continue;
            }

            TKey key = item.Key;
            if (link.Next == null)
            {
                dictionary.TryRemove(key, out link);
            }
            else
            {
                dictionary.TryUpdate(key, link.Next, link);
            }
        }
    }

    #endregion

    #region LocalDb

    public void ReleaseLocalDb()
    {
        var lockInfo = new WriteLockInfo();
        try
        {
            try
            {
                // Trying to lock could throw exceptions so always make sure to clean up.
                Lock(lockInfo);
            }
            finally
            {
                try
                {
                    _localDb?.Dispose();
                }
                catch (Exception ex)
                {
                    /* TBD: May already be throwing so don't throw again */
                    _logger.LogError(ex, "Error trying to release DB");
                }
                finally
                {
                    _localDb = null;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to lock");
            throw;
        }
        finally
        {
            Release(lockInfo);
        }
    }

    private void RegisterChange(int id, ContentNodeKit kit)
    {
        if (_wchanges == null)
        {
            _wchanges = new List<KeyValuePair<int, ContentNodeKit>>();
        }

        _wchanges.Add(new KeyValuePair<int, ContentNodeKit>(id, kit));
    }

    #endregion

    #region Content types

    /// <summary>
    ///     Sets data for new content types
    /// </summary>
    /// <param name="types"></param>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public void NewContentTypesLocked(IEnumerable<IPublishedContentType> types)
    {
        EnsureLocked();

        foreach (IPublishedContentType type in types)
        {
            SetContentTypeLocked(type);
        }
    }

    /// <summary>
    ///     Sets data for updated content types
    /// </summary>
    /// <param name="types"></param>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public void UpdateContentTypesLocked(IEnumerable<IPublishedContentType> types)
    {
        // nothing to do if this is empty, no need to lock/allocate/iterate/etc...
        if (!types.Any())
        {
            return;
        }

        EnsureLocked();

        var index = types.ToDictionary(x => x.Id, x => x);

        foreach (IPublishedContentType type in index.Values)
        {
            SetContentTypeLocked(type);
        }

        foreach (LinkedNode<ContentNode?> link in _contentNodes.Values)
        {
            ContentNode? node = link.Value;
            if (node == null)
            {
                continue;
            }

            var contentTypeId = node.ContentType.Id;
            if (index.TryGetValue(contentTypeId, out IPublishedContentType? contentType) == false)
            {
                continue;
            }

            SetValueLocked(_contentNodes, node.Id, new ContentNode(node, _publishedModelFactory, contentType));
        }
    }

    /// <summary>
    ///     Updates/sets data for all content types
    /// </summary>
    /// <param name="types"></param>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public void SetAllContentTypesLocked(IEnumerable<IPublishedContentType>? types)
    {
        EnsureLocked();

        // clear all existing content types
        ClearLocked(_contentTypesById);
        ClearLocked(_contentTypesByAlias);

        if (types is not null)
        {
            // set all new content types
            foreach (IPublishedContentType type in types)
            {
                SetContentTypeLocked(type);
            }
        }

        // beware! at that point the cache is inconsistent,
        // assuming we are going to SetAll content items!
    }

    /// <summary>
    ///     Updates/sets/removes data for content types
    /// </summary>
    /// <param name="removedIds"></param>
    /// <param name="refreshedTypes"></param>
    /// <param name="kits"></param>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public void UpdateContentTypesLocked(
        IReadOnlyCollection<int>? removedIds,
        IReadOnlyCollection<IPublishedContentType> refreshedTypes,
        IReadOnlyCollection<ContentNodeKit> kits)
    {
        EnsureLocked();

        IReadOnlyCollection<int> removedIdsA = removedIds ?? Array.Empty<int>();
        IReadOnlyCollection<IPublishedContentType> refreshedTypesA =
            refreshedTypes ?? Array.Empty<IPublishedContentType>();
        var refreshedIdsA = refreshedTypesA.Select(x => x.Id).ToList();
        kits = kits ?? Array.Empty<ContentNodeKit>();

        if (kits.Count == 0 && refreshedIdsA.Count == 0 && removedIdsA.Count == 0)
        {
            return; // exit - there is nothing to do here
        }

        var removedContentTypeNodes = new List<int>();
        var refreshedContentTypeNodes = new List<int>();

        // find all the nodes that are either refreshed or removed,
        // because of their content type being either refreshed or removed
        foreach (LinkedNode<ContentNode?> link in _contentNodes.Values)
        {
            ContentNode? node = link.Value;
            if (node == null)
            {
                continue;
            }

            var contentTypeId = node.ContentType.Id;
            if (removedIdsA.Contains(contentTypeId))
            {
                removedContentTypeNodes.Add(node.Id);
            }

            if (refreshedIdsA.Contains(contentTypeId))
            {
                refreshedContentTypeNodes.Add(node.Id);
            }
        }

        // perform deletion of content with removed content type
        // removing content types should have removed their content already
        // but just to be 100% sure, clear again here
        foreach (var node in removedContentTypeNodes)
        {
            ClearBranchLocked(node);
        }

        // perform deletion of removed content types
        foreach (var id in removedIdsA)
        {
            if (_contentTypesById.TryGetValue(id, out LinkedNode<IPublishedContentType?>? link) == false ||
                link.Value == null)
            {
                continue;
            }

            SetValueLocked(_contentTypesById, id, null);
            SetValueLocked(_contentTypesByAlias, link.Value.Alias, null);
        }

        // perform update of refreshed content types
        foreach (IPublishedContentType type in refreshedTypesA)
        {
            SetContentTypeLocked(type);
        }

        // perform update of content with refreshed content type - from the kits
        // skip missing type, skip missing parents & un-buildable kits - what else could we do?
        // kits are ordered by level, so ParentExists is ok here
        var visited = new List<int>();
        foreach (ContentNodeKit kit in kits.Where(x =>
                     refreshedIdsA.Contains(x.ContentTypeId) &&
                     BuildKit(x, out _)))
        {
            // replacing the node: must preserve the relations
            ContentNode? node = GetHead(_contentNodes, kit.Node.Id)?.Value;
            if (node != null)
            {
                // Preserve children
                kit.Node.FirstChildContentId = node.FirstChildContentId;
                kit.Node.LastChildContentId = node.LastChildContentId;

                // Also preserve siblings
                kit.Node.NextSiblingContentId = node.NextSiblingContentId;
                kit.Node.PreviousSiblingContentId = node.PreviousSiblingContentId;
            }

            SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);

            visited.Add(kit.Node.Id);
            if (_localDb != null)
            {
                RegisterChange(kit.Node.Id, kit);
            }
        }

        // all content should have been refreshed - but...
        IEnumerable<int> orphans = refreshedContentTypeNodes.Except(visited);
        foreach (var id in orphans)
        {
            ClearBranchLocked(id);
        }
    }

    /// <summary>
    ///     Updates data types
    /// </summary>
    /// <param name="dataTypeIds"></param>
    /// <param name="getContentType"></param>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public void UpdateDataTypesLocked(IEnumerable<int> dataTypeIds, Func<int, IPublishedContentType?> getContentType)
    {
        EnsureLocked();

        IPublishedContentType?[] contentTypes = _contentTypesById
            .Where(kvp =>
                kvp.Value.Value != null &&
                kvp.Value.Value.PropertyTypes.Any(p => dataTypeIds.Contains(p.DataType.Id)))
            .Select(kvp => kvp.Value.Value)
            .Select(x => getContentType(x!.Id))
            .Where(x => x != null) // poof, gone, very unlikely and probably an anomaly
            .ToArray();

        var contentTypeIdsA = contentTypes.Select(x => x!.Id).ToArray();
        var contentTypeNodes = new Dictionary<int, List<int>>();
        foreach (var id in contentTypeIdsA)
        {
            contentTypeNodes[id] = new List<int>();
        }

        foreach (LinkedNode<ContentNode?> link in _contentNodes.Values)
        {
            ContentNode? node = link.Value;
            if (node != null && contentTypeIdsA.Contains(node.ContentType.Id))
            {
                contentTypeNodes[node.ContentType.Id].Add(node.Id);
            }
        }

        foreach (IPublishedContentType contentType in contentTypes.WhereNotNull())
        {
            // again, weird situation
            if (contentTypeNodes.ContainsKey(contentType.Id) == false)
            {
                continue;
            }

            foreach (var id in contentTypeNodes[contentType.Id])
            {
                _contentNodes.TryGetValue(id, out LinkedNode<ContentNode?>? link);
                if (link?.Value == null)
                {
                    continue;
                }

                var node = new ContentNode(link.Value, _publishedModelFactory, contentType);
                SetValueLocked(_contentNodes, id, node);
                if (_localDb != null)
                {
                    RegisterChange(id, node.ToKit());
                }
            }
        }
    }

    /// <summary>
    ///     Validate the <see cref="ContentNodeKit" /> and try to create a parent <see cref="LinkedNode{ContentNode}" />
    /// </summary>
    /// <param name="kit"></param>
    /// <param name="parent"></param>
    /// <returns>
    ///     Returns false if the parent was not found or if the kit validation failed
    /// </returns>
    private bool BuildKit(ContentNodeKit kit, [MaybeNullWhen(false)] out LinkedNode<ContentNode> parent)
    {
        // make sure parent exists
        parent = GetParentLink(kit.Node, null);
        if (parent == null)
        {
            _logger.LogWarning("Skip item id={kitNodeId}, could not find parent id={kitNodeParentContentId}.", kit.Node.Id, kit.Node.ParentContentId);
            return false;
        }

        // We cannot continue if there's no value. This shouldn't happen but it can happen if the database umbracoNode.path
        // data is invalid/corrupt. If that is the case, the parentId might be ok but not the Path which can result in null
        // because the data sort operation is by path.
        if (parent.Value == null)
        {
            _logger.LogWarning(
                "Skip item id={kitNodeId}, no Data assigned for linked node with path {kitNodePath} and parent id {kitNodeParentContentId}. This can indicate data corruption for the Path value for node {kitNodeId}. See the Health Check dashboard in Settings to resolve data integrity issues.",
                kit.Node.Id,
                kit.Node.Path,
                kit.Node.ParentContentId,
                kit.Node.Id);
            return false;
        }

        // make sure the kit is valid
        if (kit.DraftData == null && kit.PublishedData == null)
        {
            _logger.LogWarning("Skip item id={kitNodeId}, both draft and published data are null.", kit.Node.Id);
            return false;
        }

        // unknown = bad
        if (_contentTypesById.TryGetValue(kit.ContentTypeId, out LinkedNode<IPublishedContentType?>? link) == false ||
            link.Value == null)
        {
            _logger.LogWarning("Skip item id={kitNodeId}, could not find content type id={kitContentTypeId}.", kit.Node.Id, kit.ContentTypeId);
            return false;
        }

        // check whether parent is published
        var canBePublished = ParentPublishedLocked(kit);

        // and use
        kit.Build(link.Value, _publishedSnapshotAccessor, _variationContextAccessor, _publishedModelFactory, canBePublished);

        return true;
    }

    #endregion

    #region Set, Clear, Get

    public int Count => _contentNodes.Count;

    /// <summary>
    ///     Get the most recent version of the LinkedNode stored in the dictionary for the supplied key
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static LinkedNode<TValue>? GetHead<TKey, TValue>(
        ConcurrentDictionary<TKey, LinkedNode<TValue>> dict,
        TKey key)
        where TValue : class?
        where TKey : notnull
    {
        dict.TryGetValue(key, out LinkedNode<TValue>? link); // else null
        return link;
    }

    /// <summary>
    ///     Sets the data for a <see cref="ContentNodeKit" />
    /// </summary>
    /// <param name="kit"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public bool SetLocked(ContentNodeKit kit)
    {
        EnsureLocked();

        // ReSharper disable LocalizableElement
        if (kit.IsEmpty)
        {
            throw new ArgumentException("Kit is empty.", nameof(kit));
        }

        if (kit.Node.FirstChildContentId > 0)
        {
            throw new ArgumentException("Kit content cannot have children.", nameof(kit));
        }

        // ReSharper restore LocalizableElement
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Set content ID: {KitNodeId}", kit.Node.Id);
        }

        // get existing
        _contentNodes.TryGetValue(kit.Node.Id, out LinkedNode<ContentNode?>? link);
        ContentNode? existing = link?.Value;

        if (!BuildKit(kit, out LinkedNode<ContentNode>? parent))
        {
            return false;
        }

        // moving?
        var moving = existing != null && existing.ParentContentId != kit.Node.ParentContentId;

        // manage children
        if (existing != null)
        {
            kit.Node.FirstChildContentId = existing.FirstChildContentId;
            kit.Node.LastChildContentId = existing.LastChildContentId;
        }

        // set
        SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
        if (_localDb != null)
        {
            RegisterChange(kit.Node.Id, kit);
        }

        // manage the tree
        if (existing == null)
        {
            // new, add to parent
            AddTreeNodeLocked(kit.Node, parent);
        }
        else if (moving || existing.SortOrder != kit.Node.SortOrder)
        {
            // moved, remove existing from its parent, add content to its parent
            RemoveTreeNodeLocked(existing);
            AddTreeNodeLocked(kit.Node);
        }
        else
        {
            // replacing existing, handle siblings
            kit.Node.NextSiblingContentId = existing.NextSiblingContentId;
            kit.Node.PreviousSiblingContentId = existing.PreviousSiblingContentId;
        }

        _contentKeyToIdMap[kit.Node.Uid] = kit.Node.Id;

        return true;
    }

    private void ClearRootLocked()
    {
        if (_root.Gen != _liveGen)
        {
            _root = new LinkedNode<ContentNode>(new ContentNode(), _liveGen, _root);
        }
        else if (_root.Value is not null)
        {
            _root.Value.FirstChildContentId = -1;
        }
    }

    /// <summary>
    ///     Builds all kits on startup using a fast forward only cursor
    /// </summary>
    /// <param name="kits">
    ///     All kits sorted by Level + Parent Id + Sort order
    /// </param>
    /// <param name="fromDb">True if the data is coming from the database (not the local cache db)</param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         This requires that the collection is sorted by Level + ParentId + Sort Order.
    ///         This should be used only on a site startup as the first generations.
    ///         This CANNOT be used after startup since it bypasses all checks for Generations.
    ///     </para>
    ///     <para>
    ///         This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///         otherwise an exception will occur.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    [Obsolete("Use the overload that takes a 'kitGroupSize' parameter instead")]
    public bool SetAllFastSortedLocked(IEnumerable<ContentNodeKit> kits, bool fromDb) =>
        SetAllFastSortedLocked(kits, 1, fromDb);

    /// <summary>
    ///     Builds all kits on startup using a fast forward only cursor
    /// </summary>
    /// <param name="kits">
    ///     All kits sorted by Level + Parent Id + Sort order
    /// </param>
    /// <param name="kitGroupSize"></param>
    /// <param name="fromDb">True if the data is coming from the database (not the local cache db)</param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         This requires that the collection is sorted by Level + ParentId + Sort Order.
    ///         This should be used only on a site startup as the first generations.
    ///         This CANNOT be used after startup since it bypasses all checks for Generations.
    ///     </para>
    ///     <para>
    ///         This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///         otherwise an exception will occur.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public bool SetAllFastSortedLocked(IEnumerable<ContentNodeKit> kits, int kitGroupSize, bool fromDb)
    {
        EnsureLocked();

        var ok = true;

        ClearLocked(_contentNodes);
        ClearRootLocked();

        // The name of the game here is to populate each kit's
        //  FirstChildContentId
        //  LastChildContentId
        //  NextSiblingContentId
        //  PreviousSiblingContentId
        ContentNode? previousNode = null;
        ContentNode? parent = null;

        // By using InGroupsOf() here we are forcing the database query result extraction to retrieve items in batches,
        // reducing the possibility of a database timeout (ThreadAbortException) on large datasets.
        // This in turn reduces the possibility that the NuCache file will remain locked, because an exception
        // here results in the calling method to not release the lock.

        // However the larger the batck size, the more content loaded into memory.  So by default, this is set to 1 and can be increased by setting
        // the configuration setting Umbraco:CMS:NuCache:KitPageSize to a higher value.

        // If we are not loading from the database, then we can ignore this restriction.
        foreach (IEnumerable<ContentNodeKit> kitGroup in
                 kits.InGroupsOf(!fromDb || kitGroupSize < 1 ? 1 : kitGroupSize))
        {
            foreach (ContentNodeKit kit in kitGroup)
            {
                if (!BuildKit(kit, out LinkedNode<ContentNode>? parentLink))
                {
                    ok = false;
                    continue; // skip that one
                }

                ContentNode thisNode = kit.Node;

                if (parent == null)
                {
                    // first parent
                    parent = parentLink.Value;
                    parent!.FirstChildContentId = thisNode.Id; // this node is the first node
                }
                else if (parent.Id != parentLink.Value!.Id)
                {
                    // new parent
                    parent = parentLink.Value;
                    parent.FirstChildContentId = thisNode.Id; // this node is the first node
                    previousNode = null; // there is no previous sibling
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Set {thisNodeId} with parent {thisNodeParentContentId}", thisNode.Id, thisNode.ParentContentId);
                }

                SetValueLocked(_contentNodes, thisNode.Id, thisNode);

                // if we are initializing from the database source ensure the local db is updated
                if (fromDb && _localDb != null)
                {
                    RegisterChange(thisNode.Id, kit);
                }

                // this node is always the last child
                parent.LastChildContentId = thisNode.Id;

                // wire previous node as previous sibling
                if (previousNode != null)
                {
                    previousNode.NextSiblingContentId = thisNode.Id;
                    thisNode.PreviousSiblingContentId = previousNode.Id;
                }

                // this node becomes the previous node
                previousNode = thisNode;

                _contentKeyToIdMap[kit.Node.Uid] = kit.Node.Id;
            }
        }

        return ok;
    }

    /// <summary>
    ///     Set all data for a collection of <see cref="ContentNodeKit" />
    /// </summary>
    /// <param name="kits"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    [Obsolete("Use the overload that takes the 'kitGroupSize' and 'fromDb' parameters instead")]
    public bool SetAllLocked(IEnumerable<ContentNodeKit> kits) => SetAllLocked(kits, 1, false);

    /// <summary>
    ///     Set all data for a collection of <see cref="ContentNodeKit" />
    /// </summary>
    /// <param name="kits"></param>
    /// <param name="kitGroupSize"></param>
    /// <param name="fromDb">True if the data is coming from the database (not the local cache db)</param>
    /// <returns></returns>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public bool SetAllLocked(IEnumerable<ContentNodeKit> kits, int kitGroupSize, bool fromDb)
    {
        EnsureLocked();

        var ok = true;

        ClearLocked(_contentNodes);
        ClearRootLocked();

        // do NOT clear types else they are gone!
        // ClearLocked(_contentTypesById);
        // ClearLocked(_contentTypesByAlias);

        // By using InGroupsOf() here we are forcing the database query result extraction to retrieve items in batches,
        // reducing the possibility of a database timeout (ThreadAbortException) on large datasets.
        // This in turn reduces the possibility that the NuCache file will remain locked, because an exception
        // here results in the calling method to not release the lock.

        // However the larger the batck size, the more content loaded into memory.  So by default, this is set to 1 and can be increased by setting
        // the configuration setting Umbraco:CMS:NuCache:KitPageSize to a higher value.

        // If we are not loading from the database, then we can ignore this restriction.
        foreach (IEnumerable<ContentNodeKit> kitGroup in
                 kits.InGroupsOf(!fromDb || kitGroupSize < 1 ? 1 : kitGroupSize))
        {
            foreach (ContentNodeKit kit in kitGroup)
            {
                if (!BuildKit(kit, out LinkedNode<ContentNode>? parent))
                {
                    ok = false;
                    continue; // skip that one
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Set {kitNodeId} with parent {kitNodeParentContentId}", kit.Node.Id, kit.Node.ParentContentId);
                }

                SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);

                if (_localDb != null)
                {
                    RegisterChange(kit.Node.Id, kit);
                }

                AddTreeNodeLocked(kit.Node, parent);

                _contentKeyToIdMap[kit.Node.Uid] = kit.Node.Id;
            }
        }

        return ok;
    }

    /// <summary>
    ///     Sets data for a branch of <see cref="ContentNodeKit" />
    /// </summary>
    /// <param name="rootContentId"></param>
    /// <param name="kits"></param>
    /// <returns></returns>
    /// <remarks>
    ///     <para>
    ///         IMPORTANT kits must be sorted out by LEVEL and by SORT ORDER
    ///     </para>
    ///     <para>
    ///         This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///         otherwise an exception will occur.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public bool SetBranchLocked(int rootContentId, IEnumerable<ContentNodeKit> kits)
    {
        EnsureLocked();

        var ok = true;

        // get existing
        _contentNodes.TryGetValue(rootContentId, out LinkedNode<ContentNode?>? link);
        ContentNode? existing = link?.Value;

        // clear
        if (existing != null)
        {
            // this zero's out the branch (recursively), if we're in a new gen this will add a NULL placeholder for the gen
            ClearBranchLocked(existing);

            // TODO: This removes the current GEN from the tree - do we really want to do that? (not sure if this is still an issue....)
            RemoveTreeNodeLocked(existing);
        }

        // now add them all back
        foreach (ContentNodeKit kit in kits)
        {
            if (!BuildKit(kit, out LinkedNode<ContentNode>? parent))
            {
                ok = false;
                continue; // skip that one
            }

            SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
            if (_localDb != null)
            {
                RegisterChange(kit.Node.Id, kit);
            }

            AddTreeNodeLocked(kit.Node, parent);

            _contentKeyToIdMap[kit.Node.Uid] = kit.Node.Id;
        }

        return ok;
    }

    /// <summary>
    ///     Clears data for a given node id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This methods MUST be called from within a write lock, normally wrapped within GetScopedWriteLock
    ///     otherwise an exception will occur.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if this method is not called within a write lock
    /// </exception>
    public bool ClearLocked(int id)
    {
        EnsureLocked();

        // try to find the content
        // if it is not there, nothing to do
        _contentNodes.TryGetValue(id, out LinkedNode<ContentNode?>? link); // else null
        if (link?.Value == null)
        {
            return false;
        }

        ContentNode? content = link.Value;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Clear content ID: {ContentId}", content.Id);
        }

        // clear the entire branch
        ClearBranchLocked(content);

        // manage the tree
        RemoveTreeNodeLocked(content);

        return true;
    }

    private void ClearBranchLocked(int id)
    {
        _contentNodes.TryGetValue(id, out LinkedNode<ContentNode?>? link);
        if (link?.Value == null)
        {
            return;
        }

        ClearBranchLocked(link.Value);
    }

    private void ClearBranchLocked(ContentNode? content)
    {
        // This should never be null, all code that calls this method is null checking but we've seen
        // issues of null ref exceptions in issue reports so we'll double check here
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        SetValueLocked(_contentNodes, content.Id, null);
        if (_localDb != null)
        {
            RegisterChange(content.Id, ContentNodeKit.Null);
        }

        _contentKeyToIdMap.TryRemove(content.Uid, out _);

        var id = content.FirstChildContentId;
        while (id > 0)
        {
            // get the required link node, this ensures that both `link` and `link.Value` are not null
            LinkedNode<ContentNode> link = GetRequiredLinkedNode(id, "child", null);
            ContentNode? linkValue = link.Value; // capture local since clearing in recurse can clear it
            ClearBranchLocked(linkValue); // recurse
            id = linkValue?.NextSiblingContentId ?? 0;
        }
    }

    /// <summary>
    ///     Gets the link node and if it doesn't exist throw a <see cref="PanicException" />
    /// </summary>
    /// <param name="id"></param>
    /// <param name="description"></param>
    /// <param name="gen">the generation requested, null for the latest stored</param>
    /// <returns></returns>
    private LinkedNode<ContentNode> GetRequiredLinkedNode(int id, string description, long? gen)
    {
        if (_contentNodes.TryGetValue(id, out LinkedNode<ContentNode?>? link))
        {
            link = GetLinkedNodeGen(link, gen);
            if (link is not null && link.Value is not null)
            {
                return link!;
            }
        }

        throw new PanicException($"failed to get {description} with id={id}");
    }

    /// <summary>
    ///     Gets the parent link node, may be null or root if ParentContentId is less than 0
    /// </summary>
    /// <param name="gen">the generation requested, null for the latest stored</param>
    /// <param name="content">The content node</param>
    private LinkedNode<ContentNode>? GetParentLink(ContentNode content, long? gen)
    {
        if (content.ParentContentId < 0)
        {
            LinkedNode<ContentNode>? root = GetLinkedNodeGen(_root, gen);
            return root;
        }

        if (_contentNodes.TryGetValue(content.ParentContentId, out LinkedNode<ContentNode?>? link))
        {
            link = GetLinkedNodeGen(link, gen);
        }

        if (link is not null && link.Value is not null)
        {
            return link!;
        }

        return null;
    }

    /// <summary>
    ///     Gets the linked parent node and if it doesn't exist throw a <see cref="PanicException" />
    /// </summary>
    /// <param name="content">The content node</param>
    /// <param name="gen">the generation requested, null for the latest stored</param>
    /// <returns></returns>
    private LinkedNode<ContentNode> GetRequiredParentLink(ContentNode content, long? gen) => content.ParentContentId < 0
        ? _root
        : GetRequiredLinkedNode(content.ParentContentId, "parent", gen);

    /// <summary>
    ///     Iterates over the LinkedNode's generations to find the correct one
    /// </summary>
    /// <param name="link"></param>
    /// <param name="gen">The generation requested, use null to avoid the lookup</param>
    /// <returns></returns>
    private LinkedNode<TValue>? GetLinkedNodeGen<TValue>(LinkedNode<TValue>? link, long? gen)
        where TValue : class?
    {
        if (!gen.HasValue)
        {
            return link;
        }

        // find the correct snapshot, find the first that is <= the requested gen
        while (link != null && link.Gen > gen)
        {
            link = link.Next;
        }

        return link;
    }

    /// <summary>
    ///     This removes this current node from the tree hiearchy by removing it from it's parent's linked list
    /// </summary>
    /// <param name="content"></param>
    /// <remarks>
    ///     This is called within a lock which means a new Gen is being created therefore this will not modify any existing
    ///     content in a Gen.
    /// </remarks>
    private void RemoveTreeNodeLocked(ContentNode content)
    {
        // NOTE: DO NOT modify `content` here, this would modify data for an existing Gen, all modifications are done to clones
        // which would be targeting the new Gen.
        LinkedNode<ContentNode> parentLink = content.ParentContentId < 0
            ? _root
            : GetRequiredLinkedNode(content.ParentContentId, "parent", null);

        ContentNode? parent = parentLink.Value;

        // must have children
        if (parent!.FirstChildContentId < 0)
        {
            throw new PanicException("no children");
        }

        // if first/last, clone parent, then remove
        if (parent.FirstChildContentId == content.Id || parent.LastChildContentId == content.Id)
        {
            parent = GenCloneLocked(parentLink);
        }

        if (parent is not null)
        {
            if (parent.FirstChildContentId == content.Id)
            {
                parent.FirstChildContentId = content.NextSiblingContentId;
            }

            if (parent.LastChildContentId == content.Id)
            {
                parent.LastChildContentId = content.PreviousSiblingContentId;
            }
        }

        // maintain linked list
        if (content.NextSiblingContentId > 0)
        {
            LinkedNode<ContentNode> nextLink =
                GetRequiredLinkedNode(content.NextSiblingContentId, "next sibling", null);
            ContentNode? next = GenCloneLocked(nextLink);

            if (next is not null)
            {
                next.PreviousSiblingContentId = content.PreviousSiblingContentId;
            }
        }

        if (content.PreviousSiblingContentId > 0)
        {
            LinkedNode<ContentNode> prevLink =
                GetRequiredLinkedNode(content.PreviousSiblingContentId, "previous sibling", null);
            ContentNode? prev = GenCloneLocked(prevLink);

            if (prev is not null)
            {
                prev.NextSiblingContentId = content.NextSiblingContentId;
            }
        }
    }

    private bool ParentPublishedLocked(ContentNodeKit kit)
    {
        if (kit.Node.ParentContentId < 0)
        {
            return true;
        }

        LinkedNode<ContentNode>? link = GetParentLink(kit.Node, null);
        ContentNode? node = link?.Value;
        return node != null && node.HasPublished;
    }

    private ContentNode? GenCloneLocked(LinkedNode<ContentNode> link)
    {
        ContentNode? node = link.Value;

        if (node != null && link.Gen != _liveGen)
        {
            node = new ContentNode(node, _publishedModelFactory);
            if (link == _root)
            {
                SetRootLocked(node);
            }
            else
            {
                SetValueLocked(_contentNodes, node.Id, node);
            }
        }

        return node;
    }

    /// <summary>
    ///     Adds a node to the tree structure.
    /// </summary>
    private void AddTreeNodeLocked(ContentNode content, LinkedNode<ContentNode>? parentLink = null)
    {
        parentLink = parentLink ?? GetRequiredParentLink(content, null);

        ContentNode? parent = parentLink.Value;

        // We are doing a null check here but this should no longer be possible because we have a null check in BuildKit
        // for the parent.Value property and we'll output a warning. However I'll leave this additional null check in place.
        // see https://github.com/umbraco/Umbraco-CMS/issues/7868
        if (parent == null)
        {
            throw new PanicException(
                $"A null Value was returned on the {nameof(parentLink)} LinkedNode with id={content.ParentContentId}, potentially your database paths are corrupted.");
        }

        // if parent has no children, clone parent + add as first child
        if (parent.FirstChildContentId < 0)
        {
            parent = GenCloneLocked(parentLink);
            if (parent is not null)
            {
                parent.FirstChildContentId = content.Id;
                parent.LastChildContentId = content.Id;
            }

            return;
        }

        // get parent's first child
        LinkedNode<ContentNode> childLink = GetRequiredLinkedNode(parent.FirstChildContentId, "first child", null);
        ContentNode? child = childLink.Value;

        // if first, clone parent + insert as first child
        // NOTE: Don't perform this check if loading from local DB since we know it's already sorted
        if (child?.SortOrder > content.SortOrder)
        {
            content.NextSiblingContentId = parent.FirstChildContentId;
            content.PreviousSiblingContentId = -1;

            parent = GenCloneLocked(parentLink);
            if (parent is not null)
            {
                parent.FirstChildContentId = content.Id;
            }

            child = GenCloneLocked(childLink);

            if (child is not null)
            {
                child.PreviousSiblingContentId = content.Id;
            }

            return;
        }

        // get parent's last child
        LinkedNode<ContentNode> lastChildLink = GetRequiredLinkedNode(parent.LastChildContentId, "last child", null);
        ContentNode? lastChild = lastChildLink.Value;

        // if last, clone parent + append as last child
        if (lastChild?.SortOrder <= content.SortOrder)
        {
            content.PreviousSiblingContentId = parent.LastChildContentId;
            content.NextSiblingContentId = -1;

            parent = GenCloneLocked(parentLink);
            if (parent is not null)
            {
                parent.LastChildContentId = content.Id;
            }

            lastChild = GenCloneLocked(lastChildLink);

            if (lastChild is not null)
            {
                lastChild.NextSiblingContentId = content.Id;
            }

            return;
        }

        // else it's going somewhere in the middle,
        // TODO: There was a note about performance when this occurs and that this only happens when moving and not very often, but that is not true,
        // this also happens anytime a middle node is unpublished or republished (which causes a branch update), i'm unsure if this has perf impacts,
        // i think this used to but it doesn't seem bad anymore that I can see...
        while (child?.NextSiblingContentId > 0)
        {
            // get next child
            LinkedNode<ContentNode> nextChildLink =
                GetRequiredLinkedNode(child.NextSiblingContentId, "next child", null);
            ContentNode? nextChild = nextChildLink.Value;

            // if here, clone previous + append/insert
            // NOTE: Don't perform this check if loading from local DB since we know it's already sorted
            if (nextChild?.SortOrder > content.SortOrder)
            {
                content.NextSiblingContentId = nextChild.Id;
                content.PreviousSiblingContentId = nextChild.PreviousSiblingContentId;

                child = GenCloneLocked(childLink);

                if (child is not null)
                {
                    child.NextSiblingContentId = content.Id;
                }

                ContentNode? nnext = GenCloneLocked(nextChildLink);

                if (nnext is not null)
                {
                    nnext.PreviousSiblingContentId = content.Id;
                }

                return;
            }

            childLink = nextChildLink;
            child = nextChild;
        }

        // should never get here
        throw new PanicException("No more children.");
    }

    // replaces the root node
    private void SetRootLocked(ContentNode node)
    {
        if (_root.Gen != _liveGen)
        {
            _root = new LinkedNode<ContentNode>(node, _liveGen, _root);
        }
        else
        {
            _root.Value = node;
        }
    }

    private void SetContentTypeLocked(IPublishedContentType type)
    {
        SetValueLocked(_contentTypesById, type.Id, type);
        SetValueLocked(_contentTypesByAlias, type.Alias, type);

        // ensure the key/id map is accurate
        _contentTypeKeyToIdMap[type.Key] = type.Id;
    }

    // set a node (just the node, not the tree)
    private void SetValueLocked<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict, TKey key, TValue value)
        where TValue : class?
        where TKey : notnull
    {
        // this is safe only because we're write-locked
        LinkedNode<TValue>? link = GetHead(dict, key);
        if (link != null)
        {
            // already in the dict
            if (link.Gen != _liveGen)
            {
                // for an older gen - if value is different then insert a new
                // link for the new gen, with the new value
                if (link.Value != value)
                {
                    dict.TryUpdate(key, new LinkedNode<TValue>(value, _liveGen, link), link);
                }
            }
            else
            {
                // for the live gen - we can fix the live gen - and remove it
                // if value is null and there's no next gen
                if (value == null && link.Next == null)
                {
                    dict.TryRemove(key, out link);
                }
                else
                {
                    link.Value = value;
                }
            }
        }
        else
        {
            dict.TryAdd(key, new LinkedNode<TValue>(value, _liveGen));
        }
    }

    private void ClearLocked<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict)
        where TValue : class?
        where TKey : notnull
    {
        // this is safe only because we're write-locked
        foreach (KeyValuePair<TKey, LinkedNode<TValue>> kvp in dict.Where(x => x.Value != null))
        {
            if (kvp.Value.Gen != _liveGen)
            {
                var link = new LinkedNode<TValue>(null, _liveGen, kvp.Value);
                dict.TryUpdate(kvp.Key, link, kvp.Value);
            }
            else
            {
                kvp.Value.Value = null;
            }
        }
    }

    public ContentNode? Get(int id, long gen) => GetValue(_contentNodes, id, gen);

    public ContentNode? Get(Guid uid, long gen) =>
        _contentKeyToIdMap.TryGetValue(uid, out var id)
            ? GetValue(_contentNodes, id, gen)
            : null;

    public IEnumerable<ContentNode?> GetAtRoot(long gen)
    {
        LinkedNode<ContentNode>? root = GetLinkedNodeGen(_root, gen);
        if (root == null)
        {
            yield break;
        }

        var id = root.Value?.FirstChildContentId;

        while (id > 0)
        {
            LinkedNode<ContentNode> link = GetRequiredLinkedNode(id.Value, "root", gen);
            yield return link.Value;
            id = link.Value?.NextSiblingContentId;
        }
    }

    private TValue? GetValue<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict, TKey key, long gen)
        where TValue : class?
        where TKey : notnull
    {
        // look ma, no lock!
        LinkedNode<TValue>? link = GetHead(dict, key);
        link = GetLinkedNodeGen(link, gen);
        return link?.Value; // may be null
    }

    public IEnumerable<ContentNode> GetAll(long gen)
    {
        // enumerating on .Values locks the concurrent dictionary,
        // so better get a shallow clone in an array and release
        LinkedNode<ContentNode?>[] links = _contentNodes.Values.ToArray();
        foreach (LinkedNode<ContentNode?> l in links)
        {
            LinkedNode<ContentNode?>? link = GetLinkedNodeGen(l, gen);
            if (link?.Value != null)
            {
                yield return link.Value;
            }
        }
    }

    public bool IsEmpty(long gen)
    {
        var has = _contentNodes.Any(x =>
        {
            LinkedNode<ContentNode?>? link = GetLinkedNodeGen(x.Value, gen);
            return link?.Value != null;
        });
        return has == false;
    }

    public IPublishedContentType? GetContentType(int id, long gen) => GetValue(_contentTypesById, id, gen);

    public IPublishedContentType? GetContentType(string alias, long gen) => GetValue(_contentTypesByAlias, alias, gen);

    public IPublishedContentType? GetContentType(Guid key, long gen)
    {
        if (!_contentTypeKeyToIdMap.TryGetValue(key, out var id))
        {
            return null;
        }

        return GetContentType(id, gen);
    }

    #endregion

    #region Snapshots

    public Snapshot CreateSnapshot()
    {
        lock (_rlocko)
        {
            // if no next generation is required, and we already have one,
            // use it and create a new snapshot
            if (_nextGen == false && _genObj != null)
            {
                return new Snapshot(this, _genObj.GetGenRef()
#if DEBUG
                    , _loggerFactory.CreateLogger<Snapshot>()
#endif
                );
            }

            // else we need to try to create a new gen ref
            // whether we are wlocked or not, noone can rlock while we do,
            // so _liveGen and _nextGen are safe
            if (Monitor.IsEntered(_wlocko))
            {
                // write-locked, cannot use latest gen (at least 1) so use previous
                var snapGen = _nextGen ? _liveGen - 1 : _liveGen;

                // create a new gen ref unless we already have it
                if (_genObj == null)
                {
                    _genObjs.Enqueue(_genObj = new GenObj(snapGen));
                }
                else if (_genObj.Gen != snapGen)
                {
                    throw new PanicException(
                        $"The generation {_genObj.Gen} does not equal the snapshot generation {snapGen}");
                }
            }
            else
            {
                // not write-locked, can use latest gen, create a new gen ref
                _genObjs.Enqueue(_genObj = new GenObj(_liveGen));
                _nextGen = false; // this is the ONLY thing that triggers a _liveGen++
            }

            // so...
            // the genRefRef has a weak ref to the genRef, and is queued
            // the snapshot has a ref to the genRef, which has a ref to the genRefRef
            // when the snapshot is disposed, it decreases genRefRef counter
            // so after a while, one of these conditions is going to be true:
            // - the genRefRef counter is zero because all snapshots have properly been disposed
            // - the genRefRef weak ref is dead because all snapshots have been collected
            // in both cases, we will dequeue and collect
            var snapshot = new Snapshot(this, _genObj.GetGenRef()
#if DEBUG
                , _loggerFactory.CreateLogger<Snapshot>()
#endif
            );

            // reading _floorGen is safe if _collectTask is null
            if (_collectTask == null && _collectAuto && _liveGen - _floorGen > CollectMinGenDelta)
            {
                CollectAsyncLocked();
            }

            return snapshot;
        }
    }

    public Snapshot LiveSnapshot => new(this, _liveGen
#if DEBUG
        , _loggerFactory.CreateLogger<Snapshot>()
#endif
    );

    public Task CollectAsync()
    {
        lock (_rlocko)
        {
            return CollectAsyncLocked();
        }
    }

    private Task CollectAsyncLocked()
    {
        // NOTE: What in the heck is going on here? Why is any of this running in async contexts?
        // SD: From what I can tell this was designed to be a set and forget background task to do the
        // collecting which is why it's called from non-async methods within this class. This is
        // slightly dangerous because it's not taking into account app shutdown.
        // TODO: There should be a different method or class responsible for executing the cleanup on a
        // background (set and forget) thread.
        if (_collectTask != null)
        {
            return _collectTask;
        }

        // ReSharper disable InconsistentlySynchronizedField
        Task task = _collectTask = Task.Run(Collect);
        _collectTask.ContinueWith(
            _ =>
            {
                lock (_rlocko)
                {
                    _collectTask = null;
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,

            // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
            TaskScheduler.Default);

        // ReSharper restore InconsistentlySynchronizedField
        return task;
    }

    private void Collect()
    {
        // see notes in CreateSnapshot
#if DEBUG
        _logger.LogDebug("Collect.");
#endif
        while (_genObjs.TryPeek(out GenObj? genObj) && (genObj.Count == 0 || genObj.WeakGenRef.IsAlive == false))
        {
            _genObjs.TryDequeue(out genObj); // cannot fail since TryPeek has succeeded
            _floorGen = genObj!.Gen;
#if DEBUG
            // _logger.LogDebug("_floorGen=" + _floorGen + ", _liveGen=" + _liveGen);
#endif
        }

        Collect(_contentNodes);
        CollectRoot();
        Collect(_contentTypesById);
        Collect(_contentTypesByAlias);
    }

    private void CollectRoot()
    {
        LinkedNode<ContentNode>? link = _root;
        while (link.Next != null && link.Next.Gen > _floorGen)
        {
            link = link.Next;
        }

        link.Next = null;
    }

    private void Collect<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict)
        where TValue : class?
        where TKey : notnull
    {
        // it is OK to enumerate a concurrent dictionary and it does not lock
        // it - and here it's not an issue if we skip some items, they will be
        // processed next time we collect
        long liveGen;

        // r is good
        lock (_rlocko)
        {
            liveGen = _liveGen;
            if (_nextGen == false)
            {
                liveGen += 1;
            }
        }

        foreach (KeyValuePair<TKey, LinkedNode<TValue>> kvp in dict)
        {
            LinkedNode<TValue>? link = kvp.Value;

#if DEBUG
            // _logger.LogDebug("Collect id:" + kvp.Key + ", gen:" + link.Gen +
            //    ", nxt:" + (link.Next == null ? "null" : "link") +
            //    ", val:" + (link.Value == null ? "null" : "value"));
#endif

            // reasons to collect the head:
            //   gen must be < liveGen (we never collect live gen)
            //   next == null && value == null (we have no data at all)
            //   next != null && value == null BUT gen > floor (noone wants us)
            // not live means .Next and .Value are safe
            if (link.Gen < liveGen && link.Value == null
                                   && (link.Next == null || link.Gen <= _floorGen))
            {
                // not live, null value, no next link = remove that one -- but only if
                // the dict has not been updated, have to do it via ICollection<> (thanks
                // Mr Toub) -- and if the dict has been updated there is nothing to collect
                var idict = dict as ICollection<KeyValuePair<TKey, LinkedNode<TValue>>>;
                idict.Remove(kvp);
                continue;
            }

            // in any other case we're not collecting the head, we need to go to Next
            // and if there is no Next, skip
            if (link.Next == null)
            {
                continue;
            }

            // else go to Next and loop while above floor, and kill everything below
            while (link.Next != null && link.Next.Gen > _floorGen)
            {
                link = link.Next;
            }

            link.Next = null;
        }
    }

    // TODO: This is never used? Should it be? Maybe move to TestHelper below?
    // public async Task WaitForPendingCollect()
    // {
    //    Task task;
    //    lock (_rlocko)
    //    {
    //        task = _collectTask;
    //    }
    //    if (task != null)
    //        await task;
    // }
    public long GenCount => _genObjs.Count;

    public long SnapCount => _genObjs.Sum(x => x.Count);

    #endregion

    #region Internals/Unit testing

    private TestHelper? _unitTesting;

    // note: nothing here is thread-safe
    internal class TestHelper
    {
        private readonly ContentStore _store;

        public TestHelper(ContentStore store) => _store = store;

        public long LiveGen => _store._liveGen;

        public long FloorGen => _store._floorGen;

        public bool NextGen => _store._nextGen;

        public bool CollectAuto
        {
            get => _store._collectAuto;
            set => _store._collectAuto = value;
        }

        /// <summary>
        ///     Return a list of Gen/ContentNode values
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public (long gen, ContentNode? contentNode)[] GetValues(int id)
        {
            _store._contentNodes.TryGetValue(id, out LinkedNode<ContentNode?>? link); // else null

            if (link == null)
            {
                return Array.Empty<(long, ContentNode?)>();
            }

            var tuples = new List<(long, ContentNode?)>();
            do
            {
                tuples.Add((link.Gen, link.Value));
                link = link.Next;
            }
            while (link != null);

            return tuples.ToArray();
        }
    }

    internal TestHelper Test => _unitTesting ?? (_unitTesting = new TestHelper(this));

    #endregion
}
