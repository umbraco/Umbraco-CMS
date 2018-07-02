using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

namespace Umbraco.Web.Cache
{
    public sealed class DictionaryCacheRefresher : CacheRefresherBase<DictionaryCacheRefresher>
    {
        public DictionaryCacheRefresher(CacheHelper cacheHelper)
            : base(cacheHelper)
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
