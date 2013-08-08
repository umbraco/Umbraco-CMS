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
        void ClearCacheByKeySearch(string keyStartsWith);
        void ClearCacheByKeyExpression(string regexString);
        IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith);
        object GetCacheItem(string cacheKey);
        object GetCacheItem(string cacheKey, Func<object> getCacheItem);
    }
}