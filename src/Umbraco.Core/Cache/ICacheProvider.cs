using System;
using System.Collections.Generic;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// An abstract class for implementing a basic cache provider
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        void ClearAllCache();

        /// <summary>
        /// Removes an item from the cache, identified by its key.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        void ClearCacheItem(string key);

        /// <summary>
        /// Removes items from the cache, of a specified type.
        /// </summary>
        /// <param name="typeName">The name of the type to remove.</param>
        /// <remarks>
        /// <para>If the type is an interface, then all items of a type implementing that interface are
        /// removed. Otherwise, only items of that exact type are removed (items of type inheriting from
        /// the specified type are not removed).</para>
        /// <para>Performs a case-sensitive search.</para>
        /// </remarks>
        void ClearCacheObjectTypes(string typeName);

        /// <summary>
        /// Removes items from the cache, of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the items to remove.</typeparam>
        /// <remarks>If the type is an interface, then all items of a type implementing that interface are
        /// removed. Otherwise, only items of that exact type are removed (items of type inheriting from
        /// the specified type are not removed).</remarks>
        void ClearCacheObjectTypes<T>();

        /// <summary>
        /// Removes items from the cache, of a specified type, satisfying a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the items to remove.</typeparam>
        /// <param name="predicate">The predicate to satisfy.</param>
        /// <remarks>If the type is an interface, then all items of a type implementing that interface are
        /// removed. Otherwise, only items of that exact type are removed (items of type inheriting from
        /// the specified type are not removed).</remarks>
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