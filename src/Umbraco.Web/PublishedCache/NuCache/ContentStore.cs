using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpTest.Net.Collections;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Scoping;
using Umbraco.Web.PublishedCache.NuCache.Snap;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // stores content
    internal class ContentStore
    {
        // this class is an extended version of SnapDictionary
        // most of the snapshots management code, etc is an exact copy
        // SnapDictionary has unit tests to ensure it all works correctly

        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ConcurrentDictionary<int, LinkedNode<ContentNode>> _contentNodes;
        private readonly ConcurrentDictionary<int, LinkedNode<object>> _contentRootNodes;
        private readonly ConcurrentDictionary<int, LinkedNode<PublishedContentType>> _contentTypesById;
        private readonly ConcurrentDictionary<string, LinkedNode<PublishedContentType>> _contentTypesByAlias;
        private readonly ConcurrentDictionary<Guid, int> _xmap;

        private readonly ILogger _logger;
        private BPlusTree<int, ContentNodeKit> _localDb;
        private readonly ConcurrentQueue<GenObj> _genObjs;
        private GenObj _genObj;
        private readonly object _wlocko = new object();
        private readonly object _rlocko = new object();
        private long _liveGen, _floorGen;
        private bool _nextGen, _collectAuto;
        private Task _collectTask;
        private volatile int _wlocked;
        private List<KeyValuePair<int, ContentNodeKit>> _wchanges;

        // TODO: collection trigger (ok for now)
        // see SnapDictionary notes
        private const long CollectMinGenDelta = 8;

        #region Ctor

        public ContentStore(
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IVariationContextAccessor variationContextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor,
            ILogger logger,
            BPlusTree<int, ContentNodeKit> localDb = null)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            _variationContextAccessor = variationContextAccessor;
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
            _localDb = localDb;

            _contentNodes = new ConcurrentDictionary<int, LinkedNode<ContentNode>>();
            _contentRootNodes = new ConcurrentDictionary<int, LinkedNode<object>>();
            _contentTypesById = new ConcurrentDictionary<int, LinkedNode<PublishedContentType>>();
            _contentTypesByAlias = new ConcurrentDictionary<string, LinkedNode<PublishedContentType>>(StringComparer.InvariantCultureIgnoreCase);
            _xmap = new ConcurrentDictionary<Guid, int>();

            _genObjs = new ConcurrentQueue<GenObj>();
            _genObj = null; // no initial gen exists
            _liveGen = _floorGen = 0;
            _nextGen = false; // first time, must create a snapshot
            _collectAuto = true; // collect automatically by default
        }

        #endregion

        #region Locking

        // see notes on SnapDictionary

        private readonly string _instanceId = Guid.NewGuid().ToString("N");

        private class ReadLockInfo
        {
            public bool Taken;
        }

        private class WriteLockInfo
        {
            public bool Taken;
            public bool Count;
        }

        // a scope contextual that represents a locked writer to the dictionary
        private class ScopedWriteLock : ScopeContextualBase
        {
            private readonly WriteLockInfo _lockinfo = new WriteLockInfo();
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
                    return;
                _store.Release(_lockinfo, completed);
            }
        }

        // gets a scope contextual representing a locked writer to the dictionary
        // TODO: GetScopedWriter? should the dict have a ref onto the scope provider?
        public IDisposable GetScopedWriteLock(IScopeProvider scopeProvider)
        {
            return ScopeContextualBase.Get(scopeProvider, _instanceId, scoped => new ScopedWriteLock(this, scoped));
        }

        private void Lock(WriteLockInfo lockInfo, bool forceGen = false)
        {
            Monitor.Enter(_wlocko, ref lockInfo.Taken);

            var rtaken = false;
            try
            {
                Monitor.Enter(_rlocko, ref rtaken);

                // see SnapDictionary
                try { } finally
                {
                    _wlocked++;
                    lockInfo.Count = true;
                    if (_nextGen == false || (forceGen && _wlocked == 1))
                    {
                        // because we are changing things, a new generation
                        // is created, which will trigger a new snapshot
                        if (_nextGen)
                            _genObjs.Enqueue(_genObj = new GenObj(_liveGen));
                        _liveGen += 1;
                        _nextGen = true;
                    }
                }
            }
            finally
            {
                if (rtaken) Monitor.Exit(_rlocko);
            }
        }

        private void Lock(ReadLockInfo lockInfo)
        {
            Monitor.Enter(_rlocko, ref lockInfo.Taken);
        }

        private void Release(WriteLockInfo lockInfo, bool commit = true)
        {
            if (commit == false)
            {
                var rtaken = false;
                try
                {
                    Monitor.Enter(_rlocko, ref rtaken);
                    try { }
                    finally
                    {
                        _nextGen = false;
                        _liveGen -= 1;
                    }
                }
                finally
                {
                    if (rtaken) Monitor.Exit(_rlocko);
                }

                Rollback(_contentNodes);
                Rollback(_contentRootNodes);
                Rollback(_contentTypesById);
                Rollback(_contentTypesByAlias);
            }
            else if (_localDb != null && _wchanges != null)
            {
                foreach (var change in _wchanges)
                {
                    if (change.Value.IsNull)
                        _localDb.TryRemove(change.Key, out ContentNodeKit unused);
                    else
                        _localDb[change.Key] = change.Value;
                }
                _wchanges = null;
                _localDb.Commit();
            }

            if (lockInfo.Count) _wlocked--;
            if (lockInfo.Taken) Monitor.Exit(_wlocko);
        }

        private void Release(ReadLockInfo lockInfo)
        {
            if (lockInfo.Taken) Monitor.Exit(_rlocko);
        }

        private void Rollback<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dictionary)
            where TValue : class
        {
            foreach (var item in dictionary)
            {
                var link = item.Value;
                if (link.Gen <= _liveGen) continue;

                var key = item.Key;
                if (link.Next == null)
                    dictionary.TryRemove(key, out link);
                else
                    dictionary.TryUpdate(key, link.Next, link);
            }
        }

        #endregion

        #region LocalDb

        public void ReleaseLocalDb()
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                if (_localDb == null) return;
                _localDb.Dispose();
                _localDb = null;
            }
            finally
            {
                Release(lockInfo);
            }
        }

        private void RegisterChange(int id, ContentNodeKit kit)
        {
            if (_wchanges == null) _wchanges = new List<KeyValuePair<int, ContentNodeKit>>();
            _wchanges.Add(new KeyValuePair<int, ContentNodeKit>(id, kit));
        }

        #endregion

        #region Content types

        public void NewContentTypes(IEnumerable<PublishedContentType> types)
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                foreach (var type in types)
                {
                    SetValueLocked(_contentTypesById, type.Id, type);
                    SetValueLocked(_contentTypesByAlias, type.Alias, type);
                }
            }
            finally
            {
                Release(lockInfo);
            }
        }

        public void UpdateContentTypes(IEnumerable<PublishedContentType> types)
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                var index = types.ToDictionary(x => x.Id, x => x);

                foreach (var type in index.Values)
                {
                    SetValueLocked(_contentTypesById, type.Id, type);
                    SetValueLocked(_contentTypesByAlias, type.Alias, type);
                }

                foreach (var link in _contentNodes.Values)
                {
                    var node = link.Value;
                    if (node == null) continue;
                    var contentTypeId = node.ContentType.Id;
                    if (index.TryGetValue(contentTypeId, out PublishedContentType contentType) == false) continue;
                    SetValueLocked(_contentNodes, node.Id, new ContentNode(node, contentType, _publishedSnapshotAccessor, _variationContextAccessor, _umbracoContextAccessor));
                }
            }
            finally
            {
                Release(lockInfo);
            }
        }

        public void UpdateContentTypes(IEnumerable<int> removedIds, IEnumerable<PublishedContentType> refreshedTypes, IEnumerable<ContentNodeKit> kits)
        {
            var removedIdsA = removedIds?.ToArray() ?? Array.Empty<int>();
            var refreshedTypesA = refreshedTypes?.ToArray() ?? Array.Empty<PublishedContentType>();
            var refreshedIdsA = refreshedTypesA.Select(x => x.Id).ToArray();
            kits = kits ?? Array.Empty<ContentNodeKit>();

            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                var removedContentTypeNodes = new List<int>();
                var refreshedContentTypeNodes = new List<int>();

                foreach (var link in _contentNodes.Values)
                {
                    var node = link.Value;
                    if (node == null) continue;
                    var contentTypeId = node.ContentType.Id;
                    if (removedIdsA.Contains(contentTypeId)) removedContentTypeNodes.Add(node.Id);
                    if (refreshedIdsA.Contains(contentTypeId)) refreshedContentTypeNodes.Add(node.Id);
                }

                // all content should have been deleted - but
                foreach (var node in removedContentTypeNodes)
                    ClearBranchLocked(node);

                foreach (var id in removedIdsA)
                {
                    if (_contentTypesById.TryGetValue(id, out LinkedNode<PublishedContentType> link) == false || link.Value == null)
                        continue;
                    SetValueLocked(_contentTypesById, id, null);
                    SetValueLocked(_contentTypesByAlias, link.Value.Alias, null);
                }

                foreach (var type in refreshedTypesA)
                {
                    SetValueLocked(_contentTypesById, type.Id, type);
                    SetValueLocked(_contentTypesByAlias, type.Alias, type);
                }

                // skip missing type
                // skip missing parents & unbuildable kits - what else could we do?
                var visited = new List<int>();
                foreach (var kit in kits.Where(x =>
                    refreshedIdsA.Contains(x.ContentTypeId) &&
                    ParentExistsLocked(x) &&
                    BuildKit(x)))
                {
                    SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
                    visited.Add(kit.Node.Id);
                    if (_localDb != null) RegisterChange(kit.Node.Id, kit);
                }

                // all content should have been refreshed - but...

                var orphans = refreshedContentTypeNodes.Except(visited);
                foreach (var id in orphans)
                    ClearBranchLocked(id);
            }
            finally
            {
                Release(lockInfo);
            }
        }

        public void UpdateDataTypes(IEnumerable<int> dataTypeIds, Func<int, PublishedContentType> getContentType)
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                var contentTypes = _contentTypesById
                    .Where(kvp =>
                        kvp.Value.Value != null &&
                        kvp.Value.Value.PropertyTypes.Any(p => dataTypeIds.Contains(p.DataType.Id)))
                    .Select(kvp => kvp.Value.Value)
                    .Select(x => getContentType(x.Id))
                    .Where(x => x != null) // poof, gone, very unlikely and probably an anomaly
                    .ToArray();

                var contentTypeIdsA = contentTypes.Select(x => x.Id).ToArray();
                var contentTypeNodes = new Dictionary<int, List<int>>();
                foreach (var id in contentTypeIdsA)
                    contentTypeNodes[id] = new List<int>();
                foreach (var link in _contentNodes.Values)
                {
                    var node = link.Value;
                    if (node != null && contentTypeIdsA.Contains(node.ContentType.Id))
                        contentTypeNodes[node.ContentType.Id].Add(node.Id);
                }

                foreach (var contentType in contentTypes)
                {
                    // again, weird situation
                    if (contentTypeNodes.ContainsKey(contentType.Id) == false)
                        continue;

                    foreach (var id in contentTypeNodes[contentType.Id])
                    {
                        _contentNodes.TryGetValue(id, out LinkedNode<ContentNode> link);
                        if (link?.Value == null)
                            continue;
                        var node = new ContentNode(link.Value, contentType, _publishedSnapshotAccessor, _variationContextAccessor, _umbracoContextAccessor);
                        SetValueLocked(_contentNodes, id, node);
                        if (_localDb != null) RegisterChange(id, node.ToKit());
                    }
                }
            }
            finally
            {
                Release(lockInfo);
            }
        }

        private bool BuildKit(ContentNodeKit kit)
        {
            // make sure the kit is valid
            if (kit.DraftData == null && kit.PublishedData == null)
                return false;

            // unknown = bad
            if (_contentTypesById.TryGetValue(kit.ContentTypeId, out LinkedNode<PublishedContentType> link) == false || link.Value == null)
                return false;

            // check whether parent is published
            var canBePublished = ParentPublishedLocked(kit);

            // and use
            kit.Build(link.Value, _publishedSnapshotAccessor, _variationContextAccessor, canBePublished, _umbracoContextAccessor);

            return true;
        }

        #endregion

        #region Set, Clear, Get

        public int Count => _contentNodes.Count;

        private static LinkedNode<TValue> GetHead<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict, TKey key)
            where TValue : class
        {
            dict.TryGetValue(key, out LinkedNode<TValue> link); // else null
            return link;
        }

        public void Set(ContentNodeKit kit)
        {
            // ReSharper disable LocalizableElement
            if (kit.IsEmpty)
                throw new ArgumentException("Kit is empty.", nameof(kit));
            if (kit.Node.ChildContentIds.Count > 0)
                throw new ArgumentException("Kit content cannot have children.", nameof(kit));
            // ReSharper restore LocalizableElement

            _logger.Debug<ContentStore>("Set content ID: {KitNodeId}", kit.Node.Id);

            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                // get existing
                _contentNodes.TryGetValue(kit.Node.Id, out LinkedNode<ContentNode> link);
                var existing = link?.Value;

                // else ignore, what else could we do?
                if (ParentExistsLocked(kit) == false || BuildKit(kit) == false)
                    return;

                // moving?
                var moving = existing != null && existing.ParentContentId != kit.Node.ParentContentId;

                // manage children
                if (existing != null)
                    kit.Node.ChildContentIds = existing.ChildContentIds;

                // set
                SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
                if (_localDb != null) RegisterChange(kit.Node.Id, kit);

                // manage the tree
                if (existing == null)
                {
                    // new, add to parent
                    AddToParentLocked(kit.Node);
                }
                else if (moving)
                {
                    // moved, remove existing from its parent, add content to its parent
                    RemoveFromParentLocked(existing);
                    AddToParentLocked(kit.Node);
                }

                _xmap[kit.Node.Uid] = kit.Node.Id;
            }
            finally
            {
                Release(lockInfo);
            }
        }

        // IMPORTANT kits must be sorted out by LEVEL
        public void SetAll(IEnumerable<ContentNodeKit> kits)
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                ClearLocked(_contentNodes);
                ClearLocked(_contentRootNodes);

                // do NOT clear types else they are gone!
                //ClearLocked(_contentTypesById);
                //ClearLocked(_contentTypesByAlias);

                // skip missing parents & unbuildable kits - what else could we do?
                foreach (var kit in kits.Where(x => ParentExistsLocked(x) && BuildKit(x)))
                {
                    SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
                    if (_localDb != null) RegisterChange(kit.Node.Id, kit);
                    AddToParentLocked(kit.Node);

                    _xmap[kit.Node.Uid] = kit.Node.Id;
                }
            }
            finally
            {
                Release(lockInfo);
            }
        }

        // IMPORTANT kits must be sorted out by LEVEL
        public void SetBranch(int rootContentId, IEnumerable<ContentNodeKit> kits)
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                // get existing
                _contentNodes.TryGetValue(rootContentId, out LinkedNode<ContentNode> link);
                var existing = link?.Value;

                // clear
                if (existing != null)
                {
                    ClearBranchLocked(existing);
                    RemoveFromParentLocked(existing);
                }

                // now add them all back
                // skip missing parents & unbuildable kits - what else could we do?
                foreach (var kit in kits.Where(x => ParentExistsLocked(x) && BuildKit(x)))
                {
                    SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
                    if (_localDb != null) RegisterChange(kit.Node.Id, kit);
                    AddToParentLocked(kit.Node);

                    _xmap[kit.Node.Uid] = kit.Node.Id;
                }
            }
            finally
            {
                Release(lockInfo);
            }
        }

        public bool Clear(int id)
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                // try to find the content
                // if it is not there, nothing to do
                _contentNodes.TryGetValue(id, out LinkedNode<ContentNode> link); // else null
                if (link?.Value == null) return false;

                var content = link.Value;
                _logger.Debug<ContentStore>("Clear content ID: {ContentId}", content.Id);

                // clear the entire branch
                ClearBranchLocked(content);

                // manage the tree
                RemoveFromParentLocked(content);

                return true;
            }
            finally
            {
                Release(lockInfo);
            }
        }

        private void ClearBranchLocked(int id)
        {
            _contentNodes.TryGetValue(id, out var link);
            if (link?.Value == null)
                return;
            ClearBranchLocked(link.Value);
        }

        private void ClearBranchLocked(ContentNode content)
        {
            SetValueLocked(_contentNodes, content.Id, null);
            if (_localDb != null) RegisterChange(content.Id, ContentNodeKit.Null);

            _xmap.TryRemove(content.Uid, out _);

            foreach (var childId in content.ChildContentIds)
            {
                if (_contentNodes.TryGetValue(childId, out LinkedNode<ContentNode> link) == false || link.Value == null) continue;
                ClearBranchLocked(link.Value);
            }
        }

        private LinkedNode<ContentNode> GetParentLink(ContentNode content)
        {
            _contentNodes.TryGetValue(content.ParentContentId, out var link); // else null
            //if (link == null || link.Value == null)
            //    throw new Exception("Panic: parent not found.");
            return link;
        }

        private void RemoveFromParentLocked(ContentNode content)
        {
            // remove from root content index,
            // or parent's children index
            if (content.ParentContentId < 0)
            {
                SetValueLocked(_contentRootNodes, content.Id, null);
            }
            else
            {
                // obviously parent has to exist
                var link = GetParentLink(content);
                var parent = link.Value;
                if (link.Gen < _liveGen)
                    parent = parent.CloneParent(_publishedSnapshotAccessor, _umbracoContextAccessor);
                parent.ChildContentIds.Remove(content.Id);
                if (link.Gen < _liveGen)
                    SetValueLocked(_contentNodes, parent.Id, parent);
            }
        }

        private bool ParentExistsLocked(ContentNodeKit kit)
        {
            if (kit.Node.ParentContentId < 0)
                return true;
            var link = GetParentLink(kit.Node);
            return link?.Value != null;
        }

        private bool ParentPublishedLocked(ContentNodeKit kit)
        {
            if (kit.Node.ParentContentId < 0)
                return true;
            var link = GetParentLink(kit.Node);
            var node = link?.Value;
            return node?.PublishedModel != null;
        }

        private void AddToParentLocked(ContentNode content)
        {
            // add to root content index,
            // or parent's children index
            if (content.ParentContentId < 0)
            {
                // need an object reference... just use this...
                SetValueLocked(_contentRootNodes, content.Id, this);
            }
            else
            {
                // assume parent has been validated and exists
                var link = GetParentLink(content);
                var parent = link.Value;
                if (link.Gen < _liveGen)
                    parent = parent.CloneParent(_publishedSnapshotAccessor, _umbracoContextAccessor);
                parent.ChildContentIds.Add(content.Id);
                if (link.Gen < _liveGen)
                    SetValueLocked(_contentNodes, parent.Id, parent);
            }
        }

        private void SetValueLocked<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict, TKey key, TValue value)
            where TValue : class
        {
            // this is safe only because we're write-locked
            var link = GetHead(dict, key);
            if (link != null)
            {
                // already in the dict
                if (link.Gen != _liveGen)
                {
                    // for an older gen - if value is different then insert a new
                    // link for the new gen, with the new value
                    if (link.Value != value)
                        dict.TryUpdate(key, new LinkedNode<TValue>(value, _liveGen, link), link);
                }
                else
                {
                    // for the live gen - we can fix the live gen - and remove it
                    // if value is null and there's no next gen
                    if (value == null && link.Next == null)
                        dict.TryRemove(key, out link);
                    else
                        link.Value = value;
                }
            }
            else
            {
                dict.TryAdd(key, new LinkedNode<TValue>(value, _liveGen));
            }
        }

        private void ClearLocked<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict)
            where TValue : class
        {
            // this is safe only because we're write-locked
            foreach (var kvp in dict.Where(x => x.Value != null))
            {
                if (kvp.Value.Gen < _liveGen)
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

        public ContentNode Get(int id, long gen)
        {
            return GetValue(_contentNodes, id, gen);
        }

        public ContentNode Get(Guid uid, long gen)
        {
            return _xmap.TryGetValue(uid, out var id)
                ? GetValue(_contentNodes, id, gen)
                : null;
        }

        public IEnumerable<ContentNode> GetAtRoot(long gen)
        {
            // look ma, no lock!
            foreach (var kvp in _contentRootNodes)
            {
                var link = kvp.Value;
                while (link != null)
                {
                    if (link.Gen <= gen)
                        break;
                    link = link.Next;
                }
                if (link?.Value != null)
                    yield return Get(kvp.Key, gen);
            }
        }

        private TValue GetValue<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict, TKey key, long gen)
            where TValue : class
        {
            // look ma, no lock!
            var link = GetHead(dict, key);
            while (link != null)
            {
                if (link.Gen <= gen)
                    return link.Value; // may be null
                link = link.Next;
            }
            return null;
        }

        public IEnumerable<ContentNode> GetAll(long gen)
        {
            // enumerating on .Values locks the concurrent dictionary,
            // so better get a shallow clone in an array and release
            var links = _contentNodes.Values.ToArray();
            foreach (var l in links)
            {
                var link = l;
                while (link != null)
                {
                    if (link.Gen <= gen)
                    {
                        if (link.Value != null)
                            yield return link.Value;
                        break;
                    }
                    link = link.Next;
                }
            }
        }

        public bool IsEmpty(long gen)
        {
            var has = _contentNodes.Any(x =>
            {
                var link = x.Value;
                while (link != null)
                {
                    if (link.Gen <= gen && link.Value != null)
                        return true;
                    link = link.Next;
                }
                return false;
            });
            return has == false;
        }

        public PublishedContentType GetContentType(int id, long gen)
        {
            return GetValue(_contentTypesById, id, gen);
        }

        public PublishedContentType GetContentType(string alias, long gen)
        {
            return GetValue(_contentTypesByAlias, alias, gen);
        }

        #endregion

        #region Snapshots

        public Snapshot CreateSnapshot()
        {
            var lockInfo = new ReadLockInfo();
            try
            {
                Lock(lockInfo);

                // if no next generation is required, and we already have one,
                // use it and create a new snapshot
                if (_nextGen == false && _genObj != null)
                    return new Snapshot(this, _genObj.GetGenRef()
#if DEBUG
                        , _logger
#endif
                        );

                // else we need to try to create a new gen ref
                // whether we are wlocked or not, noone can rlock while we do,
                // so _liveGen and _nextGen are safe
                if (_wlocked > 0) // volatile, cannot ++ but could --
                {
                    // write-locked, cannot use latest gen (at least 1) so use previous
                    var snapGen = _nextGen ? _liveGen - 1 : _liveGen;

                    // create a new gen ref unless we already have it
                    if (_genObj == null)
                        _genObjs.Enqueue(_genObj = new GenObj(snapGen));
                    else if (_genObj.Gen != snapGen)
                        throw new Exception("panic");
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
                    , _logger
#endif
                    );

                // reading _floorGen is safe if _collectTask is null
                if (_collectTask == null && _collectAuto && _liveGen - _floorGen > CollectMinGenDelta)
                    CollectAsyncLocked();

                return snapshot;
            }
            finally
            {
                Release(lockInfo);
            }
        }

        public Snapshot LiveSnapshot => new Snapshot(this, _liveGen
#if DEBUG
            , _logger
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
            if (_collectTask != null)
                return _collectTask;

            // ReSharper disable InconsistentlySynchronizedField
            var task = _collectTask = Task.Run(() => Collect());
            _collectTask.ContinueWith(_ =>
            {
                lock (_rlocko)
                {
                    _collectTask = null;
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
            // ReSharper restore InconsistentlySynchronizedField

            return task;
        }

        private void Collect()
        {
            // see notes in CreateSnapshot
#if DEBUG
            _logger.Debug<ContentStore>("Collect.");
#endif
            while (_genObjs.TryPeek(out var genObj) && (genObj.Count == 0 || genObj.WeakGenRef.IsAlive == false))
            {
                _genObjs.TryDequeue(out genObj); // cannot fail since TryPeek has succeeded
                _floorGen = genObj.Gen;
#if DEBUG
                //_logger.Debug<ContentStore>("_floorGen=" + _floorGen + ", _liveGen=" + _liveGen);
#endif
            }

            Collect(_contentNodes);
            Collect(_contentRootNodes);
            Collect(_contentTypesById);
            Collect(_contentTypesByAlias);
        }

        private void Collect<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict)
            where TValue : class
        {
            // it is OK to enumerate a concurrent dictionary and it does not lock
            // it - and here it's not an issue if we skip some items, they will be
            // processed next time we collect

            long liveGen;
            lock (_rlocko) // r is good
            {
                liveGen = _liveGen;
                if (_nextGen == false)
                    liveGen += 1;
            }

            foreach (var kvp in dict)
            {
                var link = kvp.Value;

#if DEBUG
                //_logger.Debug<ContentStore>("Collect id:" + kvp.Key + ", gen:" + link.Gen +
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
                    continue;

                // else go to Next and loop while above floor, and kill everything below
                while (link.Next != null && link.Next.Gen > _floorGen)
                    link = link.Next;
                link.Next = null;
            }
        }

        public async Task WaitForPendingCollect()
        {
            Task task;
            lock (_rlocko)
            {
                task = _collectTask;
            }
            if (task != null)
                await task;
        }

        public long GenCount => _genObjs.Count;

        public long SnapCount => _genObjs.Sum(x => x.Count);

        #endregion

        #region Unit testing

        private TestHelper _unitTesting;

        // note: nothing here is thread-safe
        internal class TestHelper
        {
            private readonly ContentStore _store;

            public TestHelper(ContentStore store)
            {
                _store = store;
            }

            public long LiveGen => _store._liveGen;
            public long FloorGen => _store._floorGen;
            public bool NextGen => _store._nextGen;
            public bool CollectAuto
            {
                get => _store._collectAuto;
                set => _store._collectAuto = value;
            }

            public Tuple<long, ContentNode>[] GetValues(int id)
            {
                _store._contentNodes.TryGetValue(id, out LinkedNode<ContentNode> link); // else null

                if (link == null)
                    return new Tuple<long, ContentNode>[0];

                var tuples = new List<Tuple<long, ContentNode>>();
                do
                {
                    tuples.Add(Tuple.Create(link.Gen, link.Value));
                    link = link.Next;
                } while (link != null);
                return tuples.ToArray();
            }
        }

        internal TestHelper Test => _unitTesting ?? (_unitTesting = new TestHelper(this));

        #endregion

        #region Classes

        public class Snapshot : IDisposable
        {
            private readonly ContentStore _store;
            private readonly GenRef _genRef;
            private long _gen;
#if DEBUG
            private readonly ILogger _logger;
#endif

            //private static int _count;
            //private readonly int _thisCount;

            internal Snapshot(ContentStore store, GenRef genRef
#if DEBUG
                    , ILogger logger
#endif
                )
            {
                _store = store;
                _genRef = genRef;
                _gen = genRef.Gen;
                Interlocked.Increment(ref genRef.GenObj.Count);
                //_thisCount = _count++;

#if DEBUG
                _logger = logger;
                _logger.Debug<Snapshot>("Creating snapshot.");
#endif
            }

            internal Snapshot(ContentStore store, long gen
#if DEBUG
                , ILogger logger
#endif
                )
            {
                _store = store;
                _gen = gen;

#if DEBUG
                _logger = logger;
                _logger.Debug<Snapshot>("Creating live.");
#endif
            }

            public ContentNode Get(int id)
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.Get(id, _gen);
            }

            public ContentNode Get(Guid id)
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.Get(id, _gen);
            }

            public IEnumerable<ContentNode> GetAtRoot()
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.GetAtRoot(_gen);
            }

            public IEnumerable<ContentNode> GetAll()
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.GetAll(_gen);
            }

            public PublishedContentType GetContentType(int id)
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.GetContentType(id, _gen);
            }

            public PublishedContentType GetContentType(string alias)
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.GetContentType(alias, _gen);
            }

            // this code is here just so you don't try to implement it
            // the only way we can iterate over "all" without locking the entire cache forever
            // is by shallow cloning the cache, which is quite expensive, so we should probably not do it,
            // and implement cache-level indexes
            //public IEnumerable<ContentNode> GetAll()
            //{
            //    if (_gen < 0)
            //        throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            //    return _store.GetAll(_gen);
            //}

            public bool IsEmpty
            {
                get
                {
                    if (_gen < 0)
                        throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                    return _store.IsEmpty(_gen);
                }
            }

            public long Gen
            {
                get
                {
                    if (_gen < 0)
                        throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                    return _gen;
                }
            }

            public void Dispose()
            {
                if (_gen < 0) return;
#if DEBUG
                _logger.Debug<Snapshot>("Dispose snapshot ({Snapshot})", _genRef?.GenObj.Count.ToString() ?? "live");
#endif
                _gen = -1;
                if (_genRef != null)
                    Interlocked.Decrement(ref _genRef.GenObj.Count);
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
