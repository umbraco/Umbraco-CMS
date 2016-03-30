using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;

using Umbraco.Core.Persistence.Repositories;
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
            ClearAllIsolatedCacheByEntityType<IUser>();
            if (UserPermissionsCache)
                UserPermissionsCache.Result.ClearCacheByKeySearch(CacheKeys.UserPermissionsCacheKey);
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            var userCache = ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<IUser>();
            if (userCache)
                userCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IUser>(id));

            if (UserPermissionsCache)
                UserPermissionsCache.Result.ClearCacheByKeySearch(string.Format("{0}{1}", CacheKeys.UserPermissionsCacheKey, id));
            
            base.Remove(id);
        }

        private Attempt<IRuntimeCacheProvider> UserPermissionsCache
        {
            get { return ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<EntityPermission>(); }
        }

    }
}