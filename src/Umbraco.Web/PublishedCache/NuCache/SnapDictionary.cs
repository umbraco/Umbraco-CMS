using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Scoping;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class SnapDictionary<TKey, TValue>
        where TValue : class
    {
        // read
        // http://www.codeproject.com/Articles/548406/Dictionary-plus-Locking-versus-ConcurrentDictionar
        // http://arbel.net/2013/02/03/best-practices-for-using-concurrentdictionary/
        // http://blogs.msdn.com/b/pfxteam/archive/2011/04/02/10149222.aspx

        // Set, Clear and GetSnapshot have to be protected by a lock
        // This class is optimized for many readers, few writers
        // Readers are lock-free

        private readonly ConcurrentDictionary<TKey, LinkedNode> _items;
        private readonly ConcurrentQueue<GenerationObject> _generationObjects;
        private GenerationObject _generationObject;
        private readonly object _wlocko = new object();
        private readonly object _rlocko = new object();
        private long _liveGen, _floorGen;
        private bool _nextGen, _collectAuto;
        private Task _collectTask;
        private volatile int _wlocked;

        // fixme - collection trigger (ok for now)
        // minGenDelta to be adjusted
        // we may want to throttle collects even if delta is reached
        // we may want to force collect if delta is not reached but very old
        // we may want to adjust delta depending on the number of changes
        private const long CollectMinGenDelta = 4;

        #region Ctor

        public SnapDictionary()
        {
            _items = new ConcurrentDictionary<TKey, LinkedNode>();
            _generationObjects = new ConcurrentQueue<GenerationObject>();
            _generationObject = null; // no initial gen exists
            _liveGen = _floorGen = 0;
            _nextGen = false; // first time, must create a snapshot
            _collectAuto = true; // collect automatically by default
        }

        #endregion

        #region Locking

        // read and write locks are not exclusive
        // it is not possible to write-lock while someone is read-locked
        // it is possible to read-lock while someone is write-locked
        //
        // so when getting a read-lock,
        //  either we are write-locked or not, but if not, we won't be write-locked
        //  otoh the write-lock may be released in the meantime

        // Lock has a 'forceGen' parameter:
        //  used to start a set of changes that may not commit, to isolate the set from any pending
        //  changes that would not have been snapshotted yet, so they cannot be rolled back by accident
        //
        // Release has a 'commit' parameter:
        //  if false, the live gen is scrapped and changes that have been applied as part of the lock
        //  are all ignored - Release is private and meant to be invoked with 'commit' being false only
        //  only on the outermost lock (by SnapDictionaryWriter)

        // using (...) {} for locking is prone to nasty leaks in case of weird exceptions
        // such as thread-abort or out-of-memory, but let's not worry about it now

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
        private class SnapDictionaryWriter : ScopeContextualBase
        {
            private readonly WriteLockInfo _lockinfo = new WriteLockInfo();
            private SnapDictionary<TKey, TValue> _dictionary;

            public SnapDictionaryWriter(SnapDictionary<TKey, TValue> dictionary, bool scoped)
            {
                _dictionary = dictionary;
                dictionary.Lock(_lockinfo, scoped);
            }

            public override void Release(bool completed)
            {
                if (_dictionary == null) return;
                _dictionary.Release(_lockinfo, completed);
                _dictionary = null;
            }
        }

        // gets a scope contextual representing a locked writer to the dictionary
        // fixme GetScopedWriter? should the dict have a ref onto the scope provider?
        public IDisposable GetWriter(IScopeProvider scopeProvider)
        {
            return ScopeContextualBase.Get(scopeProvider, _instanceId, scoped => new SnapDictionaryWriter(this, scoped));
        }

        private void Lock(WriteLockInfo lockInfo, bool forceGen = false)
        {
            Monitor.Enter(_wlocko, ref lockInfo.Taken);

            var rtaken = false;
            try
            {
                Monitor.Enter(_rlocko, ref rtaken);

                // assume everything in finally runs atomically
                // http://stackoverflow.com/questions/18501678/can-this-unexpected-behavior-of-prepareconstrainedregions-and-thread-abort-be-ex
                // http://joeduffyblog.com/2005/03/18/atomicity-and-asynchronous-exception-failures/
                // http://joeduffyblog.com/2007/02/07/introducing-the-new-readerwriterlockslim-in-orcas/
                // http://chabster.blogspot.fr/2013/12/readerwriterlockslim-fails-on-dual.html
                //RuntimeHelpers.PrepareConstrainedRegions();
                try { } finally
                {
                    _wlocked++;
                    lockInfo.Count = true;
                    if (_nextGen == false || (forceGen && _wlocked == 1)) // if true already... ok to have "holes" in generation objects
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
                    try { } finally
                    {
                        _nextGen = false;
                        _liveGen -= 1;
                    }
                }
                finally
                {
                    if (rtaken) Monitor.Exit(_rlocko);
                }

                foreach (var item in _items)
                {
                    var link = item.Value;
                    if (link.Gen <= _liveGen) continue;

                    var key = item.Key;
                    if (link.Next == null)
                        _items.TryRemove(key, out link);
                    else
                        _items.TryUpdate(key, link.Next, link);
                }
            }

            if (lockInfo.Count) _wlocked--;
            if (lockInfo.Taken) Monitor.Exit(_wlocko);
        }

        private void Release(ReadLockInfo lockInfo)
        {
            if (lockInfo.Taken) Monitor.Exit(_rlocko);
        }

        #endregion

        #region Set, Clear, Get, Has

        public int Count => _items.Count;

        private LinkedNode GetHead(TKey key)
        {
            _items.TryGetValue(key, out LinkedNode link); // else null
            return link;
        }

        public void Set(TKey key, TValue value)
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                // this is safe only because we're write-locked
                var link = GetHead(key);
                if (link != null)
                {
                    // already in the dict
                    if (link.Gen != _liveGen)
                    {
                        // for an older gen - if value is different then insert a new
                        // link for the new gen, with the new value
                        if (link.Value != value)
                            _items.TryUpdate(key, new LinkedNode(value, _liveGen, link), link);
                    }
                    else
                    {
                        // for the live gen - we can fix the live gen - and remove it
                        // if value is null and there's no next gen
                        if (value == null && link.Next == null)
                            _items.TryRemove(key, out link);
                        else
                            link.Value = value;
                    }
                }
                else
                {
                    _items.TryAdd(key, new LinkedNode(value, _liveGen));
                }
            }
            finally
            {
                Release(lockInfo);
            }
        }

        public void Clear(TKey key)
        {
            Set(key, null);
        }

        public void Clear()
        {
            var lockInfo = new WriteLockInfo();
            try
            {
                Lock(lockInfo);

                // this is safe only because we're write-locked
                foreach (var kvp in _items.Where(x => x.Value != null))
                {
                    if (kvp.Value.Gen < _liveGen)
                    {
                        var link = new LinkedNode(null, _liveGen, kvp.Value);
                        _items.TryUpdate(kvp.Key, link, kvp.Value);
                    }
                    else
                    {
                        kvp.Value.Value = null;
                    }
                }
            }
            finally
            {
                Release(lockInfo);
            }
        }

        public TValue Get(TKey key, long gen)
        {
            // look ma, no lock!
            var link = GetHead(key);
            while (link != null)
            {
                if (link.Gen <= gen)
                    return link.Value; // may be null
                link = link.Next;
            }
            return null;
        }

        public IEnumerable<TValue> GetAll(long gen)
        {
            // enumerating on .Values locks the concurrent dictionary,
            // so better get a shallow clone in an array and release
            var links = _items.Values.ToArray();
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
            var has = _items.Any(x =>
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
                if (_nextGen == false && _generationObject != null)
                    return new Snapshot(this, _generationObject.GetReference());

                // else we need to try to create a new gen ref
                // whether we are wlocked or not, noone can rlock while we do,
                // so _liveGen and _nextGen are safe
                if (_wlocked > 0) // volatile, cannot ++ but could --
                {
                    // write-locked, cannot use latest gen (at least 1) so use previous
                    var snapGen = _nextGen ? _liveGen - 1 : _liveGen;

                    // create a new gen ref unless we already have it
                    if (_generationObject == null)
                        _generationObjects.Enqueue(_generationObject = new GenerationObject(snapGen));
                    else if (_generationObject.Gen != snapGen)
                        throw new Exception("panic");
                }
                else
                {
                    // not write-locked, can use latest gen, create a new gen ref
                    _generationObjects.Enqueue(_generationObject = new GenerationObject(_liveGen));
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

                var snapshot = new Snapshot(this, _generationObject.GetReference());

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
            while (_generationObjects.TryPeek(out GenerationObject generationObject) && (generationObject.Count == 0 || generationObject.WeakReference.IsAlive == false))
            {
                _generationObjects.TryDequeue(out generationObject); // cannot fail since TryPeek has succeeded
                _floorGen = generationObject.Gen;
            }

            Collect(_items);
        }

        private void Collect(ConcurrentDictionary<TKey, LinkedNode> dict)
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

            //Console.WriteLine("Collect live=" + liveGen + " floor=" + _floorGen);

            foreach (var kvp in dict)
            {
                var link = kvp.Value;

                //Console.WriteLine("Collect id=" + kvp.Key + " gen=" + link.Gen
                //    + " nxt=" + (link.Next == null ? null : "next")
                //    + " val=" + link.Value);

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
                    var idict = dict as ICollection<KeyValuePair<TKey, LinkedNode>>;
                    /*var removed =*/ idict.Remove(kvp);
                    //Console.WriteLine("remove (" + (removed ? "true" : "false") + ")");
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

        public /*async*/ Task PendingCollect()
        {
            Task task;
            lock (_rlocko)
            {
                task = _collectTask;
            }
            return task ?? Task.FromResult(0);
            //if (task != null)
            //    await task;
        }

        public long GenCount => _generationObjects.Count;

        public long SnapCount => _generationObjects.Sum(x => x.Count);

        #endregion

        #region Unit testing

        private TestHelper _unitTesting;

        // note: nothing here is thread-safe
        internal class TestHelper
        {
            private readonly SnapDictionary<TKey, TValue> _dict;

            public TestHelper(SnapDictionary<TKey, TValue> dict)
            {
                _dict = dict;
            }

            public long LiveGen => _dict._liveGen;
            public long FloorGen => _dict._floorGen;
            public bool NextGen => _dict._nextGen;

            public bool CollectAuto
            {
                get => _dict._collectAuto;
                set => _dict._collectAuto = value;
            }

            public ConcurrentQueue<GenerationObject> GenerationObjects => _dict._generationObjects;

            public Snapshot LiveSnapshot => new Snapshot(_dict, _dict._liveGen);

            public GenVal[] GetValues(TKey key)
            {
                _dict._items.TryGetValue(key, out LinkedNode link); // else null

                if (link == null)
                    return new GenVal[0];

                var genVals = new List<GenVal>();
                do
                {
                    genVals.Add(new GenVal(link.Gen, link.Value));
                    link = link.Next;
                } while (link != null);
                return genVals.ToArray();
            }

            public class GenVal
            {
                public GenVal(long gen, TValue value)
                {
                    Gen = gen;
                    Value = value;
                }

                public long Gen { get; }
                public TValue Value { get; }
            }
        }

        internal TestHelper Test => _unitTesting ?? (_unitTesting = new TestHelper(this));

        #endregion

        #region Classes

        private class LinkedNode
        {
            public LinkedNode(TValue value, long gen, LinkedNode next = null)
            {
                Value = value;
                Gen = gen;
                Next = next;
            }

            internal readonly long Gen;

            // reading & writing references is thread-safe on all .NET platforms
            // mark as volatile to ensure we always read the correct value
            internal volatile TValue Value;
            internal volatile LinkedNode Next;
        }

        public class Snapshot : IDisposable
        {
            private readonly SnapDictionary<TKey, TValue> _store;
            private readonly GenerationReference _generationReference;
            private long _gen; // copied for perfs

            internal Snapshot(SnapDictionary<TKey, TValue> store, GenerationReference generationReference)
            {
                _store = store;
                _generationReference = generationReference;
                _gen = generationReference.GenerationObject.Gen;
                _generationReference.GenerationObject.Reference();
            }

            internal Snapshot(SnapDictionary<TKey, TValue> store, long gen)
            {
                _store = store;
                _gen = gen;
            }

            public TValue Get(TKey key)
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.Get(key, _gen);
            }

            public IEnumerable<TValue> GetAll()
            {
                if (_gen < 0)
                    throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
                return _store.GetAll(_gen);
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
                _gen = -1;
                _generationReference?.GenerationObject.Release();
                GC.SuppressFinalize(this);
            }
        }

        internal class GenerationObject
        {
            public GenerationObject(long gen)
            {
                Gen = gen;
                WeakReference = new WeakReference(null);
            }

            public GenerationReference GetReference()
            {
                // not thread-safe but always invoked from within a lock
                var generationReference = (GenerationReference) WeakReference.Target;
                if (generationReference == null)
                    WeakReference.Target = generationReference = new GenerationReference(this);
                return generationReference;
            }

            public readonly long Gen;
            public readonly WeakReference WeakReference;
            public int Count;

            public void Reference()
            {
                Interlocked.Increment(ref Count);
            }

            public void Release()
            {
                Interlocked.Decrement(ref Count);
            }
        }

        internal class GenerationReference
        {
            public GenerationReference(GenerationObject generationObject)
            {
                GenerationObject = generationObject;
            }

            public readonly GenerationObject GenerationObject;
        }

        #endregion
    }
}
