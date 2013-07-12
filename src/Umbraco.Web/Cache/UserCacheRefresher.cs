using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Handles User cache invalidation/refreshing
    /// </summary>
    public sealed class UserCacheRefresher : CacheRefresherBase<UserCacheRefresher>
    {
        protected override UserCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.UserCacheRefresherId); }
        }

        public override string Name
        {
            get { return "User cache refresher"; }
        }

        public override void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.UserCacheKey);
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.UserPermissionsCacheKey);
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.UserContextCacheKey);
        }

        public override void Refresh(int id)
        {
            Remove(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(string.Format("{0}{1}", CacheKeys.UserCacheKey, id));
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(string.Format("{0}{1}", CacheKeys.UserPermissionsCacheKey, id));

            //we need to clear all UserContextCacheKey since we cannot invalidate based on ID since the cache is done so based
            //on the current contextId stored in the database
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.UserContextCacheKey);
        }

    }
}