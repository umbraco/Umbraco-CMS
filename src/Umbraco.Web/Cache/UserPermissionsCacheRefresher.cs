using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Used only to invalidate the user permissions cache
    /// </summary>
    /// <remarks>
    /// The UserCacheRefresher will also clear a user's permissions cache, this refresher is for invalidating only permissions
    /// for users/content, not the users themselves.
    /// </remarks>
    public sealed class UserPermissionsCacheRefresher : CacheRefresherBase<UserPermissionsCacheRefresher>
    {
        protected override UserPermissionsCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.UserPermissionsCacheRefresherId); }
        }


        public override string Name
        {
            get { return "User permissions cache refresher"; }
        }

        public override void RefreshAll()
        {
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