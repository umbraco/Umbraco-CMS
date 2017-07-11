using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;

using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    public sealed class UserTypeCacheRefresher : CacheRefresherBase<UserTypeCacheRefresher>
    {
        public UserTypeCacheRefresher(CacheHelper cacheHelper) 
            : base(cacheHelper)
        { }

        #region Define

        protected override UserTypeCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("7E707E21-0195-4522-9A3C-658CC1761BD4");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "User Type Cache Refresher";

        #endregion

        #region Refresher

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<IUserType>();
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            var userTypeCache = CacheHelper.IsolatedRuntimeCache.GetCache<IUserType>();
            if (userTypeCache)
                userTypeCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IUserType>(id));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            var userTypeCache = CacheHelper.IsolatedRuntimeCache.GetCache<IUserType>();
            if (userTypeCache)
                userTypeCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IUserType>(id));
            base.Remove(id);
        }

        #endregion
    }
}