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
            _staticCache.RemoveAll(kvp => kvp.Value.GetType().ToString().InvariantEquals(typeName));
        }

        public override void ClearCacheObjectTypes<T>()
        {
            _staticCache.RemoveAll(kvp => kvp.Value is T);
        }

        public override void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            _staticCache.RemoveAll(kvp => kvp.Value is T && predicate(kvp.Key, (T)kvp.Value));
        }

        public override void ClearCacheByKeySearch(string keyStartsWith)
        {
            _staticCache.RemoveAll(kvp => kvp.Key.InvariantStartsWith(keyStartsWith));
        }

        public override void ClearCacheByKeyExpression(string regexString)
        {
            _staticCache.RemoveAll(kvp => Regex.IsMatch(kvp.Key, regexString));
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