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
        object GetCacheItem(string cacheKey);
        object GetCacheItem(string cacheKey, Func<object> getCacheItem);
    }
}