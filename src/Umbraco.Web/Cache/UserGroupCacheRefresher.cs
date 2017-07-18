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
    /// <remarks>
    /// This also needs to clear the user cache since IReadOnlyUserGroup's are attached to IUser objects
    /// </remarks>
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

            //We'll need to clear all user cache too
            ClearAllIsolatedCacheByEntityType<IUser>();

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

            //we don't know what user's belong to this group without doing a look up so we'll need to just clear them all
            ClearAllIsolatedCacheByEntityType<IUser>();

            base.Remove(id);
        }
        
    }
}