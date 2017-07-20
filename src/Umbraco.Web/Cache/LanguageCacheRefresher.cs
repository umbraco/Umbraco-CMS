using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

namespace Umbraco.Web.Cache
{
    public sealed class LanguageCacheRefresher : CacheRefresherBase<LanguageCacheRefresher>
    {
        public LanguageCacheRefresher(CacheHelper cacheHelper)
            : base(cacheHelper)
        { }

        #region Define

        protected override LanguageCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("3E0F95D8-0BE5-44B8-8394-2B8750B62654");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Language Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(int id)
        {
            ClearAllIsolatedCacheByEntityType<ILanguage>();
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearAllIsolatedCacheByEntityType<ILanguage>();
            //if a language is removed, then all dictionary cache needs to be removed
            ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
            base.Remove(id);
        }

        #endregion
    }
}
