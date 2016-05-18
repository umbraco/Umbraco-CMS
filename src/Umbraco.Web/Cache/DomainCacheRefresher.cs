using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure language cache is refreshed when languages change
    /// </summary>
    public sealed class DomainCacheRefresher : CacheRefresherBase<DomainCacheRefresher>
    {
        public DomainCacheRefresher(CacheHelper cacheHelper) : base(cacheHelper)
        {
        }

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

        public override void RefreshAll()
        {
            ClearCache();
            base.RefreshAll();
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
            ClearAllIsolatedCacheByEntityType<IDomain>();

            //TODO: Fix this - if we keep this logic here to clear this cache, then  we'll need to use IUmbracoContextAccessor since
            // instances of the cache refreshers are singletons, not transient
            //
            // SD: we need to clear the routes cache here!             
            // zpqrtbnk: no, not here, in fact the caches should subsribe to refresh events else we
            // are creating a nasty dependency - but keep it like that for the time being while
            // SD is cleaning cache refreshers up.
            if (UmbracoContext.Current != null)
            {
                var contentCache = UmbracoContext.Current.ContentCache.InnerCache as PublishedContentCache;
                if (contentCache != null)
                    contentCache.RoutesCache.Clear();    
            }
        }
    }
}