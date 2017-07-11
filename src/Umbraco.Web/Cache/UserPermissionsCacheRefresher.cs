using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Cache
{
    public sealed class UserPermissionsCacheRefresher : CacheRefresherBase<UserPermissionsCacheRefresher>
    {
        public UserPermissionsCacheRefresher(CacheHelper cacheHelper)
            : base(cacheHelper)
        { }

        #region Define

        protected override UserPermissionsCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("840AB9C5-5C0B-48DB-A77E-29FE4B80CD3A");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "User Permissions Cache Refresher";

        #endregion

        #region Refresher

        public override void RefreshAll()
        {
            var userPermissionCache = CacheHelper.IsolatedRuntimeCache.GetCache<EntityPermission>();
            if (userPermissionCache)
                userPermissionCache.Result.ClearCacheByKeySearch(CacheKeys.UserPermissionsCacheKey);    
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            var userPermissionCache = CacheHelper.IsolatedRuntimeCache.GetCache<EntityPermission>();
            if (userPermissionCache)
                userPermissionCache.Result.ClearCacheByKeySearch($"{CacheKeys.UserPermissionsCacheKey}{id}");
            base.Remove(id);
        }

        #endregion
    }
}