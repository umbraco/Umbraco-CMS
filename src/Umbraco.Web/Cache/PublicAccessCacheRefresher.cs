using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

namespace Umbraco.Web.Cache
{
    public sealed class PublicAccessCacheRefresher : CacheRefresherBase<PublicAccessCacheRefresher>
    {
        public PublicAccessCacheRefresher(CacheHelper cacheHelper)
            : base(cacheHelper)
        { }

        #region Define

        protected override PublicAccessCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("1DB08769-B104-4F8B-850E-169CAC1DF2EC");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Public Access Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(Guid id)
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.Refresh(id);
        }

        public override void Refresh(int id)
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.Refresh(id);
        }

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.RefreshAll();
        }

        public override void Remove(int id)
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.Remove(id);
        }

        #endregion
    }
}
