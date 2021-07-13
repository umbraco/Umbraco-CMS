using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Implements <see cref="IAppPolicyCache"/> on top of a <see cref="System.Web.Caching.Cache"/>.
    /// </summary>
    /// <remarks>The underlying cache is expected to be HttpRuntime.Cache.</remarks>
    internal class WebCachingAppCache : FastDictionaryAppCacheBase, IAppPolicyCache, IDisposable
    {
        // locker object that supports upgradeable read locking
        // does not need to support recursion if we implement the cache correctly and ensure
        // that methods cannot be reentrant, ie we do NOT create values while holding a lock.
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly System.Web.Caching.Cache _cache;
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebCachingAppCache"/> class.
        /// </summary>
        public WebCachingAppCache(System.Web.Caching.Cache cache)
        {
            _cache = cache;
        }

        /// <inheritdoc />
        public override object Get(string key, Func<object> factory)
        {
            return Get(key, factory, null, dependentFiles: null);
        }

        /// <inheritdoc />
        public object Get(string key, Func<object> factory, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            return GetInternal(key, factory, timeout, isSliding, priority, removedCallback, dependentFiles);
        }

        /// <inheritdoc />
        public void Insert(string key, Func<object> factory, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            InsertInternal(key, factory, timeout, isSliding, priority, removedCallback, dependentFiles);
        }

        #region Dictionary

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";
            return _cache.Cast<DictionaryEntry>()
                .Where(x => x.Key is string && ((string)x.Key).StartsWith(prefix));
        }

        protected override void RemoveEntry(string key)
        {
            _cache.Remove(key);
        }

        protected override object GetEntry(string key)
        {
            return _cache.Get(key);
        }

        #endregion

        #region Lock

        protected override void EnterReadLock()
        {
            _locker.EnterReadLock();
        }

        protected override void EnterWriteLock()
        {
            _locker.EnterWriteLock();
        }

        protected override void ExitReadLock()
        {
            if (_locker.IsReadLockHeld)
                _locker.ExitReadLock();
        }

        protected override void ExitWriteLock()
        {
            if (_locker.IsWriteLockHeld)
                _locker.ExitWriteLock();
        }

        #endregion

        private object GetInternal(string key, Func<object> factory, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            key = GetCacheKey(key);

            // NOTE - because we don't know what getCacheItem does, how long it will take and whether it will hang,
            // getCacheItem should run OUTSIDE of the global application lock else we run into lock contention and
            // nasty performance issues.

            // So.... we insert a Lazy<object> in the cache while holding the global application lock, and then rely
            // on the Lazy lock to ensure that getCacheItem runs once and everybody waits on it, while the global
            // application lock has been released.

            // NOTE
            //   The Lazy value creation may produce a null value.
            //   Must make sure (for backward compatibility) that we pretend they are not in the cache.
            //   So if we find an entry in the cache that already has its value created and is null,
            //   pretend it was not there. If value is not already created, wait... and return null, that's
            //   what prior code did.

            // NOTE
            //   The Lazy value creation may throw.

            // So... the null value _will_ be in the cache but never returned

            Lazy<object> result;

            // Fast!
            // Only one thread can enter an UpgradeableReadLock at a time, but it does not prevent other
            // threads to enter a ReadLock in the meantime -- only upgrading to WriteLock will prevent all
            // reads. We first try with a normal ReadLock for maximum concurrency and take the penalty of
            // having to re-lock in case there's no value. Would need to benchmark to figure out whether
            // it's worth it, though...
            try
            {
                _locker.EnterReadLock();
                result = _cache.Get(key) as Lazy<object>; // null if key not found
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
            var value = result == null ? null : GetSafeLazyValue(result);
            if (value != null) return value;

            try
            {
                _locker.EnterUpgradeableReadLock();

                result = _cache.Get(key) as Lazy<object>; // null if key not found

                // cannot create value within the lock, so if result.IsValueCreated is false, just
                // do nothing here - means that if creation throws, a race condition could cause
                // more than one thread to reach the return statement below and throw - accepted.

                if (result == null || GetSafeLazyValue(result, true) == null) // get non-created as NonCreatedValue & exceptions as null
                {
                    result = GetSafeLazy(factory);
                    var absolute = isSliding ? System.Web.Caching.Cache.NoAbsoluteExpiration : (timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
                    var sliding = isSliding == false ? System.Web.Caching.Cache.NoSlidingExpiration : (timeout ?? System.Web.Caching.Cache.NoSlidingExpiration);

                    try
                    {
                        _locker.EnterWriteLock();

                        // create a cache dependency if one is needed. 
                        var dependency = dependentFiles != null && dependentFiles.Length > 0 ? new CacheDependency(dependentFiles) : null;

                        //NOTE: 'Insert' on System.Web.Caching.Cache actually does an add or update!
                        _cache.Insert(key, result, dependency, absolute, sliding, priority, removedCallback);
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

            // using GetSafeLazy and GetSafeLazyValue ensures that we don't cache
            // exceptions (but try again and again) and silently eat them - however at
            // some point we have to report them - so need to re-throw here

            // this does not throw anymore
            //return result.Value;

            value = result.Value; // will not throw (safe lazy)
            if (value is ExceptionHolder eh) eh.Exception.Throw(); // throw once!
            return value;
        }

        private void InsertInternal(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            // NOTE - here also we must insert a Lazy<object> but we can evaluate it right now
            // and make sure we don't store a null value.

            var result = GetSafeLazy(getCacheItem);
            var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
            if (value == null) return; // do not store null values (backward compat)

            cacheKey = GetCacheKey(cacheKey);

            var absolute = isSliding ? System.Web.Caching.Cache.NoAbsoluteExpiration : (timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
            var sliding = isSliding == false ? System.Web.Caching.Cache.NoSlidingExpiration : (timeout ?? System.Web.Caching.Cache.NoSlidingExpiration);

            try
            {
                _locker.EnterWriteLock();

                // create a cache dependency if one is needed. 
                var dependency = dependentFiles != null && dependentFiles.Length > 0 ? new CacheDependency(dependentFiles) : null;

                //NOTE: 'Insert' on System.Web.Caching.Cache actually does an add or update!
                _cache.Insert(cacheKey, result, dependency, absolute, sliding, priority, removedCallback);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
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
