using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A CacheProvider that wraps the logic of the HttpRuntime.Cache
    /// </summary>
    internal class HttpRuntimeCacheProvider : RuntimeCacheProviderBase
    {
        private readonly System.Web.Caching.Cache _cache;
        private static readonly object Locker = new object();

        public HttpRuntimeCacheProvider(System.Web.Caching.Cache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Clears everything in umbraco's runtime cache, which means that not only
        /// umbraco content is removed, but also other cache items from pages running in
        /// the same application / website. Use with care :-)
        /// </summary>
        public override void ClearAllCache()
        {
            var cacheEnumerator = _cache.GetEnumerator();
            while (cacheEnumerator.MoveNext())
            {
                _cache.Remove(cacheEnumerator.Key.ToString());
            }
        }

        /// <summary>
        /// Clears the item in umbraco's runtime cache with the given key 
        /// </summary>
        /// <param name="key">Key</param>
        public override void ClearCacheItem(string key)
        {
            // NH 10 jan 2012
            // Patch by the always wonderful Stéphane Gay to avoid cache null refs
            lock (Locker)
            {
                if (_cache[key] == null) return;
                _cache.Remove(key); ;
            }
        }


        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type name as the
        /// input parameter. (using [object].GetType())
        /// </summary>
        /// <param name="typeName">The name of the System.Type which should be cleared from cache ex "System.Xml.XmlDocument"</param>
        public override void ClearCacheObjectTypes(string typeName)
        {
            try
            {
                lock (Locker)
                {
                    foreach (DictionaryEntry c in _cache)
                    {
                        if (_cache[c.Key.ToString()] != null
                            && _cache[c.Key.ToString()].GetType().ToString().InvariantEquals(typeName))
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
        /// Clears all objects in the System.Web.Cache with the System.Type specified
        /// </summary>
        public override void ClearCacheObjectTypes<T>()
        {
            try
            {
                lock (Locker)
                {
                    foreach (DictionaryEntry c in _cache)
                    {
                        if (_cache[c.Key.ToString()] != null
                            && _cache[c.Key.ToString()].GetType() == typeof(T))
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
        /// Clears all cache items that starts with the key passed.
        /// </summary>
        /// <param name="keyStartsWith">The start of the key</param>
        public override void ClearCacheByKeySearch(string keyStartsWith)
        {
            foreach (DictionaryEntry c in _cache)
            {
                if (c.Key is string && ((string)c.Key).InvariantStartsWith(keyStartsWith))
                {
                    ClearCacheItem((string)c.Key);
                }
            }
        }

        /// <summary>
        /// Clears all cache items that have a key that matches the regular expression
        /// </summary>
        /// <param name="regexString"></param>
        public override void ClearCacheByKeyExpression(string regexString)
        {
            foreach (DictionaryEntry c in _cache)
            {
                if (c.Key is string && Regex.IsMatch(((string)c.Key), regexString))
                {
                    ClearCacheItem((string)c.Key);
                }
            }
        }

        public override IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            return (from DictionaryEntry c in _cache
                    where c.Key is string && ((string)c.Key).InvariantStartsWith(keyStartsWith)
                    select c.Value.TryConvertTo<T>()
                    into attempt
                    where attempt.Success
                    select attempt.Result).ToList();
        }

        /// <summary>
        /// Returns a cache item by key, does not update the cache if it isn't there.
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public override TT GetCacheItem<TT>(string cacheKey)
        {
            var result = _cache.Get(cacheKey);
            if (result == null)
            {
                return default(TT);
            }
            return result.TryConvertTo<TT>().Result;
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with all of the default parameters
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public override TT GetCacheItem<TT>(string cacheKey, Func<TT> getCacheItem)
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
        public override TT GetCacheItem<TT>(string cacheKey,
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
        public override TT GetCacheItem<TT>(string cacheKey,
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
        public override TT GetCacheItem<TT>(string cacheKey,
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
        public override TT GetCacheItem<TT>(string cacheKey,
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
            var result = _cache.Get(cacheKey);
            if (result == null)
            {
                lock (syncLock)
                {
                    result = _cache.Get(cacheKey);
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
        public override void InsertCacheItem<T>(string cacheKey,
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
        public override void InsertCacheItem<T>(string cacheKey,
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
        public override void InsertCacheItem<T>(string cacheKey,
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
        public override void InsertCacheItem<T>(string cacheKey,
                                                CacheItemPriority priority,
                                                CacheItemRemovedCallback refreshAction,
                                                CacheDependency cacheDependency,
                                                TimeSpan? timeout,
                                                Func<T> getCacheItem)
        {
            object result = getCacheItem();
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