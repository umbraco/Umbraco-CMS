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
            StaticCache.RemoveAll(kvp => kvp.Value != null && kvp.Value.GetType().ToString().InvariantEquals(typeName));
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
            var typeOfT = typeof(T);
            StaticCache.RemoveAll(kvp => kvp.Value != null && kvp.Value.GetType() == typeOfT);
        }

        public virtual void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            var typeOfT = typeof(T);
            StaticCache.RemoveAll(kvp => kvp.Value != null && kvp.Value.GetType() == typeOfT && predicate(kvp.Key, (T)kvp.Value));
        }

        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
            StaticCache.RemoveAll(kvp => kvp.Key.InvariantStartsWith(keyStartsWith));
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
            StaticCache.RemoveAll(kvp => Regex.IsMatch(kvp.Key, regexString)); 
        }

        public virtual IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return (from KeyValuePair<string, object> c in StaticCache
                    where c.Key.InvariantStartsWith(keyStartsWith)
                    select c.Value).ToList();
        }

        public IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            return (from KeyValuePair<string, object> c in StaticCache
                    where Regex.IsMatch(c.Key, regexString) 
                    select c.Value).ToList();
        }

        public virtual object GetCacheItem(string cacheKey)
        {
            var result = StaticCache[cacheKey];
            return result;
        }

        public virtual object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return StaticCache.GetOrAdd(cacheKey, key => getCacheItem());
        }
        
    }
}