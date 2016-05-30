using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpTest.Net.Collections;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // stores content
    internal class ContentStore2
    {
        // this class is an extended version of SnapDictionary
        // most of the snapshots management code, etc is an exact copy
        // SnapDictionary has unit tests to ensure it all works correctly

        private readonly IFacadeAccessor _facadeAccessor;
        private readonly ConcurrentDictionary<int, LinkedNode<ContentNode>> _contentNodes;
        private readonly ConcurrentDictionary<int, LinkedNode<object>> _contentRootNodes;
        private readonly ConcurrentDictionary<int, LinkedNode<PublishedContentType>> _contentTypesById;
        private readonly ConcurrentDictionary<string, LinkedNode<PublishedContentType>> _contentTypesByAlias;
        private readonly Dictionary<int, HashSet<int>> _contentTypeNodes;

        private readonly ILogger _logger;
        private BPlusTree<int, ContentNodeKit> _localDb;
        private readonly ConcurrentQueue<GenRefRef> _genRefRefs;
        private GenRefRef _genRefRef;
        private readonly object _wlocko = new object();
        private readonly object _rlocko = new object();
        private long _liveGen, _floorGen;
        private bool _nextGen, _collectAuto;
        private Task _collectTask;
        private volatile int _wlocked;

        // fixme - collection trigger (ok for now)
        // see SnapDictionary notes
        private const long CollectMinGenDelta = 8;

        #region Ctor

        public ContentStore2(IFacadeAccessor facadeAccessor, ILogger logger, BPlusTree<int, ContentNodeKit> localDb = null)
        {
            _facadeAccessor = facadeAccessor;
            _logger = logger;
            _localDb = localDb;

            _contentNodes = new ConcurrentDictionary<int, LinkedNode<ContentNode>>();
            _contentRootNodes = new ConcurrentDictionary<int, LinkedNode<object>>();
            _contentTypesById = new ConcurrentDictionary<int, LinkedNode<PublishedContentType>>();
            _contentTypesByAlias = new ConcurrentDictionary<string, LinkedNode<PublishedContentType>>(StringComparer.InvariantCultureIgnoreCase);
            _contentTypeNodes = new Dictionary<int, HashSet<int>>();

            _genRefRefs = new ConcurrentQueue<GenRefRef>();
            _genRefRef = null; // no initial gen exists
            _liveGen = _floorGen = 0;
            _nextGen = false; // first time, must create a snapshot
            _collectAuto = true; // collect automatically by default
        }

        #endregion

        #region Locking

        public void WriteLocked(Action action)
        {
            var wtaken = false;
            var wcount = false;
            try
            {
                Monitor.Enter(_wlocko, ref wtaken);

                var rtaken = false;
                try
                {
                    Monitor.Enter(_rlocko, ref rtaken);

                    // see SnapDictionary
                    try
                    { }
                    finally
                    {
                        _wlocked++;
                        wcount = true;
                        if (_nextGen == false)
                        {
                            // because we are changing things, a new generation
                            // is created, which will trigger a new snapshot
                            _nextGen = true;
                            _liveGen += 1;
                        }
                    }
                }
                finally
                {
                    if (rtaken) Monitor.Exit(_rlocko);
                }

                action();
            }
            finally
            {
                if (wcount) _wlocked--;
                if (wtaken) Monitor.Exit(_wlocko);
            }
        }

        public T WriteLocked<T>(Func<T> func)
        {
            var wtaken = false;
            var wcount = false;
            try
            {
                Monitor.Enter(_wlocko, ref wtaken);

                var rtaken = false;
                try
                {
                    Monitor.Enter(_rlocko, ref rtaken);

                    try
                    { }
                    finally
                    {
                        _wlocked++;
                        wcount = true;
                        if (_nextGen == false)
                        {
                            // because we are changing things, a new generation
                            // is created, which will trigger a new snapshot
                            _nextGen = true;
                            _liveGen += 1;
                        }
                    }
                }
                finally
                {
                    if (rtaken) Monitor.Exit(_rlocko);
                }

                return func();
            }
            finally
            {
                if (wcount) _wlocked--;
                if (wtaken) Monitor.Exit(_wlocko);
            }
        }

        private T ReadLocked<T>(Func<bool, T> func)
        {
            var rtaken = false;
            try
            {
                Monitor.Enter(_rlocko, ref rtaken);

                // we have rlock, so it cannot ++
                // it could -- though, so... volatile
                var wlocked = _wlocked > 0;
                return func(wlocked);
            }
            finally
            {
                if (rtaken) Monitor.Exit(_rlocko);
            }
        }

        #endregion

        #region LocalDb

        public void ReleaseLocalDb()
        {
            WriteLocked(() =>
            {
                if (_localDb == null) return;
                _localDb.Dispose();
                _localDb = null;
            });
        }
        
        #endregion

        #region Content types

        public void UpdateContentTypes(IEnumerable<int> removedIds, IEnumerable<PublishedContentType> refreshedTypes, IEnumerable<ContentNodeKit> kits)
        {
            removedIds = removedIds ?? Enumerable.Empty<int>();
            refreshedTypes = refreshedTypes ?? Enumerable.Empty<PublishedContentType>();
            kits = kits ?? new ContentNodeKit[0];

            WriteLocked(() =>
            {
                foreach (var id in removedIds)
                {
                    // all content should have been deleted - but
                    if (_contentTypeNodes.ContainsKey(id))
                    {
                        foreach (var node in _contentTypeNodes[id])
                            ClearBranchLocked(node);
                        _contentTypeNodes.Remove(id);
                    }

                    LinkedNode<PublishedContentType> link;
                    if (_contentTypesById.TryGetValue(id, out link) == false || link.Value == null)
                        continue;
                    SetValueLocked(_contentTypesById, id, null);
                    SetValueLocked(_contentTypesByAlias, link.Value.Alias, null);
                }

                var temp = new Dictionary<int, HashSet<int>>();

                foreach (var type in refreshedTypes)
                {
                    if (_contentTypeNodes.ContainsKey(type.Id) == false)
                        _contentTypeNodes[type.Id] = new HashSet<int>();

                    SetValueLocked(_contentTypesById, type.Id, type);
                    SetValueLocked(_contentTypesByAlias, type.Alias, type);

                    temp.Add(type.Id, new HashSet<int>(_contentTypeNodes[type.Id]));
                }

                // skip missing type
                // skip missing parents & unbuildable kits - what else could we do?
                foreach (var kit in kits.Where(x =>
                    temp.ContainsKey(x.ContentTypeId) &&
                    ParentExistsLocked(x) &&
                    BuildKit(x)))
                {
                    SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
                    if (_localDb != null)
                        _localDb[kit.Node.Id] = kit;
                    temp[kit.ContentTypeId].Remove(kit.Node.Id);
                }

                // all content should have been refreshed - but...
                foreach (var id in temp.Values.SelectMany(x => x))
                    ClearBranchLocked(id);

                if (_localDb != null)
                    _localDb.Commit();
            });
        }

        public void UpdateDataTypes(IEnumerable<int> dataTypeIds, Func<int, PublishedContentType> getContentType)
        {
            WriteLocked(() =>
            {
                var contentTypes = _contentTypesById
                    .Where(kvp =>
                        kvp.Value.Value != null &&
                        kvp.Value.Value.PropertyTypes.Any(p => dataTypeIds.Contains(p.DataTypeId)))
                    .Select(kvp => kvp.Value.Value)
                    .Select(x => getContentType(x.Id));

                foreach (var contentType in contentTypes)
                {
                    // poof, gone, very unlikely and probably an anomaly
                    if (contentType == null)
                        continue;

                    // again, weird situation
                    if (_contentTypeNodes.ContainsKey(contentType.Id) == false)
                        continue;

                    foreach (var id in _contentTypeNodes[contentType.Id])
                    {
                        LinkedNode<ContentNode> link;
                        _contentNodes.TryGetValue(id, out link);
                        if (link == null || link.Value == null)
                            continue;
                        var node = new ContentNode(link.Value, contentType, _facadeAccessor);
                        SetValueLocked(_contentNodes, id, node);
                        if (_localDb != null)
                            _localDb[id] = node.ToKit();
                    }
                }

                if (_localDb != null)
                    _localDb.Commit();
            });
        }

        private bool BuildKit(ContentNodeKit kit)
        {
            // make sure the kit is valid
            if (kit.DraftData == null && kit.PublishedData == null)
                return false;

            LinkedNode<PublishedContentType> link;

            // unknown = bad
            if (_contentTypesById.TryGetValue(kit.ContentTypeId, out link) == false || link.Value == null)
                return false;
            
            // not checking ByAlias, assuming we don't have internal errors

            // register
            if (_contentTypeNodes.ContainsKey(kit.ContentTypeId) == false)
                _contentTypeNodes[kit.ContentTypeId] = new HashSet<int>();
            _contentTypeNodes[kit.ContentTypeId].Add(kit.Node.Id);

            // and use
            kit.Build(link.Value, _facadeAccessor);

            return true;
        }

        private void ReleaseContentTypeLocked(ContentNode content)
        {
            if (_contentTypeNodes.ContainsKey(content.ContentType.Id) == false)
                return; // though, ?!
            _contentTypeNodes[content.ContentType.Id].Remove(content.Id);
        }

        #endregion

        #region Set, Clear, Get

        public int Count
        {
            get { return _contentNodes.Count; }
        }

        private LinkedNode<TValue> GetHead<TKey, TValue>(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict, TKey key)
            where TValue : class
        {
            LinkedNode<TValue> link;
            dict.TryGetValue(key, out link); // else null
            return link;
        }

        public void Set(ContentNodeKit kit)
        {
            // ReSharper disable LocalizableElement
            if (kit.IsEmpty)
                throw new ArgumentException("Kit is empty.", "kit");
            if (kit.Node.ChildContentIds.Count > 0)
                throw new ArgumentException("Kit content cannot have children.", "kit");
            // ReSharper restore LocalizableElement

            _logger.Debug<ContentStore2>("Set content ID:" + kit.Node.Id);

            WriteLocked(() =>
            {
                // get existing
                LinkedNode<ContentNode> link;
                _contentNodes.TryGetValue(kit.Node.Id, out link);
                var existing = link == null ? null : link.Value;

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
                if (_localDb != null)
                    _localDb[kit.Node.Id] = kit;

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

                if (_localDb != null)
                    _localDb.Commit();
            });
        }

        public void SetAll(IEnumerable<ContentNodeKit> kits)
        {
            WriteLocked(() =>
            {
                ClearLocked(_contentNodes);
                ClearLocked(_contentRootNodes);

                // do NOT clear types else they are gone!
                //ClearLocked(_contentTypesById);
                //ClearLocked(_contentTypesByAlias);

                // skip missing parents & unbuildable kits - what else could we do?
                foreach (var kit in kits.Where(x => ParentExistsLocked(x) && BuildKit(x)))
                {
                    SetValueLocked(_contentNodes, kit.Node.Id, kit.Node);
                    if (_localDb != null)
                        _localDb[kit.Node.Id] = kit;
                    AddToParentLocked(kit.Node);
                }

                if (_localDb != null)
                    _localDb.Commit();
            });
        }

        public void SetBranch(int rootContentId, IEnumerable<ContentNodeKit> kits)
        {
            WriteLocked(() =>
            {
                // get existing
                LinkedNode<ContentNode> link;
                _contentNodes.TryGetValue(rootContentId, out link);
                var existing = link == null ? null : link.Value;

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
                    if (_localDb != null)
                        _localDb[kit.Node.Id] = kit;
                    AddToParentLocked(kit.Node);
                }

                if (_localDb != null)
                    _localDb.Commit();
            });
        }

        public bool Clear(int id)
        {
            return WriteLocked(() =>
            {
                // try to find the content
                // if it is not there, nothing to do
                LinkedNode<ContentNode> link;
                _contentNodes.TryGetValue(id, out link); // else null
                if (link == null || link.Value == null) return false;

                var content = link.Value;
                _logger.Debug<ContentStore2>("Clear content ID:" + content.Id);

                // clear the entire branch
                ClearBranchLocked(content);

                // manage the tree
                RemoveFromParentLocked(content);

                return true;
            });
        }

        private void ClearBranchLocked(int id)
        {
            LinkedNode<ContentNode> link;
            _contentNodes.TryGetValue(id, out link);
            if (link == null || link.Value == null)
                return;
            ClearBranchLocked(link.Value);
        }

        private void ClearBranchLocked(ContentNode content)
        {
            SetValueLocked(_contentNodes, content.Id, null);
            if (_localDb != null)
            {
                ContentNodeKit kit;
                _localDb.TryRemove(content.Id, out kit);
            }
            ReleaseContentTypeLocked(content);
            foreach (var childId in content.ChildContentIds)
            {
                LinkedNode<ContentNode> link;
                if (_contentNodes.TryGetValue(childId, out link) == false || link.Value == null) continue;
                ClearBranchLocked(link.Value);
            }
        }

        private LinkedNode<ContentNode> GetParentLink(ContentNode content)
        {
            LinkedNode<ContentNode> link;
            _contentNodes.TryGetValue(content.ParentContentId, out link); // else null
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
                    parent = parent.CloneParent(_facadeAccessor);
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
            return link != null && link.Value != null;
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
                    parent = parent.CloneParent(_facadeAccessor);
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
            WriteLocked(() =>
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
            });
        }

        public ContentNode Get(int id, long gen)
        {
            return GetValue(_contentNodes, id, gen);
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
                if (link != null && link.Value != null)
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
            return ReadLocked(wlocked =>
            {
                // if no next generation is required, and we already have one,
                // use it and create a new snapshot
                if (_nextGen == false && _genRefRef != null)
                    return new Snapshot(this, _genRefRef.GetGenRef()
#if DEBUG
                        , _logger
#endif
                        );

                // else we need to try to create a new gen ref
                // whether we are wlocked or not, noone can rlock while we do,
                // so _liveGen and _nextGen are safe
                if (wlocked)
                {
                    // write-locked, cannot use latest gen (at least 1) so use previous
                    var snapGen = _nextGen ? _liveGen - 1 : _liveGen;

                    // create a new gen ref unless we already have it
                    if (_genRefRef == null)
                        _genRefRefs.Enqueue(_genRefRef = new GenRefRef(snapGen));
                    else if (_genRefRef.Gen != snapGen)
                        throw new Exception("panic");
                }
                else
                {
                    // not write-locked, can use latest gen, create a new gen ref
                    _genRefRefs.Enqueue(_genRefRef = new GenRefRef(_liveGen));
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

                var snapshot = new Snapshot(this, _genRefRef.GetGenRef()
#if DEBUG
                    , _logger
#endif
                    );

                // reading _floorGen is safe if _collectTask is null
                if (_collectTask == null && _collectAuto && _liveGen - _floorGen > CollectMinGenDelta)
                    CollectAsyncLocked();

                return snapshot;
            });
        }

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
            _logger.Debug<ContentStore2>("Collect.");
#endif
            GenRefRef genRefRef;
            while (_genRefRefs.TryPeek(out genRefRef) && (genRefRef.Count == 0 || genRefRef.WGenRef.IsAlive == false))
            {
                _genRefRefs.TryDequeue(out genRefRef); // cannot fail since TryPeek has succeeded
                _floorGen = genRefRef.Gen;
#if DEBUG
                //_logger.Debug<ContentStore2>("_floorGen=" + _floorGen + ", _liveGen=" + _liveGen);
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
                //_logger.Debug<ContentStore2>("Collect id:" + kvp.Key + ", gen:" + link.Gen +
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

        public long GenCount
        {
            get { return _genRefRefs.Count; }
        }

        public long SnapCount
        {
            get
            {
                return _genRefRefs.Sum(x => x.Count);
            }
        }

        #endregion

        #region Unit testing

        private TestHelper _unitTesting;

        // note: nothing here is thread-safe
        internal class TestHelper
        {
            private readonly ContentStore2 _store;

            public TestHelper(ContentStore2 store)
            {
                _store = store;
            }

            public long LiveGen { get { return _store._liveGen; } }
            public long FloorGen { get { return _store._floorGen; } }
            public bool NextGen { get { return _store._nextGen; } }
            public bool CollectAuto { get { return _store._collectAuto; } set { _store._collectAuto = value; } }

            public Tuple<long, ContentNode>[] GetValues(int id)
            {
                LinkedNode<ContentNode> link;
                _store._contentNodes.TryGetValue(id, out link); // else null

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

        internal TestHelper Test { get { return _unitTesting ?? (_unitTesting = new TestHelper(this)); } }
        
        #endregion

        #region Classes

        private class LinkedNode<TValue>
            where TValue: class
        {
            public LinkedNode(TValue value, long gen, LinkedNode<TValue> next = null)
            {
                Value = value;
                Gen = gen;
                Next = next;
            }

            internal readonly long Gen;

            // reading & writing references is thread-safe on all .NET platforms
            // mark as volatile to ensure we always read the correct value
            internal volatile TValue Value;
            internal volatile LinkedNode<TValue> Next;
        }

        public class Snapshot : IDisposable
        {
            private readonly ContentStore2 _store;
            private readonly GenRef _genRef;
            private long _gen;
#if DEBUG
            private readonly ILogger _logger;
#endif

            //private static int _count;
            //private readonly int _thisCount;

            internal Snapshot(ContentStore2 store, GenRef genRef
#if DEBUG
                    , ILogger logger
#endif
                )
            {
                _store = store;
                _genRef = genRef;
                _gen = genRef.Gen;
                Interlocked.Increment(ref genRef.GenRefRef.Count);
                //_thisCount = _count++;

#if DEBUG
                _logger = logger;
                _logger.Debug<Snapshot>("Creating snapshot.");
#endif
            }

            public ContentNode Get(int id)
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
                _logger.Debug<Snapshot>("Dispose snapshot (" + _genRef.GenRefRef.Count + ").");
#endif
                _gen = -1;
                Interlocked.Decrement(ref _genRef.GenRefRef.Count);
                GC.SuppressFinalize(this);
            }
        }

        internal class GenRefRef
        {
            public GenRefRef(long gen)
            {
                Gen = gen;
                WGenRef = new WeakReference(null);
            }

            public GenRef GetGenRef()
            {
                // not thread-safe but always invoked from within a lock
                var genRef = (GenRef) WGenRef.Target;
                if (genRef == null)
                    WGenRef.Target = genRef = new GenRef(this, Gen);
                return genRef;
            }

            public readonly long Gen;
            public readonly WeakReference WGenRef;
            public int Count;
        }

        internal class GenRef
        {
            public GenRef(GenRefRef genRefRef, long gen)
            {
                GenRefRef = genRefRef;
                Gen = gen;
            }

            public readonly GenRefRef GenRefRef;
            public readonly long Gen;
        }

        #endregion
    }
}
