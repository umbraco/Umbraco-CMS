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
    internal class StaticCacheProvider : CacheProviderBase
    {
        private readonly ConcurrentDictionary<string, object> _staticCache = new ConcurrentDictionary<string, object>();

        public override void ClearAllCache()
        {
            _staticCache.Clear();
        }

        public override void ClearCacheItem(string key)
        {
            object val;
            _staticCache.TryRemove(key, out val);
        }

        public override void ClearCacheObjectTypes(string typeName)
        {
            foreach (var key in _staticCache.Keys)
            {
                if (_staticCache[key] != null
                    && _staticCache[key].GetType().ToString().InvariantEquals(typeName))
                {
                    object val;
                    _staticCache.TryRemove(key, out val);
                }
            }
        }

        public override void ClearCacheObjectTypes<T>()
        {
            foreach (var key in _staticCache.Keys)
            {
                if (_staticCache[key] != null
                    && _staticCache[key].GetType() == typeof(T))
                {
                    object val;
                    _staticCache.TryRemove(key, out val);
                }
            }
        }

        public override void ClearCacheByKeySearch(string keyStartsWith)
        {
            foreach (var key in _staticCache.Keys)
            {
                if (key.InvariantStartsWith(keyStartsWith))
                {
                    ClearCacheItem(key);
                }
            }
        }

        public override void ClearCacheByKeyExpression(string regexString)
        {
            foreach (var key in _staticCache.Keys)
            {
                if (Regex.IsMatch(key, regexString))
                {
                    ClearCacheItem(key);
                }
            }
        }

        public override IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            return (from KeyValuePair<string, object> c in _staticCache
                    where c.Key.InvariantStartsWith(keyStartsWith)
                    select c.Value.TryConvertTo<T>()
                    into attempt
                    where attempt.Success
                    select attempt.Result).ToList();
        }

        public override T GetCacheItem<T>(string cacheKey)
        {
            var result = _staticCache[cacheKey];
            if (result == null)
            {
                return default(T);
            }
            return result.TryConvertTo<T>().Result;
        }

        public override T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            return (T)_staticCache.GetOrAdd(cacheKey, getCacheItem);
        }
        
    }
}