using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.LegacyXmlCache;

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
            ClearCache();
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearCache();
            base.Remove(id);
        }

        private void ClearCache()
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.DomainCacheKey);
            //we need to clear the routes cache here!                    
            var contentCache = PublishedContentCacheResolver.Current.ContentCache as PublishedContentCache;
            if (contentCache != null)
            {
                contentCache.RoutesCache.Clear();
            }
        }
    }
}