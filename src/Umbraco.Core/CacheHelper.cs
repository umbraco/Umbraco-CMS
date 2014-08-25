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
	    private readonly ICacheProvider _requestCache;
        private readonly ICacheProvider _nullRequestCache = new NullCacheProvider();
        private readonly ICacheProvider _staticCache;
        private readonly ICacheProvider _nullStaticCache = new NullCacheProvider();
        private readonly IRuntimeCacheProvider _httpCache;
        private readonly IRuntimeCacheProvider _nullHttpCache = new NullCacheProvider();

        /// <summary>
        /// Creates a cache helper with disabled caches
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Good for unit testing
        /// </remarks>
        public static CacheHelper CreateDisabledCacheHelper()
        {
            return new CacheHelper(null, null, null, false);
        }

	    /// <summary>
	    /// Initializes a new instance for use in the web
	    /// </summary>
	    public CacheHelper()
	        : this(
	            new HttpRuntimeCacheProvider(HttpRuntime.Cache),
	            new StaticCacheProvider(),
                new HttpRequestCacheProvider())
	    {
	    }

	    /// <summary>
	    /// Initializes a new instance for use in the web
	    /// </summary>
	    /// <param name="cache"></param>
	    public CacheHelper(System.Web.Caching.Cache cache)
	        : this(
	            new HttpRuntimeCacheProvider(cache),
	            new StaticCacheProvider(),
                new HttpRequestCacheProvider())
	    {
	    }

	    /// <summary>
        /// Initializes a new instance based on the provided providers
        /// </summary>
        /// <param name="httpCacheProvider"></param>
        /// <param name="staticCacheProvider"></param>
        /// <param name="requestCacheProvider"></param>
        public CacheHelper(
            IRuntimeCacheProvider httpCacheProvider, 
            ICacheProvider staticCacheProvider, 
            ICacheProvider requestCacheProvider)
            : this(httpCacheProvider, staticCacheProvider, requestCacheProvider, true)
        {            
        }

        /// <summary>
        /// Private ctor used for creating a disabled cache helper
        /// </summary>
        /// <param name="httpCacheProvider"></param>
        /// <param name="staticCacheProvider"></param>
        /// <param name="requestCacheProvider"></param>
        /// <param name="enableCache"></param>
        private CacheHelper(
            IRuntimeCacheProvider httpCacheProvider,
            ICacheProvider staticCacheProvider,
            ICacheProvider requestCacheProvider, 
            bool enableCache)
        {
            if (enableCache)
            {
                _httpCache = httpCacheProvider;
                _staticCache = staticCacheProvider;
                _requestCache = requestCacheProvider;
            }
            else
            {
                _httpCache = null;
                _staticCache = null;
                _requestCache = null;
            }

            _enableCache = enableCache;
        }

        /// <summary>
        /// Returns the current Request cache
        /// </summary>
        public ICacheProvider RequestCache
        {
            get { return _enableCache ? _requestCache : _nullRequestCache; }
        }

        /// <summary>
        /// Returns the current Runtime cache
        /// </summary>
        public ICacheProvider StaticCache
        {
            get { return _enableCache ? _staticCache : _nullStaticCache; }
        }

        /// <summary>
        /// Returns the current Runtime cache
        /// </summary>
	    public IRuntimeCacheProvider RuntimeCache
	    {
	        get { return _enableCache ? _httpCache : _nullHttpCache; }
	    }
        
	    #region Legacy Runtime/Http Cache accessors

        /// <summary>
        /// Clears the item in umbraco's runtime cache
        /// </summary>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public void ClearAllCache()
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public void ClearCacheItem(string key)
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public void ClearCacheObjectTypes(string typeName)
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public void ClearCacheObjectTypes<T>()
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public void ClearCacheByKeySearch(string keyStartsWith)
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public void ClearCacheByKeyExpression(string regexString)
        {
            if (_enableCache == false)
            {
                _nullHttpCache.ClearCacheByKeyExpression(regexString);
            }
            else
            {
                _httpCache.ClearCacheByKeyExpression(regexString);
            }
        }

        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public TT GetCacheItem<TT>(string cacheKey)
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public TT GetCacheItem<TT>(string cacheKey, Func<TT> getCacheItem)
        {
            if (_enableCache == false)
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public TT GetCacheItem<TT>(string cacheKey,
            TimeSpan timeout, Func<TT> getCacheItem)
        {
            if (_enableCache == false)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout);
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout, removedCallback: refreshAction);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout, removedCallback: refreshAction);
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
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            if (_enableCache == false)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout, false, priority, refreshAction);
            }
            else
            {
                return _httpCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout, false, priority, refreshAction);
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
        [Obsolete("Do not use this method, we no longer support the caching overloads with references to CacheDependency, use the overloads specifying a file collection instead")]
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority,
            CacheItemRemovedCallback refreshAction,
            CacheDependency cacheDependency,
            TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            if (_enableCache == false)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout, false, priority, refreshAction, null);
            }
            else
            {
                var cache = _httpCache as HttpRuntimeCacheProvider;
                if (cache != null)
                {
                    var result = cache.GetCacheItem(cacheKey, () => getCacheItem(), timeout, false, priority, refreshAction, cacheDependency);
                    return result == null ? default(TT) : result.TryConvertTo<TT>().Result;
                }
                throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
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
        [Obsolete("Do not use this method, we no longer support the caching overloads with references to CacheDependency, use the overloads specifying a file collection instead")]
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority,
            CacheDependency cacheDependency,
            Func<TT> getCacheItem)
        {
            if (!_enableCache)
            {
                return _nullHttpCache.GetCacheItem<TT>(cacheKey, getCacheItem, null, false, priority, null, null);
            }
            else
            {
                var cache = _httpCache as HttpRuntimeCacheProvider;
                if (cache != null)
                {
                    var result = cache.GetCacheItem(cacheKey, () => getCacheItem(), null, false, priority, null, cacheDependency);
                    return result == null ? default(TT) : result.TryConvertTo<TT>().Result;
                }
                throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
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
            if (_enableCache == false)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, getCacheItem, priority: priority);
            }
            else
            {
                _httpCache.InsertCacheItem<T>(cacheKey, getCacheItem, priority: priority);
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
            if (_enableCache == false)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, getCacheItem, timeout, priority: priority);
            }
            else
            {
                _httpCache.InsertCacheItem<T>(cacheKey, getCacheItem, timeout, priority: priority);
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
        [Obsolete("Do not use this method, we no longer support the caching overloads with references to CacheDependency, use the overloads specifying a file collection instead")]
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       CacheDependency cacheDependency,
                                       TimeSpan timeout,
                                       Func<T> getCacheItem)
        {
            if (_enableCache == false)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, getCacheItem, timeout, priority: priority, dependentFiles:null);
            }
            else
            {
                var cache = _httpCache as HttpRuntimeCacheProvider;
                if (cache != null)
                {
                    cache.InsertCacheItem(cacheKey, () => getCacheItem(), timeout, false, priority, null, cacheDependency);
                }
                throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
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
        [Obsolete("Do not use this method, we no longer support the caching overloads with references to CacheDependency, use the overloads specifying a file collection instead")]
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       CacheItemRemovedCallback refreshAction,
                                       CacheDependency cacheDependency,
                                       TimeSpan? timeout,
                                       Func<T> getCacheItem)
        {
            if (_enableCache == false)
            {
                _nullHttpCache.InsertCacheItem<T>(cacheKey, getCacheItem, timeout, false, priority, refreshAction, null);
            }
            else
            {
                var cache = _httpCache as HttpRuntimeCacheProvider;
                if (cache != null)
                {
                    cache.InsertCacheItem(cacheKey, () => getCacheItem(), timeout, false, priority, refreshAction, cacheDependency);
                }
                throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
            }
        } 
        #endregion

	}

}
