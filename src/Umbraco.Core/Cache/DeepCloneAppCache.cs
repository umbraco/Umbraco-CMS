using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Implements <see cref="IAppPolicyCache"/> by wrapping an inner other <see cref="IAppPolicyCache"/>
    /// instance, and ensuring that all inserts and returns are deep cloned copies of the cache item,
    /// when the item is deep-cloneable.
    /// </summary>
    internal class DeepCloneAppCache : IAppPolicyCache, IDisposable
    {
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeepCloneAppCache"/> class.
        /// </summary>
        public DeepCloneAppCache(IAppPolicyCache innerCache)
        {
            var type = typeof (DeepCloneAppCache);

            if (innerCache.GetType() == type)
                throw new InvalidOperationException($"A {type} cannot wrap another instance of itself.");

            InnerCache = innerCache;
        }

        /// <summary>
        /// Gets the inner cache.
        /// </summary>
        public IAppPolicyCache InnerCache { get; }

        /// <inheritdoc />
        public object Get(string key)
        {
            var item = InnerCache.Get(key);
            return CheckCloneableAndTracksChanges(item);
        }

        /// <inheritdoc />
        public object Get(string key, Func<object> factory)
        {
            var cached = InnerCache.Get(key, () =>
            {
                var result = FastDictionaryAppCacheBase.GetSafeLazy(factory);
                var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
                // do not store null values (backward compat), clone / reset to go into the cache
                return value == null ? null : CheckCloneableAndTracksChanges(value);
            });
            return CheckCloneableAndTracksChanges(cached);
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByKey(string keyStartsWith)
        {
            return InnerCache.SearchByKey(keyStartsWith)
                .Select(CheckCloneableAndTracksChanges);
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByRegex(string regex)
        {
            return InnerCache.SearchByRegex(regex)
                .Select(CheckCloneableAndTracksChanges);
        }

        /// <inheritdoc />
        public object Get(string key, Func<object> factory, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            var cached = InnerCache.Get(key, () =>
            {
                var result = FastDictionaryAppCacheBase.GetSafeLazy(factory);
                var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
                // do not store null values (backward compat), clone / reset to go into the cache
                return value == null ? null : CheckCloneableAndTracksChanges(value); 

                // clone / reset to go into the cache
            }, timeout, isSliding, priority, removedCallback, dependentFiles);

            // clone / reset to go into the cache
            return CheckCloneableAndTracksChanges(cached);
        }

        /// <inheritdoc />
        public void Insert(string key, Func<object> factory, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            InnerCache.Insert(key, () =>
            {
                var result = FastDictionaryAppCacheBase.GetSafeLazy(factory);
                var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
                // do not store null values (backward compat), clone / reset to go into the cache
                return value == null ? null : CheckCloneableAndTracksChanges(value);
            }, timeout, isSliding, priority, removedCallback, dependentFiles);
        }

        /// <inheritdoc />
        public void Clear()
        {
            InnerCache.Clear();
        }

        /// <inheritdoc />
        public void Clear(string key)
        {
            InnerCache.Clear(key);
        }

        /// <inheritdoc />
        public void ClearOfType(string typeName)
        {
            InnerCache.ClearOfType(typeName);
        }

        /// <inheritdoc />
        public void ClearOfType<T>()
        {
            InnerCache.ClearOfType<T>();
        }

        /// <inheritdoc />
        public void ClearOfType<T>(Func<string, T, bool> predicate)
        {
            InnerCache.ClearOfType<T>(predicate);
        }

        /// <inheritdoc />
        public void ClearByKey(string keyStartsWith)
        {
            InnerCache.ClearByKey(keyStartsWith);
        }

        /// <inheritdoc />
        public void ClearByRegex(string regex)
        {
            InnerCache.ClearByRegex(regex);
        }

        private static object CheckCloneableAndTracksChanges(object input)
        {
            if (input is IDeepCloneable cloneable)
            {
                input = cloneable.DeepClone();
            }

            // reset dirty initial properties
            if (input is IRememberBeingDirty tracksChanges)
            {
                tracksChanges.ResetDirtyProperties(false);
                input = tracksChanges;
            }

            return input;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    InnerCache.DisposeIfDisposable();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
