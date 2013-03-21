using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure language cache is refreshed when languages change
    /// </summary>
    public sealed class DomainCacheRefresher : CacheRefresherBase<DomainCacheRefresher>
    {
        protected override DomainCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.DomainCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Domain cache refresher"; }
        }

        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.DomainCacheKey);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.DomainCacheKey);
            base.Remove(id);
        }
    }
}