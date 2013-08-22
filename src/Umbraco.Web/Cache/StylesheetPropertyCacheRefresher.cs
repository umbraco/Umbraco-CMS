using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure stylesheet property cache is refreshed when stylesheet properties change
    /// </summary>
    public sealed class StylesheetPropertyCacheRefresher : CacheRefresherBase<StylesheetPropertyCacheRefresher>
    {
        protected override StylesheetPropertyCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.StylesheetPropertyCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Stylesheet property cache refresher"; }
        }

        public override void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.StylesheetPropertyCacheKey);
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetStylesheetPropertyCacheKey(id));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetStylesheetPropertyCacheKey(id));
            base.Remove(id);
        }

        private static string GetStylesheetPropertyCacheKey(int id)
        {
            return CacheKeys.StylesheetPropertyCacheKey + id;
        }
    }
}