using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Cache
{
    public sealed class DictionaryCacheRefresher : CacheRefresherBase<DictionaryCacheRefresher>
    {
        public DictionaryCacheRefresher(AppCaches appCaches)
            : base(appCaches)
        { }

        #region Define

        protected override DictionaryCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("D1D7E227-F817-4816-BFE9-6C39B6152884");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Dictionary Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(int id)
        {
            ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
            base.Remove(id);
        }

        #endregion
    }
}
