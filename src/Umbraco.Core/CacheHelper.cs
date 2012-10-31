using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{

	/// <summary>
	/// Class that is exposed by the ApplicationContext for application wide caching purposes
	/// </summary>
	/// <remarks>
	/// This class may be opened publicly at some point but needs a review of what is absoletely necessary.
	/// </remarks>
	internal class CacheHelper
	{
		private readonly Cache _cache;

		public CacheHelper(System.Web.Caching.Cache cache)
		{
			_cache = cache;
		}

		private static readonly object Locker = new object();

        /// <summary>
        /// Clears everything in umbraco's runtime cache, which means that not only
        /// umbraco content is removed, but also other cache items from pages running in
        /// the same application / website. Use with care :-)
        /// </summary>
        public void ClearAllCache()
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
        public void ClearCacheItem(string key)
        {
            // NH 10 jan 2012
            // Patch by the always wonderful Stéphane Gay to avoid cache null refs
            lock (Locker)
            {
	            if (_cache[key] == null) return;
	            _cache.Remove(key);;
            }
        }


        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type name as the
        /// input parameter. (using [object].GetType())
        /// </summary>
        /// <param name="typeName">The name of the System.Type which should be cleared from cache ex "System.Xml.XmlDocument"</param>
        public void ClearCacheObjectTypes(string typeName)
        {
            try
            {
				lock (Locker)
				{
					foreach (var c in from DictionaryEntry c in _cache where _cache[c.Key.ToString()] != null && _cache[c.Key.ToString()].GetType().ToString() == typeName select c)
					{
						_cache.Remove(c.Key.ToString());
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
        public void ClearCacheByKeySearch(string keyStartsWith)
        {
	        foreach (var c in from DictionaryEntry c in _cache where c.Key is string && ((string)c.Key).StartsWith(keyStartsWith) select c)
	        {
		        ClearCacheItem((string)c.Key);
	        }
        }
		
        public TT GetCacheItem<TT>(string cacheKey,
            TimeSpan timeout, Func<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, null, timeout, getCacheItem);
        }

        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, CacheItemPriority.Normal, refreshAction, timeout, getCacheItem);
        }

        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan timeout,
            Func<TT> getCacheItem)
        {
            return GetCacheItem(cacheKey, priority, refreshAction, null, timeout, getCacheItem);
        }

        public TT GetCacheItem<TT>(string cacheKey,
            CacheItemPriority priority, 
			CacheItemRemovedCallback refreshAction,
            CacheDependency cacheDependency, 
			TimeSpan timeout, 
			Func<TT> getCacheItem)
        {
	        return GetCacheItem(cacheKey, priority, refreshAction, cacheDependency, timeout, getCacheItem, Locker);
        }

		/// <summary>
		/// This is used only for legacy purposes as I did not want to change all of the locking to one lock found on this object, 
		/// however, the reason this is used for legacy purposes is because I see zero reason to use different sync locks, just the one
		/// lock (Locker) on this class should be sufficient.
		/// </summary>
		/// <typeparam name="TT"></typeparam>
		/// <param name="cacheKey"></param>
		/// <param name="priority"></param>
		/// <param name="refreshAction"></param>
		/// <param name="cacheDependency"></param>
		/// <param name="timeout"></param>
		/// <param name="getCacheItem"></param>
		/// <param name="syncLock"></param>
		/// <returns></returns>
		internal TT GetCacheItem<TT>(string cacheKey,
			CacheItemPriority priority, CacheItemRemovedCallback refreshAction,
			CacheDependency cacheDependency, TimeSpan timeout, Func<TT> getCacheItem, object syncLock)
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
							_cache.Insert(cacheKey, result, cacheDependency, DateTime.Now.Add(timeout), TimeSpan.Zero, priority, refreshAction);
						}
					}
				}
			}
			return (TT)result;
		}
    }

}
