using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static readonly ICacheProvider NullRequestCache = new NullCacheProvider();
        private static readonly ICacheProvider NullStaticCache = new NullCacheProvider();
        private static readonly IRuntimeCacheProvider NullRuntimeCache = new NullCacheProvider();
        
        /// <summary>
        /// Creates a cache helper with disabled caches
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Good for unit testing
        /// </remarks>
        public static CacheHelper CreateDisabledCacheHelper()
        {
            return new CacheHelper(NullRuntimeCache, NullStaticCache, NullRequestCache, new IsolatedRuntimeCache(t => NullRuntimeCache));
        }

	    /// <summary>
	    /// Initializes a new instance for use in the web
	    /// </summary>
	    public CacheHelper()
	        : this(
	            new HttpRuntimeCacheProvider(HttpRuntime.Cache),
	            new StaticCacheProvider(),
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(t => new ObjectCacheRuntimeCacheProvider()))
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
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(t => new ObjectCacheRuntimeCacheProvider()))
	    {
	    }

	    [Obsolete("Use the constructor the specifies all dependencies")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CacheHelper(
            IRuntimeCacheProvider httpCacheProvider, 
            ICacheProvider staticCacheProvider, 
            ICacheProvider requestCacheProvider)
            : this(httpCacheProvider, staticCacheProvider, requestCacheProvider, new IsolatedRuntimeCache(t => new ObjectCacheRuntimeCacheProvider()))
        {            
        }

	    /// <summary>
	    /// Initializes a new instance based on the provided providers
	    /// </summary>
	    /// <param name="httpCacheProvider"></param>
	    /// <param name="staticCacheProvider"></param>
	    /// <param name="requestCacheProvider"></param>
	    /// <param name="isolatedCacheManager"></param>
	    public CacheHelper(
            IRuntimeCacheProvider httpCacheProvider,
            ICacheProvider staticCacheProvider,
            ICacheProvider requestCacheProvider,
            IsolatedRuntimeCache isolatedCacheManager)            
        {
	        if (httpCacheProvider == null) throw new ArgumentNullException("httpCacheProvider");
	        if (staticCacheProvider == null) throw new ArgumentNullException("staticCacheProvider");
	        if (requestCacheProvider == null) throw new ArgumentNullException("requestCacheProvider");
	        if (isolatedCacheManager == null) throw new ArgumentNullException("isolatedCacheManager");
	        RuntimeCache = httpCacheProvider;
            StaticCache = staticCacheProvider;
            RequestCache = requestCacheProvider;
            IsolatedRuntimeCache = isolatedCacheManager;
        }

        /// <summary>
        /// Returns the current Request cache
        /// </summary>
        public ICacheProvider RequestCache { get; internal set; }
        
        /// <summary>
        /// Returns the current Runtime cache
        /// </summary>
        public ICacheProvider StaticCache { get; internal set; }

        /// <summary>
        /// Returns the current Runtime cache
        /// </summary>
	    public IRuntimeCacheProvider RuntimeCache { get; internal set; }

        /// <summary>
        /// Returns the current Isolated Runtime cache manager
        /// </summary>
        public IsolatedRuntimeCache IsolatedRuntimeCache { get; internal set; }

        #region Legacy Runtime/Http Cache accessors

        /// <summary>
        /// Clears the item in umbraco's runtime cache
        /// </summary>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ClearAllCache()
        {
            RuntimeCache.ClearAllCache();
            IsolatedRuntimeCache.ClearAllCaches();
        }

        /// <summary>
        /// Clears the item in umbraco's runtime cache with the given key 
        /// </summary>
        /// <param name="key">Key</param>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ClearCacheItem(string key)
        {
            RuntimeCache.ClearCacheItem(key);
        }


        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type name as the
        /// input parameter. (using [object].GetType())
        /// </summary>
        /// <param name="typeName">The name of the System.Type which should be cleared from cache ex "System.Xml.XmlDocument"</param>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        public void ClearCacheObjectTypes(string typeName)
        {
            RuntimeCache.ClearCacheObjectTypes(typeName);
        }

        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type specified
        /// </summary>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ClearCacheObjectTypes<T>()
        {
            RuntimeCache.ClearCacheObjectTypes<T>();
        }

        /// <summary>
        /// Clears all cache items that starts with the key passed.
        /// </summary>
        /// <param name="keyStartsWith">The start of the key</param>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ClearCacheByKeySearch(string keyStartsWith)
        {
            RuntimeCache.ClearCacheByKeySearch(keyStartsWith);
        }

        /// <summary>
        /// Clears all cache items that have a key that matches the regular expression
        /// </summary>
        /// <param name="regexString"></param>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ClearCacheByKeyExpression(string regexString)
        {
            RuntimeCache.ClearCacheByKeyExpression(regexString);
        }

        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            return RuntimeCache.GetCacheItemsByKeySearch<T>(keyStartsWith);
        }

        /// <summary>
        /// Returns a cache item by key, does not update the cache if it isn't there.
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TT GetCacheItem<TT>(string cacheKey)
        {
            return RuntimeCache.GetCacheItem<TT>(cacheKey);
        }

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with all of the default parameters
        /// </summary>
        /// <typeparam name="TT"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TT GetCacheItem<TT>(string cacheKey, Func<TT> getCacheItem)
        {
            return RuntimeCache.GetCacheItem<TT>(cacheKey, getCacheItem);

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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TT GetCacheItem<TT>(string cacheKey,
            TimeSpan timeout, Func<TT> getCacheItem)
        {
            return RuntimeCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout);

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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            return RuntimeCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout, removedCallback: refreshAction);

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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            return RuntimeCache.GetCacheItem<TT>(cacheKey, getCacheItem, timeout, false, priority, refreshAction);

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
            var cache = GetHttpRuntimeCacheProvider(RuntimeCache);
            if (cache != null)
            {
                var result = cache.GetCacheItem(cacheKey, () => getCacheItem(), timeout, false, priority, refreshAction, cacheDependency);
                return result == null ? default(TT) : result.TryConvertTo<TT>().Result;
            }
            throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
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
            var cache = GetHttpRuntimeCacheProvider(RuntimeCache);
            if (cache != null)
            {
                var result = cache.GetCacheItem(cacheKey, () => getCacheItem(), null, false, priority, null, cacheDependency);
                return result == null ? default(TT) : result.TryConvertTo<TT>().Result;
            }
            throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="getCacheItem"></param>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       Func<T> getCacheItem)
        {
            RuntimeCache.InsertCacheItem<T>(cacheKey, getCacheItem, priority: priority);

        }

        /// <summary>
        /// Inserts an item into the cache, if it already exists in the cache it will be replaced
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="priority"></param>
        /// <param name="timeout">This will set an absolute expiration from now until the timeout</param>
        /// <param name="getCacheItem"></param>
        [Obsolete("Do not use this method, access the runtime cache from the RuntimeCache property")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InsertCacheItem<T>(string cacheKey,
                                       CacheItemPriority priority,
                                       TimeSpan timeout,
                                       Func<T> getCacheItem)
        {
            RuntimeCache.InsertCacheItem<T>(cacheKey, getCacheItem, timeout, priority: priority);
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
            var cache = GetHttpRuntimeCacheProvider(RuntimeCache);
            if (cache != null)
            {
                cache.InsertCacheItem(cacheKey, () => getCacheItem(), timeout, false, priority, null, cacheDependency);
            }
            throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
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
            var cache = GetHttpRuntimeCacheProvider(RuntimeCache);
            if (cache != null)
            {
                cache.InsertCacheItem(cacheKey, () => getCacheItem(), timeout, false, priority, refreshAction, cacheDependency);
            }
            throw new InvalidOperationException("Cannot use this obsoleted overload when the current provider is not of type " + typeof(HttpRuntimeCacheProvider));
        } 
        #endregion

	    private HttpRuntimeCacheProvider GetHttpRuntimeCacheProvider(IRuntimeCacheProvider runtimeCache)
	    {
	        HttpRuntimeCacheProvider cache;
            var wrapper = RuntimeCache as IRuntimeCacheProviderWrapper;
	        if (wrapper != null)
	        {
	            cache = wrapper.InnerProvider as HttpRuntimeCacheProvider;
	        }
	        else
	        {
                cache = RuntimeCache as HttpRuntimeCacheProvider;
            }
	        return cache;
	    }
    }

}
