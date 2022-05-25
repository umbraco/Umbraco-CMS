using System.Collections.Concurrent;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.PublishedCache.Snap;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

public class SnapDictionary<TKey, TValue>
    where TValue : class
    where TKey : notnull
{
    // minGenDelta to be adjusted
    // we may want to throttle collects even if delta is reached
    // we may want to force collect if delta is not reached but very old
    // we may want to adjust delta depending on the number of changes
    private const long CollectMinGenDelta = 4;

    private readonly ConcurrentQueue<GenObj> _genObjs;

    // read
    // http://www.codeproject.com/Articles/548406/Dictionary-plus-Locking-versus-ConcurrentDictionar
    // http://arbel.net/2013/02/03/best-practices-for-using-concurrentdictionary/
    // http://blogs.msdn.com/b/pfxteam/archive/2011/04/02/10149222.aspx

    // Set, Clear and GetSnapshot have to be protected by a lock
    // This class is optimized for many readers, few writers
    // Readers are lock-free

    // NOTE - we used to lock _rlocko the long hand way with Monitor.Enter(_rlocko, ref lockTaken) but this has
    // been replaced with a normal c# lock because that's exactly how the normal c# lock works,
    // see https://blogs.msdn.microsoft.com/ericlippert/2009/03/06/locks-and-exceptions-do-not-mix/
    // for the readlock, there's no reason here to use the long hand way.
    private readonly ConcurrentDictionary<TKey, LinkedNode<TValue>> _items;
    private readonly object _rlocko = new();
    private readonly object _wlocko = new();
    private Task? _collectTask;
    private GenObj? _genObj;
    private long _liveGen;
    private long _floorGen;
    private bool _nextGen;
    private bool _collectAuto;

    #region Ctor

    public SnapDictionary()
    {
        _items = new ConcurrentDictionary<TKey, LinkedNode<TValue>>();
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
        private readonly long _gen; // copied for perfs
        private readonly GenRef? _genRef;
        private readonly SnapDictionary<TKey, TValue> _store;
        private int _disposed;

        // private static int _count;
        // private readonly int _thisCount;
        internal Snapshot(SnapDictionary<TKey, TValue> store, GenRef genRef)
        {
            _store = store;
            _genRef = genRef;
            _gen = genRef.GenObj.Gen;
            _genRef.GenObj.Reference();

            // _thisCount = _count++;
        }

        internal Snapshot(SnapDictionary<TKey, TValue> store, long gen)
        {
            _store = store;
            _gen = gen;
        }

        public bool IsEmpty
        {
            get
            {
                EnsureNotDisposed();
                return _store.IsEmpty(_gen);
            }
        }

        public long Gen
        {
            get
            {
                EnsureNotDisposed();
                return _gen;
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            {
                return;
            }

            _genRef?.GenObj.Release();
            GC.SuppressFinalize(this);
        }

        private void EnsureNotDisposed()
        {
            if (_disposed > 0)
            {
                throw new ObjectDisposedException("snapshot" /*+ " (" + _thisCount + ")"*/);
            }
        }

        public TValue? Get(TKey key)
        {
            EnsureNotDisposed();
            return _store.Get(key, _gen);
        }

        public IEnumerable<TValue> GetAll()
        {
            EnsureNotDisposed();
            return _store.GetAll(_gen);
        }
    }

    #endregion

    #region Locking

    // read and write locks are not exclusive
    // it is not possible to write-lock while someone is read-locked
    // it is possible to read-lock while someone is write-locked
    //
    // so when getting a read-lock,
    //  either we are write-locked or not, but if not, we won't be write-locked
    //  on the other hand the write-lock may be released in the meantime

    // Lock has a 'forceGen' parameter:
    //  used to start a set of changes that may not commit, to isolate the set from any pending
    //  changes that would not have been snapshotted yet, so they cannot be rolled back by accident
    //
    // Release has a 'commit' parameter:
    //  if false, the live gen is scrapped and changes that have been applied as part of the lock
    //  are all ignored - Release is private and meant to be invoked with 'commit' being false only
    //  only on the outermost lock (by SnapDictionaryWriter)

    // side note - using (...) {} for locking is prone to nasty leaks in case of weird exceptions
    // such as thread-abort or out-of-memory, which is why we've moved away from the old using wrapper we had on locking.
    private readonly string _instanceId = Guid.NewGuid().ToString("N");

    private class WriteLockInfo
    {
        public bool Taken;
    }

    // a scope contextual that represents a locked writer to the dictionary
    private class ScopedWriteLock : ScopeContextualBase
    {
        private readonly SnapDictionary<TKey, TValue> _dictionary;
        private readonly WriteLockInfo _lockinfo = new();

        public ScopedWriteLock(SnapDictionary<TKey, TValue> dictionary, bool scoped)
        {
            _dictionary = dictionary;
            dictionary.Lock(_lockinfo, scoped);
        }

        public override void Release(bool completed) => _dictionary.Release(_lockinfo, completed);
    }

    // gets a scope contextual representing a locked writer to the dictionary
    // the dict is write-locked until the write-lock is released
    //  which happens when it is disposed (non-scoped)
    //  or when the scope context exits (scoped)
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
            // assume everything in finally runs atomically
            // http://stackoverflow.com/questions/18501678/can-this-unexpected-behavior-of-prepareconstrainedregions-and-thread-abort-be-ex
            // http://joeduffyblog.com/2005/03/18/atomicity-and-asynchronous-exception-failures/
            // http://joeduffyblog.com/2007/02/07/introducing-the-new-readerwriterlockslim-in-orcas/
            // http://chabster.blogspot.fr/2013/12/readerwriterlockslim-fails-on-dual.html
            // RuntimeHelpers.PrepareConstrainedRegions();
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
                    _nextGen = true; // this is the ONLY place where _nextGen becomes true
                }
            }
        }
    }

    private void Release(WriteLockInfo lockInfo, bool commit = true)
    {
        // if the lock wasn't taken in the first place, do nothing
        if (!lockInfo.Taken)
        {
            return;
        }

        if (commit == false)
        {
            lock (_rlocko)
            {
                try
                {
                }
                finally
                {
                    // forget about the temp. liveGen
                    _nextGen = false;
                    _liveGen -= 1;
                }
            }

            foreach (KeyValuePair<TKey, LinkedNode<TValue>> item in _items)
            {
                LinkedNode<TValue>? link = item.Value;
                if (link.Gen <= _liveGen)
                {
                    continue;
                }

                TKey key = item.Key;
                if (link.Next == null)
                {
                    _items.TryRemove(key, out link);
                }
                else
                {
                    _items.TryUpdate(key, link.Next, link);
                }
            }
        }

        // TODO: Shouldn't this be in a finally block?
        Monitor.Exit(_wlocko);
    }

    #endregion

    #region Set, Clear, Get, Has

    public int Count => _items.Count;

    private LinkedNode<TValue>? GetHead(TKey key)
    {
        _items.TryGetValue(key, out LinkedNode<TValue>? link); // else null
        return link;
    }

    public void SetLocked(TKey key, TValue? value)
    {
        EnsureLocked();

        // this is safe only because we're write-locked
        LinkedNode<TValue>? link = GetHead(key);
        if (link != null)
        {
            // already in the dict
            if (link.Gen != _liveGen)
            {
                // for an older gen - if value is different then insert a new
                // link for the new gen, with the new value
                if (link.Value != value)
                {
                    _items.TryUpdate(key, new LinkedNode<TValue>(value, _liveGen, link), link);
                }
            }
            else
            {
                // for the live gen - we can fix the live gen - and remove it
                // if value is null and there's no next gen
                if (value == null && link.Next == null)
                {
                    _items.TryRemove(key, out link);
                }
                else
                {
                    link.Value = value;
                }
            }
        }
        else
        {
            _items.TryAdd(key, new LinkedNode<TValue>(value, _liveGen));
        }
    }

    public void ClearLocked(TKey key) => SetLocked(key, null);

    public void ClearLocked()
    {
        EnsureLocked();

        // this is safe only because we're write-locked
        foreach (KeyValuePair<TKey, LinkedNode<TValue>> kvp in _items.Where(x => x.Value != null))
        {
            if (kvp.Value.Gen < _liveGen)
            {
                var link = new LinkedNode<TValue>(null, _liveGen, kvp.Value);
                _items.TryUpdate(kvp.Key, link, kvp.Value);
            }
            else
            {
                kvp.Value.Value = null;
            }
        }
    }

    public TValue? Get(TKey key, long gen)
    {
        // look ma, no lock!
        LinkedNode<TValue>? link = GetHead(key);
        while (link != null)
        {
            if (link.Gen <= gen)
            {
                return link.Value; // may be null
            }

            link = link.Next;
        }

        return null;
    }

    public IEnumerable<TValue> GetAll(long gen)
    {
        // enumerating on .Values locks the concurrent dictionary,
        // so better get a shallow clone in an array and release
        LinkedNode<TValue>[] links = _items.Values.ToArray();
        foreach (LinkedNode<TValue> l in links)
        {
            LinkedNode<TValue>? link = l;
            while (link != null)
            {
                if (link.Gen <= gen)
                {
                    if (link.Value != null)
                    {
                        yield return link.Value;
                    }

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
            LinkedNode<TValue>? link = x.Value;
            while (link != null)
            {
                if (link.Gen <= gen && link.Value != null)
                {
                    return true;
                }

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
        lock (_rlocko)
        {
            // if no next generation is required, and we already have a gen object,
            // use it to create a new snapshot
            if (_nextGen == false && _genObj != null)
            {
                return new Snapshot(this, _genObj.GetGenRef());
            }

            // else we need to try to create a new gen object
            // whether we are wlocked or not, noone can rlock while we do,
            // so _liveGen and _nextGen are safe
            if (Monitor.IsEntered(_wlocko))
            {
                // write-locked, cannot use latest gen (at least 1) so use previous
                var snapGen = _nextGen ? _liveGen - 1 : _liveGen;

                // create a new gen object if we don't already have one
                // (happens the first time a snapshot is created)
                if (_genObj == null)
                {
                    _genObjs.Enqueue(_genObj = new GenObj(snapGen));
                }

                // if we have one already, ensure it's consistent
                else if (_genObj.Gen != snapGen)
                {
                    throw new PanicException(
                        $"The generation {_genObj.Gen} does not equal the snapshot generation {snapGen}");
                }
            }
            else
            {
                // not write-locked, can use latest gen (_liveGen), create a corresponding new gen object
                _genObjs.Enqueue(_genObj = new GenObj(_liveGen));
                _nextGen = false; // this is the ONLY thing that triggers a _liveGen++
            }

            // so...
            // the genObj has a weak ref to the genRef, and is queued
            // the snapshot has a ref to the genRef, which has a ref to the genObj
            // when the snapshot is disposed, it decreases genObj counter
            // so after a while, one of these conditions is going to be true:
            // - genObj.Count is zero because all snapshots have properly been disposed
            // - genObj.WeakGenRef is dead because all snapshots have been collected
            // in both cases, we will dequeue and collect
            var snapshot = new Snapshot(this, _genObj.GetGenRef());

            // reading _floorGen is safe if _collectTask is null
            if (_collectTask == null && _collectAuto && _liveGen - _floorGen > CollectMinGenDelta)
            {
                CollectAsyncLocked();
            }

            return snapshot;
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
        {
            return _collectTask;
        }

        // ReSharper disable InconsistentlySynchronizedField
        Task task = _collectTask = Task.Run(() => Collect());
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
        while (_genObjs.TryPeek(out GenObj? genObj) && (genObj.Count == 0 || genObj.WeakGenRef.IsAlive == false))
        {
            _genObjs.TryDequeue(out genObj); // cannot fail since TryPeek has succeeded
            _floorGen = genObj!.Gen;
        }

        Collect(_items);
    }

    private void Collect(ConcurrentDictionary<TKey, LinkedNode<TValue>> dict)
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

        // Console.WriteLine("Collect live=" + liveGen + " floor=" + _floorGen);
        foreach (KeyValuePair<TKey, LinkedNode<TValue>> kvp in dict)
        {
            LinkedNode<TValue>? link = kvp.Value;

            // Console.WriteLine("Collect id=" + kvp.Key + " gen=" + link.Gen
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
                var idict = dict as ICollection<KeyValuePair<TKey, LinkedNode<TValue>>>;
                /*var removed =*/
                idict.Remove(kvp);

                // Console.WriteLine("remove (" + (removed ? "true" : "false") + ")");
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
    // public /*async*/ Task PendingCollect()
    // {
    //    Task task;
    //    lock (_rlocko)
    //    {
    //        task = _collectTask;
    //    }
    //    return task ?? Task.CompletedTask;
    //    //if (task != null)
    //    //    await task;
    // }
    public long GenCount => _genObjs.Count;

    public long SnapCount => _genObjs.Sum(x => x.Count);

    #endregion

    #region Unit testing

    private TestHelper? _unitTesting;

    // note: nothing here is thread-safe
    internal class TestHelper
    {
        private readonly SnapDictionary<TKey, TValue> _dict;

        public TestHelper(SnapDictionary<TKey, TValue> dict) => _dict = dict;

        public long LiveGen => _dict._liveGen;

        public long FloorGen => _dict._floorGen;

        public bool NextGen => _dict._nextGen;

        public bool IsLocked => Monitor.IsEntered(_dict._wlocko);

        public bool CollectAuto
        {
            get => _dict._collectAuto;
            set => _dict._collectAuto = value;
        }

        public GenObj? GenObj => _dict._genObj;

        public ConcurrentQueue<GenObj> GenObjs => _dict._genObjs;

        public Snapshot LiveSnapshot => new(_dict, _dict._liveGen);

        public GenVal[] GetValues(TKey key)
        {
            _dict._items.TryGetValue(key, out LinkedNode<TValue>? link); // else null

            if (link == null)
            {
                return new GenVal[0];
            }

            var genVals = new List<GenVal>();
            do
            {
                genVals.Add(new GenVal(link.Gen, link.Value));
                link = link.Next;
            }
            while (link != null);

            return genVals.ToArray();
        }

        public class GenVal
        {
            public GenVal(long gen, TValue? value)
            {
                Gen = gen;
                Value = value;
            }

            public long Gen { get; }
            public TValue? Value { get; }
        }
    }

    internal TestHelper Test => _unitTesting ?? (_unitTesting = new TestHelper(this));

    #endregion
}
