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
    /// Ensures that all inserts and returns are a deep cloned copy of the item when
    /// the item is IDeepCloneable
    /// </summary>
    internal class DeepCloneRuntimeCacheProvider : IRuntimeCacheProvider
    {
        private readonly IRuntimeCacheProvider _innerProvider;

        public DeepCloneRuntimeCacheProvider(IRuntimeCacheProvider innerProvider)
        {
            _innerProvider = innerProvider;
        }

        #region Clear - doesn't require any changes
        public void ClearAllCache()
        {
            _innerProvider.ClearAllCache();
        }

        public void ClearCacheItem(string key)
        {
            _innerProvider.ClearCacheItem(key);
        }

        public void ClearCacheObjectTypes(string typeName)
        {
            _innerProvider.ClearCacheObjectTypes(typeName);
        }

        public void ClearCacheObjectTypes<T>()
        {
            _innerProvider.ClearCacheObjectTypes<T>();
        }

        public void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            _innerProvider.ClearCacheObjectTypes<T>(predicate);
        }

        public void ClearCacheByKeySearch(string keyStartsWith)
        {
            _innerProvider.ClearCacheByKeySearch(keyStartsWith);
        }

        public void ClearCacheByKeyExpression(string regexString)
        {
            _innerProvider.ClearCacheByKeyExpression(regexString);
        } 
        #endregion

        public IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return _innerProvider.GetCacheItemsByKeySearch(keyStartsWith)
                .Select(CheckCloneableAndTracksChanges);
        }

        public IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            return _innerProvider.GetCacheItemsByKeyExpression(regexString)
                .Select(CheckCloneableAndTracksChanges);
        }
        
        public object GetCacheItem(string cacheKey)
        {
            var item = _innerProvider.GetCacheItem(cacheKey);
            return CheckCloneableAndTracksChanges(item);
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return  _innerProvider.GetCacheItem(cacheKey, () =>
            {
                //Resolve the item but returned the cloned/reset item
                var item = getCacheItem();
                return CheckCloneableAndTracksChanges(item);
            });
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            return _innerProvider.GetCacheItem(cacheKey, () =>
            {
                //Resolve the item but returned the cloned/reset item
                var item = getCacheItem();
                return CheckCloneableAndTracksChanges(item);
            }, timeout, isSliding, priority, removedCallback, dependentFiles);         
        }

        public void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            _innerProvider.InsertCacheItem(cacheKey, () =>
            {
                //Resolve the item but returned the cloned/reset item
                var item = getCacheItem();
                return CheckCloneableAndTracksChanges(item);
            }, timeout, isSliding, priority, removedCallback, dependentFiles);   
        }

        private static object CheckCloneableAndTracksChanges(object input)
        {
            var entity = input as IDeepCloneable;
            if (entity == null) return input;

            var cloned = entity.DeepClone();
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            var tracksChanges = cloned as TracksChangesEntityBase;
            if (tracksChanges != null)
            {
                tracksChanges.ResetDirtyProperties(false);
                return tracksChanges;
            }
            return cloned;
        }
    }
}