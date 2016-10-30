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
    /// The UserGroupCacheRefresher will also clear a groups's permissions cache, this refresher is for invalidating only permissions
    /// for user groups/content, not the groups themselves.
    /// </remarks>
    public sealed class UserGroupPermissionsCacheRefresher : CacheRefresherBase<UserGroupPermissionsCacheRefresher>
    {
        protected override UserGroupPermissionsCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.UserGroupPermissionsCacheRefresherId); }
        }


        public override string Name
        {
            get { return "User group permissions cache refresher"; }
        }

        public override void RefreshAll()
        {
            if (UserGroupPermissionsCache)
                UserGroupPermissionsCache.Result.ClearCacheByKeySearch(CacheKeys.UserGroupPermissionsCacheKey);    
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            if (UserGroupPermissionsCache)
                UserGroupPermissionsCache.Result.ClearCacheByKeySearch(string.Format("{0}{1}", CacheKeys.UserGroupPermissionsCacheKey, id));
            base.Remove(id);
        }

        private Attempt<IRuntimeCacheProvider> UserGroupPermissionsCache
        {
            get { return ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<EntityPermission>(); }
        }
    }
}