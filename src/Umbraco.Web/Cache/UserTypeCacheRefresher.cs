using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;

using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Handles User type cache invalidation/refreshing
    /// </summary>
    public sealed class UserTypeCacheRefresher : CacheRefresherBase<UserTypeCacheRefresher>
    {
        protected override UserTypeCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.UserTypeCacheRefresherId); }
        }

        public override string Name
        {
            get { return "User type cache refresher"; }
        }

        public override void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IUserType>();
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(RepositoryBase.GetCacheIdKey<IUserType>(id));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(RepositoryBase.GetCacheIdKey<IUserType>(id));
            base.Remove(id);
        }

    }
}