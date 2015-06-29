using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;


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
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<ILanguage>();
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<ILanguage>();
            base.Remove(id);
        }
    }
}