using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Implements <see cref="IAppPolicyCache"/> and do not cache.
    /// </summary>
    public class NoAppCache : IAppPolicyCache
    {
        private NoAppCache() { }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static NoAppCache Instance { get; } = new NoAppCache();

        /// <inheritdoc />
        public virtual object Get(string cacheKey)
        {
            return null;
        }

        /// <inheritdoc />
        public virtual object Get(string cacheKey, Func<object> factory)
        {
            return factory();
        }

        /// <inheritdoc />
        public virtual IEnumerable<object> SearchByKey(string keyStartsWith)
        {
            return Enumerable.Empty<object>();
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByRegex(string regex)
        {
            return Enumerable.Empty<object>();
        }

        /// <inheritdoc />
        public object Get(string key, Func<object> factory, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            return factory();
        }

        /// <inheritdoc />
        public void Insert(string key, Func<object> factory, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        { }

        /// <inheritdoc />
        public virtual void Clear()
        { }

        /// <inheritdoc />
        public virtual void Clear(string key)
        { }

        /// <inheritdoc />
        public virtual void ClearOfType(string typeName)
        { }

        /// <inheritdoc />
        public virtual void ClearOfType<T>()
        { }

        /// <inheritdoc />
        public virtual void ClearOfType<T>(Func<string, T, bool> predicate)
        { }

        /// <inheritdoc />
        public virtual void ClearByKey(string keyStartsWith)
        { }

        /// <inheritdoc />
        public virtual void ClearByRegex(string regex)
        { }
    }
}
