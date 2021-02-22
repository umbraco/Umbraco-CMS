using System;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Cache
{
    public sealed class UserCacheRefresher : CacheRefresherBase<UserCacheRefresher>
    {
        public UserCacheRefresher(AppCaches appCaches)
            : base(appCaches)
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
                userCache.Result.Clear(RepositoryCacheKeys.GetKey<IUser>(id));

            base.Remove(id);
        }
        #endregion
    }
}
