using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Handles User group cache invalidation/refreshing
    /// </summary>
    public sealed class UserGroupCacheRefresher : CacheRefresherBase<UserGroupCacheRefresher>
    {
        protected override UserGroupCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.UserGroupCacheRefresherId); }
        }

        public override string Name
        {
            get { return "User group cache refresher"; }
        }

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<IUserGroup>();
            var userGroupCache = ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<IUserGroup>();
            if (userGroupCache)
            {
                userGroupCache.Result.ClearCacheByKeySearch(UserGroupRepository.GetByAliasCacheKeyPrefix);
            }

            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            var userGroupCache = ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<IUserGroup>();
            if (userGroupCache)
            {
                userGroupCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IUserGroup>(id));
                userGroupCache.Result.ClearCacheByKeySearch(UserGroupRepository.GetByAliasCacheKeyPrefix);
            }
            
            base.Remove(id);
        }
    }
}