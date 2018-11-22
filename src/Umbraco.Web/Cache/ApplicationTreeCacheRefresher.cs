using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Handles Application tree cache invalidation/refreshing
    /// </summary>
    public sealed class ApplicationTreeCacheRefresher : CacheRefresherBase<ApplicationTreeCacheRefresher>
    {
        protected override ApplicationTreeCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.ApplicationTreeCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Applications tree cache refresher"; }
        }

        public override void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationTreeCacheKey);
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationTreeCacheKey);
            base.Remove(id);
        }

    }
}