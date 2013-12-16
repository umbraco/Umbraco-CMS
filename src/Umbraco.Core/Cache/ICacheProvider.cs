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
    internal interface ICacheProvider
    {
        void ClearAllCache();
        void ClearCacheItem(string key);
        void ClearCacheObjectTypes(string typeName);
        void ClearCacheObjectTypes<T>();
        void ClearCacheByKeySearch(string keyStartsWith);
        void ClearCacheByKeyExpression(string regexString);
        IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith);
        T GetCacheItem<T>(string cacheKey);
        T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem);
    }
}