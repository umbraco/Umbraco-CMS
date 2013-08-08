using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache provider that statically caches everything in an in memory dictionary
    /// </summary>
    internal class StaticCacheProvider : ICacheProvider
    {
        internal readonly ConcurrentDictionary<string, object> StaticCache = new ConcurrentDictionary<string, object>();

        public virtual void ClearAllCache()
        {
            StaticCache.Clear();
        }

        public virtual void ClearCacheItem(string key)
        {
            object val;
            StaticCache.TryRemove(key, out val);
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
            foreach (var key in StaticCache.Keys)
            {
                if (StaticCache[key] != null
                    && StaticCache[key].GetType().ToString().InvariantEquals(typeName))
                {
                    object val;
                    StaticCache.TryRemove(key, out val);
                }
            }
        }        

        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
            foreach (var key in StaticCache.Keys)
            {
                if (key.InvariantStartsWith(keyStartsWith))
                {
                    ClearCacheItem(key);
                }
            }
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
            foreach (var key in StaticCache.Keys)
            {
                if (Regex.IsMatch(key, regexString))
                {
                    ClearCacheItem(key);
                }
            }
        }

        public virtual IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return (from KeyValuePair<string, object> c in StaticCache
                    where c.Key.InvariantStartsWith(keyStartsWith)
                    select c.Value).ToList();
        }

        public virtual object GetCacheItem(string cacheKey)
        {
            var result = StaticCache[cacheKey];
            return result;
        }

        public virtual object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return StaticCache.GetOrAdd(cacheKey, getCacheItem());
        }
        
    }
}