using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{

	/// <summary>
	/// Class that is exposed by the ApplicationContext for application wide caching purposes
	/// </summary>
    public class CacheHelper
	{
	    private readonly bool _enableCache;
        private readonly CacheProviderBase _staticCache;
        private readonly CacheProviderBase _nullStaticCache = new NullCacheProvider();
        private readonly RuntimeCacheProviderBase _httpCache;
        private readonly RuntimeCacheProviderBase _nullHttpCache = new NullCacheProvider();

		public CacheHelper(System.Web.Caching.Cache cache)
            : this(cache, true)
		{
		}

	    internal CacheHelper(System.Web.Caching.Cache cache, bool enableCache)
            : this(new HttpRuntimeCacheProvider(cache), enableCache)
	    {            
	    }

        internal CacheHelper(RuntimeCacheProviderBase httpCacheProvider, bool enableCache)
            : this(httpCacheProvider, new StaticCacheProvider(), enableCache)
        {
        }

        internal CacheHelper(RuntimeCacheProviderBase httpCacheProvider, CacheProviderBase staticCacheProvider, bool enableCache)
        {
            _httpCache = httpCacheProvider;
            _staticCache = staticCacheProvider;
            _enableCache = enableCache;
        }

        #region Static cache

        /// <summary>
        /// Clears the item in umbraco's static cache
        /// </summary>
        internal void ClearAllStaticCache()
        {
            if (!_enableCache)
            {
                _nullStaticCache.ClearAllCache();
            }
            else
            {
                _staticCache.ClearAllCache();
            }
        }

        /// <summary>
        /// Clears the item in umbraco's static cache with the given key 
        /// </summary>
        /// <param name="key">Key</param>
        internal void ClearStaticCacheItem(string key)
        {
            if (!_enableCache)
            {
                _nullStaticCache.ClearCacheItem(key);
            }
            else
            {
                _staticCache.ClearCacheItem(key);
            }
        }

        /// <summary>
        /// Clears all objects in the static cache with the System.Type name as the
        /// input parameter. (using [object].GetType())
        /// </summary>
        /// <param name="typeName">The name of the System.Type which should be cleared from cache ex "System.Xml.XmlDocument"</param>
        internal void ClearStaticCacheObjectTypes(string typeName)
        {
            if (!_enableCache)
            {
                _nullStaticCache.ClearCacheObjectTypes(typeName);
            }
            else
            {
                _staticCache.ClearCacheObjectTypes(typeName);
            }
        }

        /// <summary>
        /// Clears all objects in the static cache with the System.Type specified
        /// </summary>
        internal void ClearStaticCacheObjectTypes<T>()
        {
            if (!_enableCache)
            {
                _nullStaticCache.ClearCacheObjectTypes<T>();
            }
            else
            {
                _staticCache.ClearCacheObjectTypes<T>();
            }
        }

	    internal void ClearStaticCacheObjectTypes<T>(Func<string, T, bool> predicate)
	    {
	        if (_enableCache)
                _staticCache.ClearCacheObjectTypes(predicate);
            else
                _nullStaticCache.ClearCacheObjectTypes(predicate);
	    }

        /// <summary>
        /// Clears all static cache items that starts with the key passed.
        /// </summary>
        /// <param name="keyStartsWith">The start of the key</param>
        internal void ClearStaticCacheByKeySearch(string keyStartsWith)
        {
            if (!_enableCache)
            {
                _nullStaticCache.ClearCacheByKeySearch(keyStartsWith);
            }
            else
            {
                _staticCache.ClearCacheByKeySearch(keyStartsWith);
            }
        }

        /// <summary>
        /// Clears all cache items that have a key that matches the regular expression
        /// </summary>
        /// <param name="regexString"></param>
        internal void ClearStaticCacheByKeyExpression(string regexString)
        {
            if (!_enableCache)
            {
                _nullStaticCache.ClearCacheByKeyExpression(regexString);
            }
            else
            {
                _staticCache.ClearCacheByKeyExpression(regexString);
            }
        }

        internal IEnumerable<T> GetStaticCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            if (!_enableCache)
            {
                return _nullStaticCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
            }
            else
            {
                return _staticCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
            }
        }

        /// <summary>
        /// Returns a static cache item by key, does not update the cache if it isn't there.
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        internal TT GetStaticCacheItem<TT>(string cacheKey)
        {
            if (!_enableCache)
            {
                return _nullStaticCache.GetCacheItem<TT>(cacheKey);
            }
            else
            {
                return _staticCache.GetCacheItem<TT>(cacheKey);
            }
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the static cache with all of the default parameters
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        internal TT GetStaticCacheItem<TT>(string cacheKey, Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullStaticCache.GetCacheItem<TT>(cacheKey, getCacheItem);
            }
            else
            {
                return _staticCache.GetCacheItem<TT>(cacheKey, getCacheItem);
            }
        }

        #endregion


	    #region Runtime/Http Cache
        /// <summary>
        /// Clears the item in umbraco's runtime cache
        /// </summary>
        public void ClearAllCache()
        {
            if (!_enableCache)
            {
                _nullHttpCache.ClearAllCache();
            }
            else
            {
                _httpCache.ClearAllCache();
            }
        }

        /// <summary>
        /// Clears the item in umbraco's runtime cache with the given key 
        /// </summary>
        /// <param name="key">Key</param>
        public void ClearCacheItem(string key)
        {
            if (!_enableCache)
            {
                _nullHttpCache.ClearCacheItem(key);
            }
            else
            {
                _httpCache.ClearCacheItem(key);
            }
        }


        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type name as the
        /// input parameter. (using [object].GetType())
        /// </summary>
        /// <param name="typeName">The name of the System.Type which should be cleared from cache ex "System.Xml.XmlDocument"</param>
        public void ClearCacheObjectTypes(string typeName)
        {
            if (!_enableCache)
            {
                _nullHttpCache.ClearCacheObjectTypes(typeName);
            }
            else
            {
                _httpCache.ClearCacheObjectTypes(typeName);
            }
        }

        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type specified
        /// </summary>
        public void ClearCacheObjectTypes<T>()
        {
            if (!_enableCache)
            {
                _nullHttpCache.ClearCacheObjectTypes<T>();
            }
            else
            {
                _httpCache.ClearCacheObjectTypes<T>();
            }
        }

        /// <summary>
        /// Clears all cache items that starts with the key passed.
        /// </summary>
        /// <param name="keyStartsWith">The start of the key</param>
        public void ClearCacheByKeySearch(string keyStartsWith)
        {
            if (!_enableCache)
            {
                _nullHttpCache.ClearCacheByKeySearch(keyStartsWith);
            }
            else
            {
                _httpCache.ClearCacheByKeySearch(keyStartsWith);
            }
        }

        /// <summary>
        /// Clears all cache items that have a key that matches the regular expression
        /// </summary>
        /// <param name="regexString"></param>
        public void ClearCacheByKeyExpression(string regexString)
        {
            if (!_enableCache)
            {
                _nullHttpCache.ClearCacheByKeyExpression(regexString);
            }
            else
            {
                _httpCache.ClearCacheByKeyExpression(regexString);
            }
        }

        public IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
            }
            else
            {
                return _httpCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
            }
        }

        /// <summary>
        /// Returns a cache item by key, does not update the cache if it isn't there.
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public TT GetCacheItem<TT>(string cacheKey)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey);
            }
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with all of the default parameters
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public TT GetCacheItem<TT>(string cacheKey, Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, getCacheItem);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, getCacheItem);
            }
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with the specified absolute expiration date (from NOW)
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public TT GetCacheItem<TT>(string cacheKey,
            TimeSpan timeout, Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, timeout, getCacheItem);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, timeout, getCacheItem);
            }
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
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, refreshAction, timeout, getCacheItem);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, refreshAction, timeout, getCacheItem);
            }
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
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, priority, refreshAction, timeout, getCacheItem);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, priority, refreshAction, timeout, getCacheItem);
            }
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
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority,
            CacheItemRemovedCallback refreshAction,
            CacheDependency cacheDependency,
            TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
            }
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="cacheDependency"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority,
            CacheDependency cacheDependency,
            Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, priority, null, cacheDependency, null, getCacheItem);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, priority, null, cacheDependency, null, getCacheItem);
            }
        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="getCacheItem"></param>
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       Func<T> getCacheItem)
        {
            if (!_enableCache)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, priority, getCacheItem);
            }
            else
            {
                _httpCache.InsertCacheItem<T>(cacheKey, priority, getCacheItem);
            }
        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       TimeSpan timeout,
                                       Func<T> getCacheItem)
        {
            if (!_enableCache)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, priority, timeout, getCacheItem);
            }
            else
            {
                _httpCache.InsertCacheItem<T>(cacheKey, priority, timeout, getCacheItem);
            }
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
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       CacheDependency cacheDependency,
                                       TimeSpan timeout,
                                       Func<T> getCacheItem)
        {
            if (!_enableCache)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, priority, cacheDependency, timeout, getCacheItem);
            }
            else
            {
                _httpCache.InsertCacheItem<T>(cacheKey, priority, cacheDependency, timeout, getCacheItem);
            }
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
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       CacheItemRemovedCallback refreshAction,
                                       CacheDependency cacheDependency,
                                       TimeSpan? timeout,
                                       Func<T> getCacheItem)
        {
            if (!_enableCache)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
            }
            else
            {
                _httpCache.InsertCacheItem<T>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
            }
        } 
        #endregion

	}

}
