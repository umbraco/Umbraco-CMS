using System;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    public sealed class ApplicationTreeCacheRefresher : CacheRefresherBase<ApplicationTreeCacheRefresher>
    {
        public ApplicationTreeCacheRefresher(AppCaches appCaches)
            : base(appCaches)
        { }

        #region Define

        protected override ApplicationTreeCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("0AC6C028-9860-4EA4-958D-14D39F45886E");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Application Tree Cache Refresher";

        #endregion

        #region Refresher

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationTreeCacheKey);
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            Remove(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            AppCaches.RuntimeCache.ClearCacheItem(CacheKeys.ApplicationTreeCacheKey);
            base.Remove(id);
        }

        #endregion
    }
}
