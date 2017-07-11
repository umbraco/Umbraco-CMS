using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    public sealed class UserCacheRefresher : CacheRefresherBase<UserCacheRefresher>
    {
        public UserCacheRefresher(CacheHelper cacheHelper)
            : base(cacheHelper)
        { }

        #region Define

        protected override UserCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("E057AF6D-2EE6-41F4-8045-3694010F0AA6");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "User Cache Refresher";

        #endregion

        #region Refresher

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<IUser>();
            var userPermissionsCache = CacheHelper.IsolatedRuntimeCache.GetCache<EntityPermission>();
            if (userPermissionsCache)
                userPermissionsCache.Result.ClearCacheByKeySearch(CacheKeys.UserPermissionsCacheKey);
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            var userCache = CacheHelper.IsolatedRuntimeCache.GetCache<IUser>();
            if (userCache)
                userCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IUser>(id));

            var userPermissionsCache = CacheHelper.IsolatedRuntimeCache.GetCache<EntityPermission>();
            if (userPermissionsCache)
                userPermissionsCache.Result.ClearCacheByKeySearch($"{CacheKeys.UserPermissionsCacheKey}{id}");

            base.Remove(id);
        }

        #endregion
    }
}