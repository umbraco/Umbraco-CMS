using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Caching;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A CacheProvider that wraps the logic of the HttpRuntime.Cache
    /// </summary>
    internal class HttpRuntimeCacheProvider : DictionaryCacheProviderBase, IRuntimeCacheProvider
    {
        // locker object that supports upgradeable read locking
        // does not need to support recursion if we implement the cache correctly and ensure
        // that methods cannot be reentrant, ie we do NOT create values while holding a lock.
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly System.Web.Caching.Cache _cache;

        /// <summary>
        /// Used for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }

        public HttpRuntimeCacheProvider(System.Web.Caching.Cache cache)
        {
            _cache = cache;
            InstanceId = Guid.NewGuid();
        }

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";
            return _cache.Cast<DictionaryEntry>()
                .Where(x => x.Key is string && ((string) x.Key).StartsWith(prefix));
        }

        protected override void RemoveEntry(string key)
        {
            _cache.Remove(key);
        }

        protected override object GetEntry(string key)
        {
            return _cache.Get(key);
        }

        #region Lock

        protected override IDisposable ReadLock
        {
            get { return new ReadLock(_locker); }
        }

        protected override IDisposable WriteLock
        {
            get { return new WriteLock(_locker); }
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with all of the default parameters
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public override object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return GetCacheItem(cacheKey, getCacheItem, null, dependentFiles: null);
        }

        /// <summary>
        /// This overload is here for legacy purposes
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <param name="timeout"></param>
        /// <param name="isSliding"></param>
        /// <param name="priority"></param>
        /// <param name="removedCallback"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        internal object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, CacheDependency dependency = null)
        {
            cacheKey = GetCacheKey(cacheKey);

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
            using (new ReadLock(_locker))
            {
                result = _cache.Get(cacheKey) as Lazy<object>; // null if key not found
            }
            var value = result == null ? null : GetSafeLazyValue(result);
            if (value != null) return value;

            using (var lck = new UpgradeableReadLock(_locker))
            {
                result = _cache.Get(cacheKey) as Lazy<object>; // null if key not found

                // cannot create value within the lock, so if result.IsValueCreated is false, just
                // do nothing here - means that if creation throws, a race condition could cause
                // more than one thread to reach the return statement below and throw - accepted.

                if (result == null || GetSafeLazyValue(result, true) == null) // get non-created as NonCreatedValue & exceptions as null
                {
                    result = GetSafeLazy(getCacheItem);
                    var absolute = isSliding ? System.Web.Caching.Cache.NoAbsoluteExpiration : (timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
                    var sliding = isSliding == false ? System.Web.Caching.Cache.NoSlidingExpiration : (timeout ?? System.Web.Caching.Cache.NoSlidingExpiration);

                    lck.UpgradeToWriteLock();
                    //NOTE: 'Insert' on System.Web.Caching.Cache actually does an add or update!
                    _cache.Insert(cacheKey, result, dependency, absolute, sliding, priority, removedCallback);
                }
            }

            // using GetSafeLazy and GetSafeLazyValue ensures that we don't cache
            // exceptions (but try again and again) and silently eat them - however at
            // some point we have to report them - so need to re-throw here

            // this does not throw anymore
            //return result.Value;

            value = result.Value; // will not throw (safe lazy)
            var eh = value as ExceptionHolder;
            if (eh != null) throw eh.Exception; // throw once!
            return value;
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            CacheDependency dependency = null;
            if (dependentFiles != null && dependentFiles.Any())
            {
                dependency = new CacheDependency(dependentFiles);
            }
            return GetCacheItem(cacheKey, getCacheItem, timeout, isSliding, priority, removedCallback, dependency);
        }

        #endregion

        #region Insert

        /// <summary>
        /// This overload is here for legacy purposes
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <param name="timeout"></param>
        /// <param name="isSliding"></param>
        /// <param name="priority"></param>
        /// <param name="removedCallback"></param>
        /// <param name="dependency"></param>
        internal void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, CacheDependency dependency = null)
        {
            // NOTE - here also we must insert a Lazy<object> but we can evaluate it right now
            // and make sure we don't store a null value.

            var result = GetSafeLazy(getCacheItem);
            var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
            if (value == null) return; // do not store null values (backward compat)

            cacheKey = GetCacheKey(cacheKey);           

            var absolute = isSliding ? System.Web.Caching.Cache.NoAbsoluteExpiration : (timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
            var sliding = isSliding == false ? System.Web.Caching.Cache.NoSlidingExpiration : (timeout ?? System.Web.Caching.Cache.NoSlidingExpiration);

            using (new WriteLock(_locker))
            {
                //NOTE: 'Insert' on System.Web.Caching.Cache actually does an add or update!
                _cache.Insert(cacheKey, result, dependency, absolute, sliding, priority, removedCallback);
            }
        }

        public void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            CacheDependency dependency = null;
            if (dependentFiles != null && dependentFiles.Any())
            {
                dependency = new CacheDependency(dependentFiles);
            }
            InsertCacheItem(cacheKey, getCacheItem, timeout, isSliding, priority, removedCallback, dependency);
        }

        #endregion
    }
}