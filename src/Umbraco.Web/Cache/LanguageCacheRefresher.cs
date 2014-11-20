using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;

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
            RuntimeCacheProvider.Current.Clear(typeof(ILanguage));
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            RuntimeCacheProvider.Current.Clear(typeof(ILanguage));
            //when a language is removed we must also clear the text cache!
            global::umbraco.cms.businesslogic.language.Item.ClearCache();
            base.Remove(id);
        }
    }
}