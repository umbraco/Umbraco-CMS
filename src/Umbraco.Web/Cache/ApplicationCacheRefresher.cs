using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Handles Application cache invalidation/refreshing
    /// </summary>
    public sealed class ApplicationCacheRefresher : CacheRefresherBase<ApplicationCacheRefresher>
    {
        
        public ApplicationCacheRefresher(CacheHelper cacheHelper) : base(cacheHelper)
        {
        }

        protected override ApplicationCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.ApplicationCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Applications cache refresher"; }
        }

        public override void RefreshAll()
        {
            CacheHelper.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationsCacheKey);
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            CacheHelper.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationsCacheKey);
            base.Remove(id);
        }

    }
}