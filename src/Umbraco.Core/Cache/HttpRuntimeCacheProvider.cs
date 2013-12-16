using System;
using System.Web;
using System.Web.Caching;
using Umbraco.Core.Logging;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A CacheProvider that wraps the logic of the HttpRuntime.Cache
    /// </summary>
    internal class HttpRuntimeCacheProvider : DictionaryCacheProdiverBase, IRuntimeCacheProvider
    {
        private readonly System.Web.Caching.Cache _cache;
        private readonly DictionaryCacheWrapper _wrapper;
        private static readonly object Locker = new object();
        
        public HttpRuntimeCacheProvider(System.Web.Caching.Cache cache)
        {
            _cache = cache;
            _wrapper = new DictionaryCacheWrapper(_cache, s => _cache.Get(s.ToString()), o => _cache.Remove(o.ToString()));
        }

        protected override DictionaryCacheWrapper DictionaryCache
        /// Clears all objects in the System.Web.Cache with the System.Type specified that satisfy the predicate
        /// </summary>
        public override void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            try
            {
                lock (Locker)
                {
                    foreach (DictionaryEntry c in _cache)
                    {
                        var key = c.Key.ToString();
                        if (_cache[key] != null
                            && _cache[key] is T
                            && predicate(key, (T)_cache[key]))
                        {
                            _cache.Remove(c.Key.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error<CacheHelper>("Cache clearing error", e);
            }
        }

        /// <summary>
        {
            get { return _wrapper; }
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with all of the default parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public override T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            return GetCacheItem(cacheKey, CacheItemPriority.Normal, null, null, null, getCacheItem, Locker);
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with the specified absolute expiration date (from NOW)
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public virtual TT GetCacheItem<TT>(string cacheKey,
                                            TimeSpan? timeout, Func<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, null, timeout, getCacheItem);
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with the specified absolute expiration date (from NOW)
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="refreshAction"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public virtual TT GetCacheItem<TT>(string cacheKey,
                                            CacheItemRemovedCallback refreshAction, TimeSpan? timeout,
                                            Func<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, CacheItemPriority.Normal, refreshAction, timeout, getCacheItem);
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with the specified absolute expiration date (from NOW)
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="refreshAction"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public virtual TT GetCacheItem<TT>(string cacheKey,
                                            CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan? timeout,
                                            Func<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, priority, refreshAction, null, timeout, getCacheItem);
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with the specified absolute expiration date (from NOW)
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="refreshAction"></param>
        /// <param name="cacheDependency"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public virtual TT GetCacheItem<TT>(string cacheKey,
                                            CacheItemPriority priority,
                                            CacheItemRemovedCallback refreshAction,
                                            CacheDependency cacheDependency,
                                            TimeSpan? timeout,
                                            Func<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem, Locker);
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with the specified absolute expiration date (from NOW)
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="refreshAction"></param>
        /// <param name="cacheDependency"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        /// <param name="syncLock"></param>
        /// <returns></returns>
        private TT GetCacheItem<TT>(string cacheKey,
                                     CacheItemPriority priority, CacheItemRemovedCallback refreshAction,
                                     CacheDependency cacheDependency, TimeSpan? timeout, Func<TT> getCacheItem, object syncLock)
        {
            cacheKey = GetCacheKey(cacheKey);

            var result = DictionaryCache.Get(cacheKey);
            if (result == null)
            {
                lock (syncLock)
                {
                    result = DictionaryCache.Get(cacheKey);
                    if (result == null)
                    {
                        result = getCacheItem();
                        if (result != null)
                        {
                            //we use Insert instead of add if for some crazy reason there is now a cache with the cache key in there, it will just overwrite it.
                            _cache.Insert(cacheKey, result, cacheDependency,
                                          timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value),
                                          TimeSpan.Zero, priority, refreshAction);
                        }
                    }
                }
            }
            return result.TryConvertTo<TT>().Result;
        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="getCacheItem"></param>
        public virtual void InsertCacheItem<T>(string cacheKey,
                                                CacheItemPriority priority,
                                                Func<T> getCacheItem)
        {
            InsertCacheItem(cacheKey, priority, null, null, null, getCacheItem);
        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        public virtual void InsertCacheItem<T>(string cacheKey,
                                                CacheItemPriority priority,
                                                TimeSpan? timeout,
                                                Func<T> getCacheItem)
        {
            InsertCacheItem(cacheKey, priority, null, null, timeout, getCacheItem);
        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="cacheDependency"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        public virtual void InsertCacheItem<T>(string cacheKey,
                                                CacheItemPriority priority,
                                                CacheDependency cacheDependency,
                                                TimeSpan? timeout,
                                                Func<T> getCacheItem)
        {
            InsertCacheItem(cacheKey, priority, null, cacheDependency, timeout, getCacheItem);
        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="refreshAction"></param>
        /// <param name="cacheDependency"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        public virtual void InsertCacheItem<T>(string cacheKey,
                                                CacheItemPriority priority,
                                                CacheItemRemovedCallback refreshAction,
                                                CacheDependency cacheDependency,
                                                TimeSpan? timeout,
                                                Func<T> getCacheItem)
        {
            object result = getCacheItem();
            if (result != null)
            {
                cacheKey = GetCacheKey(cacheKey);

                //we use Insert instead of add if for some crazy reason there is now a cache with the cache key in there, it will just overwrite it.
                _cache.Insert(cacheKey, result, cacheDependency,
                              timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value),
                              TimeSpan.Zero, priority, refreshAction);
            }
        }

    }
}