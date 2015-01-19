using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;


namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure the dictionary cache is refreshed when dictionary change
    /// </summary>
    public sealed class DictionaryCacheRefresher : CacheRefresherBase<DictionaryCacheRefresher>
    {
        protected override DictionaryCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.DictionaryCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Dictionary cache refresher"; }
        }

        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IDictionaryItem>();
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IDictionaryItem>();
            base.Remove(id);
        }
    }
}