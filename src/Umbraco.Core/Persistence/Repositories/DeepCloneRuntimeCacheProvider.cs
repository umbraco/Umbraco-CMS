using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// A wrapper for any IRuntimeCacheProvider that ensures that all inserts and returns 
    /// are a deep cloned copy of the item when the item is IDeepCloneable and that tracks changes are
    /// reset if the object is TracksChangesEntityBase
    /// </summary>
    internal class DeepCloneRuntimeCacheProvider : IRuntimeCacheProvider
    {
        internal IRuntimeCacheProvider InnerProvider { get; private set; }

        public DeepCloneRuntimeCacheProvider(IRuntimeCacheProvider innerProvider)
        {
            InnerProvider = innerProvider;
        }

        #region Clear - doesn't require any changes
        public void ClearAllCache()
        {
            InnerProvider.ClearAllCache();
        }

        public void ClearCacheItem(string key)
        {
            InnerProvider.ClearCacheItem(key);
        }

        public void ClearCacheObjectTypes(string typeName)
        {
            InnerProvider.ClearCacheObjectTypes(typeName);
        }

        public void ClearCacheObjectTypes<T>()
        {
            InnerProvider.ClearCacheObjectTypes<T>();
        }

        public void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            InnerProvider.ClearCacheObjectTypes<T>(predicate);
        }

        public void ClearCacheByKeySearch(string keyStartsWith)
        {
            InnerProvider.ClearCacheByKeySearch(keyStartsWith);
        }

        public void ClearCacheByKeyExpression(string regexString)
        {
            InnerProvider.ClearCacheByKeyExpression(regexString);
        } 
        #endregion

        public IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return InnerProvider.GetCacheItemsByKeySearch(keyStartsWith)
                .Select(CheckCloneableAndTracksChanges);
        }

        public IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            return InnerProvider.GetCacheItemsByKeyExpression(regexString)
                .Select(CheckCloneableAndTracksChanges);
        }
        
        public object GetCacheItem(string cacheKey)
        {
            var item = InnerProvider.GetCacheItem(cacheKey);
            return CheckCloneableAndTracksChanges(item);
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return  InnerProvider.GetCacheItem(cacheKey, () =>
            {
                var result = DictionaryCacheProviderBase.GetSafeLazy(getCacheItem);
                var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
                if (value == null) return null; // do not store null values (backward compat)

                return CheckCloneableAndTracksChanges(value);
            });
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            return InnerProvider.GetCacheItem(cacheKey, () =>
            {
                var result = DictionaryCacheProviderBase.GetSafeLazy(getCacheItem);
                var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
                if (value == null) return null; // do not store null values (backward compat)

                return CheckCloneableAndTracksChanges(value);
            }, timeout, isSliding, priority, removedCallback, dependentFiles);         
        }

        public void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            InnerProvider.InsertCacheItem(cacheKey, () =>
            {
                var result = DictionaryCacheProviderBase.GetSafeLazy(getCacheItem);
                var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
                if (value == null) return null; // do not store null values (backward compat)

                return CheckCloneableAndTracksChanges(value);
            }, timeout, isSliding, priority, removedCallback, dependentFiles);   
        }

        private static object CheckCloneableAndTracksChanges(object input)
        {
            var cloneable = input as IDeepCloneable;
            if (cloneable != null)
            {
                input = cloneable.DeepClone();    
            }

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            var tracksChanges = input as IRememberBeingDirty;
            if (tracksChanges != null)
            {
                tracksChanges.ResetDirtyProperties(false);
                input =  tracksChanges;
            }

            return input;
        }
    }
}