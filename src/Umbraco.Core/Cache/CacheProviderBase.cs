using System;
using System.Collections.Generic;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// An abstract class for implementing a basic cache provider
    /// </summary>
    /// <remarks>
    /// THIS MUST REMAIN INTERNAL UNTIL WE STREAMLINE HOW ALL CACHE IS HANDLED, WE NEED TO SUPPORT HTTP RUNTIME CACHE, IN MEMORY CACHE, ETC...
    /// </remarks>
    internal abstract class CacheProviderBase
    {
        public abstract void ClearAllCache();
        public abstract void ClearCacheItem(string key);
        public abstract void ClearCacheObjectTypes(string typeName);
        public abstract void ClearCacheObjectTypes<T>();
        public abstract void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate);
        public abstract void ClearCacheByKeySearch(string keyStartsWith);
        public abstract void ClearCacheByKeyExpression(string regexString);
        public abstract IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith);
        public abstract T GetCacheItem<T>(string cacheKey);
        public abstract T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem);
    }
}