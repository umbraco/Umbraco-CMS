using System;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Cache
{
    public sealed class UserCacheRefresher : CacheRefresherBase<UserCacheRefresherNotification>
    {
        public UserCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
            : base(appCaches, eventAggregator, factory)
        { }

        #region Define

        public static readonly Guid UniqueId = Guid.Parse("E057AF6D-2EE6-41F4-8045-3694010F0AA6");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "User Cache Refresher";

        #endregion

        #region Refresher

        public override void RefreshAll()
        {
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
            var userCache = AppCaches.IsolatedCaches.Get<IUser>();
            if (userCache)
            {
                userCache.Result.Clear(RepositoryCacheKeys.GetKey<IUser, int>(id));
                userCache.Result.ClearByKey(CacheKeys.UserContentStartNodePathsPrefix + id);
                userCache.Result.ClearByKey(CacheKeys.UserMediaStartNodePathsPrefix + id);
                userCache.Result.ClearByKey(CacheKeys.UserAllContentStartNodesPrefix + id);
                userCache.Result.ClearByKey(CacheKeys.UserAllMediaStartNodesPrefix + id);
            }


            base.Remove(id);
        }
        #endregion
    }
}
