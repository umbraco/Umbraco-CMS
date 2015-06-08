using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Caching;
using Umbraco.Core.Logging;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache provider that wraps the logic of a System.Runtime.Caching.ObjectCache
    /// </summary>
    internal class ObjectCacheRuntimeCacheProvider : IRuntimeCacheProvider
    {

        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        internal ObjectCache MemoryCache;

        /// <summary>
        /// Used for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }

        public ObjectCacheRuntimeCacheProvider()
        {
            MemoryCache = new MemoryCache("in-memory");
            InstanceId = Guid.NewGuid();
        }

        #region Clear

        public virtual void ClearAllCache()
        {
            using (new WriteLock(_locker))
            {
                MemoryCache.DisposeIfDisposable();
                MemoryCache = new MemoryCache("in-memory");
            }
        }

        public virtual void ClearCacheItem(string key)
        {
            using (new WriteLock(_locker))
            {
                if (MemoryCache[key] == null) return;
                MemoryCache.Remove(key);
            }            
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
            var type = TypeFinder.GetTypeByName(typeName);
            if (type == null) return;
            var isInterface = type.IsInterface;
            using (new WriteLock(_locker))
            {
                foreach (var key in MemoryCache
                    .Where(x =>
                    {
                        // x.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = DictionaryCacheProviderBase.GetSafeLazyValue((Lazy<object>)x.Value, true);

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return value == null || (isInterface ? (type.IsInstanceOfType(value)) : (value.GetType() == type));
                    })
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
            using (new WriteLock(_locker))
            {
                var typeOfT = typeof (T);
                var isInterface = typeOfT.IsInterface;
                foreach (var key in MemoryCache
                    .Where(x =>
                    {
                        // x.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = DictionaryCacheProviderBase.GetSafeLazyValue((Lazy<object>)x.Value, true);

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return value == null || (isInterface ? (value is T) : (value.GetType() == typeOfT));

                    })
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }
        }

        public virtual void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            using (new WriteLock(_locker))
            {
                var typeOfT = typeof(T);
                var isInterface = typeOfT.IsInterface;
                foreach (var key in MemoryCache
                    .Where(x =>
                    {
                        // x.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = DictionaryCacheProviderBase.GetSafeLazyValue((Lazy<object>)x.Value, true);
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
        }

        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
            using (new WriteLock(_locker))
            {
                foreach (var key in MemoryCache
                    .Where(x => x.Key.InvariantStartsWith(keyStartsWith))
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }            
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
            using (new WriteLock(_locker))
            {
                foreach (var key in MemoryCache
                    .Where(x => Regex.IsMatch(x.Key, regexString))
                    .Select(x => x.Key)
                    .ToArray()) // ToArray required to remove
                    MemoryCache.Remove(key);
            }     
        }

        #endregion

        #region Get

        public IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            KeyValuePair<string, object>[] entries;
            using (new ReadLock(_locker))
            {
                entries = MemoryCache
                    .Where(x => x.Key.InvariantStartsWith(keyStartsWith))
                    .ToArray(); // evaluate while locked
            }
            return entries
                .Select(x => DictionaryCacheProviderBase.GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                .Where(x => x != null) // backward compat, don't store null values in the cache
                .ToList();
        }

        public IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            KeyValuePair<string, object>[] entries;
            using (new ReadLock(_locker))
            {
                entries = MemoryCache
                    .Where(x => Regex.IsMatch(x.Key, regexString))
                    .ToArray(); // evaluate while locked
            }
            return entries
                .Select(x => DictionaryCacheProviderBase.GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                .Where(x => x != null) // backward compat, don't store null values in the cache
                .ToList();
        }

        public object GetCacheItem(string cacheKey)
        {
            Lazy<object> result;
            using (new ReadLock(_locker))
            {
                result = MemoryCache.Get(cacheKey) as Lazy<object>; // null if key not found
            }
            return result == null ? null : DictionaryCacheProviderBase.GetSafeLazyValue(result); // return exceptions as null
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return GetCacheItem(cacheKey, getCacheItem, null);
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal,CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            // see notes in HttpRuntimeCacheProvider

            Lazy<object> result;

            using (var lck = new UpgradeableReadLock(_locker))
            {
                result = MemoryCache.Get(cacheKey) as Lazy<object>;
                if (result == null || DictionaryCacheProviderBase.GetSafeLazyValue(result, true) == null) // get non-created as NonCreatedValue & exceptions as null
                {
                    result = DictionaryCacheProviderBase.GetSafeLazy(getCacheItem);
                    var policy = GetPolicy(timeout, isSliding, removedCallback, dependentFiles);

                    lck.UpgradeToWriteLock();
                    //NOTE: This does an add or update
                    MemoryCache.Set(cacheKey, result, policy);
                }
            }

            //return result.Value;

            var value = result.Value; // will not throw (safe lazy)
            var eh = value as DictionaryCacheProviderBase.ExceptionHolder;
            if (eh != null) throw eh.Exception; // throw once!
            return value;
        }

        #endregion

        #region Insert

        public void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            // NOTE - here also we must insert a Lazy<object> but we can evaluate it right now
            // and make sure we don't store a null value.

            var result = DictionaryCacheProviderBase.GetSafeLazy(getCacheItem);
            var value = result.Value; // force evaluation now
            if (value == null) return; // do not store null values (backward compat)

            var policy = GetPolicy(timeout, isSliding, removedCallback, dependentFiles);
            //NOTE: This does an add or update
            MemoryCache.Set(cacheKey, result, policy);
        }

        #endregion

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
        
    }
}