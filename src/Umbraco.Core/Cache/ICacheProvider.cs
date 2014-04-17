using System;
using System.Collections.Generic;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// An abstract class for implementing a basic cache provider
    /// </summary>
    public interface ICacheProvider
    {
        void ClearAllCache();
        void ClearCacheItem(string key);
        void ClearCacheObjectTypes(string typeName);
        void ClearCacheObjectTypes<T>();
        void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate);
        void ClearCacheByKeySearch(string keyStartsWith);
        void ClearCacheByKeyExpression(string regexString);
        IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith);
        IEnumerable<object> GetCacheItemsByKeyExpression(string regexString);
        
        /// <summary>
        /// Returns an item with a given key
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        object GetCacheItem(string cacheKey);

        object GetCacheItem(string cacheKey, Func<object> getCacheItem);
    }
}