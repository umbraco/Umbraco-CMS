using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure language cache is refreshed when languages change
    /// </summary>
    public sealed class LanguageCacheRefresher : CacheRefresherBase<LanguageCacheRefresher>
    {
        protected override LanguageCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.LanguageCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Language cache refresher"; }
        }

        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.LanguageCacheKey);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(CacheKeys.LanguageCacheKey);
            base.Remove(id);
        }
    }
}