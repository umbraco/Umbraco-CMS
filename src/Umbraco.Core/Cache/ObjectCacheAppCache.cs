using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Caching;
using Umbraco.Core.Composing;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Implements <see cref="IAppPolicyCache"/> on top of a <see cref="ObjectCache"/>.
    /// </summary>
    public class ObjectCacheAppCache : IAppPolicyCache, IDisposable
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectCacheAppCache"/>.
        /// </summary>
        public ObjectCacheAppCache()
        {
            // the MemoryCache is created with name "in-memory". That name is
            // used to retrieve configuration options. It does not identify the memory cache, i.e.
            // each instance of this class has its own, independent, memory cache.
            MemoryCache = new MemoryCache("in-memory");
        }

        /// <summary>
        /// Gets the internal memory cache, for tests only!
        /// </summary>
        internal ObjectCache MemoryCache { get; private set; }

        /// <inheritdoc />
        public object Get(string key)
        {
            Lazy<object> result;
            try
            {
                _locker.EnterReadLock();
                result = MemoryCache.Get(key) as Lazy<object>; // null if key not found
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
            return result == null ? null : FastDictionaryAppCacheBase.GetSafeLazyValue(result); // return exceptions as null
        }

        /// <inheritdoc />
        public object Get(string key, Func<object> factory)
        {
            return Get(key, factory, null);
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByKey(string keyStartsWith)
        {
            KeyValuePair<string, object>[] entries;
            try
            {
                _locker.EnterReadLock();
                entries = MemoryCache
                    .Where(x => x.Key.InvariantStartsWith(keyStartsWith))
                    .ToArray(); // evaluate while locked
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
            return entries
                .Select(x => FastDictionaryAppCacheBase.GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                .Where(x => x != null) // backward compat, don't store null values in the cache
                .ToList();
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);

            KeyValuePair<string, object>[] entries;
            try
            {
                _locker.EnterReadLock();
                entries = MemoryCache
                    .Where(x => compiled.IsMatch(x.Key))
                    .ToArray(); // evaluate while locked
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
            return entries
                .Select(x => FastDictionaryAppCacheBase.GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                .Where(x => x != null) // backward compat, don't store null values in the cache
                .ToList();
        }

        /// <inheritdoc />
        public object Get(string key, Func<object> factory, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            // see notes in HttpRuntimeAppCache

            Lazy<object> result;

            try
            {
                _locker.EnterUpgradeableReadLock();

                result = MemoryCache.Get(key) as Lazy<object>;
                if (result == null || FastDictionaryAppCacheBase.GetSafeLazyValue(result, true) == null) // get non-created as NonCreatedValue & exceptions as null
                {
                    result = FastDictionaryAppCacheBase.GetSafeLazy(factory);
                    var policy = GetPolicy(timeout, isSliding, removedCallback, dependentFiles);

                    try
                    {
                        _locker.EnterWriteLock();
                        //NOTE: This does an add or update
                        MemoryCache.Set(key, result, policy);
                    }
                    finally
                    {
                        if (_locker.IsWriteLockHeld)
                            _locker.ExitWriteLock();
                    }
                }
            }
            finally
            {
                if (_locker.IsUpgradeableReadLockHeld)
                    _locker.ExitUpgradeableReadLock();
            }

            //return result.Value;

            var value = result.Value; // will not throw (safe lazy)
            if (value is FastDictionaryAppCacheBase.ExceptionHolder eh) eh.Exception.Throw(); // throw once!
            return value;
        }

        /// <inheritdoc />
        public void Insert(string key, Func<object> factory, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            // NOTE - here also we must insert a Lazy<object> but we can evaluate it right now
            // and make sure we don't store a null value.

            var result = FastDictionaryAppCacheBase.GetSafeLazy(factory);
            var value = result.Value; // force evaluation now
            if (value == null) return; // do not store null values (backward compat)

            var policy = GetPolicy(timeout, isSliding, removedCallback, dependentFiles);
            //NOTE: This does an add or update
            MemoryCache.Set(key, result, policy);
        }

        /// <inheritdoc />
        public virtual void Clear()
        {
            try
            {
                _locker.EnterWriteLock();
                MemoryCache.DisposeIfDisposable();
                MemoryCache = new MemoryCache("in-memory");
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public virtual void Clear(string key)
        {
            try
            {
                _locker.EnterWriteLock();
                if (MemoryCache[key] == null) return;
                MemoryCache.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public virtual void ClearOfType(string typeName)
        {
            var type = TypeFinder.GetTypeByName(typeName);
            if (type == null) return;
            var isInterface = type.IsInterface;
            try
            {
                _locker.EnterWriteLock();
                foreach (var key in MemoryCache
                    .Where(x =>
                    {
                        // x.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = FastDictionaryAppCacheBase.GetSafeLazyValue((Lazy<object>)x.Value, true);

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return value == null || (isInterface ? (type.IsInstanceOfType(value)) : (value.GetType() == type));
                    })
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public virtual void ClearOfType<T>()
        {
            try
            {
                _locker.EnterWriteLock();
                var typeOfT = typeof(T);
                var isInterface = typeOfT.IsInterface;
                foreach (var key in MemoryCache
                    .Where(x =>
                    {
                        // x.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = FastDictionaryAppCacheBase.GetSafeLazyValue((Lazy<object>)x.Value, true);

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return value == null || (isInterface ? (value is T) : (value.GetType() == typeOfT));

                    })
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public virtual void ClearOfType<T>(Func<string, T, bool> predicate)
        {
            try
            {
                _locker.EnterWriteLock();
                var typeOfT = typeof(T);
                var isInterface = typeOfT.IsInterface;
                foreach (var key in MemoryCache
                    .Where(x =>
                    {
                        // x.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = FastDictionaryAppCacheBase.GetSafeLazyValue((Lazy<object>)x.Value, true);
                        if (value == null) return true;

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return (isInterface ? (value is T) : (value.GetType() == typeOfT))
                               && predicate(x.Key, (T)value);
                    })
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public virtual void ClearByKey(string keyStartsWith)
        {
            try
            {
                _locker.EnterWriteLock();
                foreach (var key in MemoryCache
                    .Where(x => x.Key.InvariantStartsWith(keyStartsWith))
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public virtual void ClearByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);

            try
            {
                _locker.EnterWriteLock();
                foreach (var key in MemoryCache
                    .Where(x => compiled.IsMatch(x.Key))
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        private static CacheItemPolicy GetPolicy(TimeSpan? timeout = null, bool isSliding = false, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            var absolute = isSliding ? ObjectCache.InfiniteAbsoluteExpiration : (timeout == null ? ObjectCache.InfiniteAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
            var sliding = isSliding == false ? ObjectCache.NoSlidingExpiration : (timeout ?? ObjectCache.NoSlidingExpiration);

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absolute,
                SlidingExpiration = sliding
            };

            if (dependentFiles != null && dependentFiles.Any())
            {
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(dependentFiles.ToList()));
            }

            if (removedCallback != null)
            {
                policy.RemovedCallback = arguments =>
                {
                    //convert the reason
                    var reason = CacheItemRemovedReason.Removed;
                    switch (arguments.RemovedReason)
                    {
                        case CacheEntryRemovedReason.Removed:
                            reason = CacheItemRemovedReason.Removed;
                            break;
                        case CacheEntryRemovedReason.Expired:
                            reason = CacheItemRemovedReason.Expired;
                            break;
                        case CacheEntryRemovedReason.Evicted:
                            reason = CacheItemRemovedReason.Underused;
                            break;
                        case CacheEntryRemovedReason.ChangeMonitorChanged:
                            reason = CacheItemRemovedReason.Expired;
                            break;
                        case CacheEntryRemovedReason.CacheSpecificEviction:
                            reason = CacheItemRemovedReason.Underused;
                            break;
                    }
                    //call the callback
                    removedCallback(arguments.CacheItem.Key, arguments.CacheItem.Value, reason);
                };
            }
            return policy;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _locker.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
