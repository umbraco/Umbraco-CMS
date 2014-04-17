using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    internal class NullCacheProvider : IRuntimeCacheProvider
    {
        public virtual void ClearAllCache()
        {
        }

        public virtual void ClearCacheItem(string key)
        {
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
        }

        public virtual void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
        }




        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
        }

        public virtual IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return Enumerable.Empty<object>();
        }

        public IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            return Enumerable.Empty<object>();
        }

        public virtual object GetCacheItem(string cacheKey)
        {
            return default(object);
        }

        public virtual object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return getCacheItem();
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            return getCacheItem();
        }

        public void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            
        }
    }
}