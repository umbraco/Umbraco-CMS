using System;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    public sealed class ApplicationCacheRefresher : CacheRefresherBase<ApplicationCacheRefresher>
    {
        public ApplicationCacheRefresher(CacheHelper cacheHelper)
            : base(cacheHelper)
        { }

        #region Define

        protected override ApplicationCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("B15F34A1-BC1D-4F8B-8369-3222728AB4C8");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Application Cache Refresher";

        #endregion

        #region Refresher

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

        #endregion
    }
}
