using System;
using System.Web.Caching;
using System.Web;
using Umbraco.Core;

namespace umbraco.cms.businesslogic.cache
{
    /// <summary>
    /// Used to easily store and retrieve items from the cache.
    /// </summary>
    /// <remarks>
    /// This whole class will become obsolete, however one of the methods is still used that is not ported over to the new CacheHelper
    /// class so that is why the class declaration is not marked obsolete.
    /// We haven't migrated it because I don't know why it is needed.
    /// </remarks>
    [Obsolete("Use the ApplicationContext.Current.ApplicationCache instead")]
    public class Cache
    {
        /// <summary>
        /// Clears everything in umbraco's runtime cache, which means that not only
        /// umbraco content is removed, but also other cache items from pages running in
        /// the same application / website. Use with care :-)
        /// </summary>
        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.ClearAllCache instead")]
        public static void ClearAllCache()
        {
	        var helper = new CacheHelper(System.Web.HttpRuntime.Cache);
			helper.RuntimeCache.ClearAllCache();
        }

        /// <summary>
        /// Clears the item in umbraco's runtime cache with the given key 
        /// </summary>
        /// <param name="key">Key</param>
        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.ClearCacheItem instead")]
        public static void ClearCacheItem(string key)
        {
			var helper = new CacheHelper(System.Web.HttpRuntime.Cache);
	        helper.RuntimeCache.ClearCacheItem(key);
        }


        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type name as the
        /// input parameter. (using [object].GetType())
        /// </summary>
        /// <param name="TypeName">The name of the System.Type which should be cleared from cache ex "System.Xml.XmlDocument"</param>
        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.ClearCacheObjectTypes instead")]
        public static void ClearCacheObjectTypes(string TypeName)
        {
			var helper = new CacheHelper(System.Web.HttpRuntime.Cache);
			helper.RuntimeCache.ClearCacheObjectTypes(TypeName);
        }

        /// <summary>
        /// Clears all cache items that starts with the key passed.
        /// </summary>
        /// <param name="KeyStartsWith">The start of the key</param>
        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch instead")]
        public static void ClearCacheByKeySearch(string KeyStartsWith)
        {
			var helper = new CacheHelper(System.Web.HttpRuntime.Cache);
			helper.RuntimeCache.ClearCacheByKeySearch(KeyStartsWith);
        }

        /// <summary>
        /// Retrieve all cached items
        /// </summary>
        /// <returns>A hastable containing all cacheitems</returns>
        [Obsolete("This method should not be used, use other methods to return specific cached items")]
        public static System.Collections.Hashtable ReturnCacheItemsOrdred()
        {
            System.Collections.Hashtable ht = new System.Collections.Hashtable();
            System.Web.Caching.Cache c = System.Web.HttpRuntime.Cache;
            if (c != null)
            {
                System.Collections.IDictionaryEnumerator cacheEnumerator = c.GetEnumerator();
                while (cacheEnumerator.MoveNext())
                {
                    if (ht[c[cacheEnumerator.Key.ToString()].GetType().ToString()] == null)
                        ht.Add(c[cacheEnumerator.Key.ToString()].GetType().ToString(), new System.Collections.ArrayList());

                    ((System.Collections.ArrayList)ht[c[cacheEnumerator.Key.ToString()].GetType().ToString()]).Add(cacheEnumerator.Key.ToString());
                }
            }
            return ht;
        }

        public delegate TT GetCacheItemDelegate<TT>();

        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.GetCacheItem instead")]
        public static TT GetCacheItem<TT>(string cacheKey, object syncLock,
            TimeSpan timeout, GetCacheItemDelegate<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, syncLock, null, timeout, getCacheItem);
        }

        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.GetCacheItem instead")]
        public static TT GetCacheItem<TT>(string cacheKey, object syncLock,
            CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            GetCacheItemDelegate<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, syncLock, CacheItemPriority.Normal, refreshAction, timeout, getCacheItem);
        }
        
        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.GetCacheItem instead")]
        public static TT GetCacheItem<TT>(string cacheKey, object syncLock,
            CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            GetCacheItemDelegate<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, syncLock, priority, refreshAction, null, timeout, getCacheItem);
        }

        [Obsolete("Use the ApplicationContext.Current.ApplicationCache.GetCacheItem instead")]
        public static TT GetCacheItem<TT>(string cacheKey, object syncLock,
            CacheItemPriority priority, CacheItemRemovedCallback refreshAction,
            CacheDependency cacheDependency, TimeSpan timeout, GetCacheItemDelegate<TT> getCacheItem)
        {
			var helper = new CacheHelper(System.Web.HttpRuntime.Cache);
			Func<TT> f = () => getCacheItem();
			return helper.GetCacheItem(cacheKey, priority, refreshAction, cacheDependency, timeout, f);
        }
    }
}
