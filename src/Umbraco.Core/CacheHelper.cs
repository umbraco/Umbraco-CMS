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
	/// <remarks>
	/// This class may be opened publicly at some point but needs a review of what is absoletely necessary.
	/// </remarks>
    public class CacheHelper  //: CacheProviderBase
	{
	    private readonly bool _enableCache;
	    private readonly HttpRuntimeCacheProvider _httpCache;
	    private readonly NullCacheProvider _nullCache = new NullCacheProvider();

		public CacheHelper(System.Web.Caching.Cache cache)
            : this(cache, true)
		{
		}

	    internal CacheHelper(System.Web.Caching.Cache cache, bool enableCache)
	    {
            _httpCache = new HttpRuntimeCacheProvider(cache);
	        _enableCache = enableCache;
	    }
        
        public void ClearAllCache()
        {                         
			if (!_enableCache)
			{
			    _nullCache.ClearAllCache();
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
                _nullCache.ClearCacheItem(key);
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
                _nullCache.ClearCacheObjectTypes(typeName);
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
                _nullCache.ClearCacheObjectTypes<T>();
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
                _nullCache.ClearCacheByKeySearch(keyStartsWith);
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
                _nullCache.ClearCacheByKeyExpression(regexString);
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
                return _nullCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
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
                return _nullCache.GetCacheItem<TT>(cacheKey);
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
                return _nullCache.GetCacheItem<TT>(cacheKey, getCacheItem);
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
                return _nullCache.GetCacheItem<TT>(cacheKey, timeout, getCacheItem);
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
                return _nullCache.GetCacheItem<TT>(cacheKey, refreshAction, timeout, getCacheItem);
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
                return _nullCache.GetCacheItem<TT>(cacheKey, priority, refreshAction, timeout, getCacheItem);
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
                return _nullCache.GetCacheItem<TT>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
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
                _nullCache.InsertCacheItem<T>(cacheKey, priority, timeout, getCacheItem);
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
                _nullCache.InsertCacheItem<T>(cacheKey, priority, cacheDependency, timeout, getCacheItem);
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
                _nullCache.InsertCacheItem<T>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
            }
            else
            {
                _httpCache.InsertCacheItem<T>(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem);
            }
	    }

	}

}
