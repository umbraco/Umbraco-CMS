using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;

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
        public UserGroupCacheRefresher(AppCaches appCaches)
            : base(appCaches)
        { }

        #region Define

        protected override UserGroupCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("45178038-B232-4FE8-AA1A-F2B949C44762");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "User Group Cache Refresher";

        #endregion

        #region Refresher

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<IUserGroup>();
            var userGroupCache = AppCaches.IsolatedCaches.Get<IUserGroup>();
            if (userGroupCache)
            {
                userGroupCache.Result.ClearByKey(UserGroupRepository.GetByAliasCacheKeyPrefix);
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
            var userGroupCache = AppCaches.IsolatedCaches.Get<IUserGroup>();
            if (userGroupCache)
            {
                userGroupCache.Result.Clear(RepositoryCacheKeys.GetKey<IUserGroup, int>(id));
                userGroupCache.Result.ClearByKey(UserGroupRepository.GetByAliasCacheKeyPrefix);
            }

            //we don't know what user's belong to this group without doing a look up so we'll need to just clear them all
            ClearAllIsolatedCacheByEntityType<IUser>();

            base.Remove(id);
        }

        #endregion
    }
}
