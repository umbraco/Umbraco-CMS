using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure stylesheet cache is refreshed when stylesheets change
    /// </summary>
    public sealed class StylesheetCacheRefresher : CacheRefresherBase<StylesheetCacheRefresher>
    {
        protected override StylesheetCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.StylesheetCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Stylesheet cache refresher"; }
        }

        public override void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.StylesheetCacheKey);
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetStylesheetCacheKey(id));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetStylesheetCacheKey(id));
            base.Remove(id);
        }

        private static string GetStylesheetCacheKey(int id)
        {
            return CacheKeys.StylesheetCacheKey + id;
        }
    }
}