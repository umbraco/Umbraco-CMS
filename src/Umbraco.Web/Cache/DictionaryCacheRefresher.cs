using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Caching;

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
            RuntimeCacheProvider.Current.Clear(typeof(IDictionaryItem));
            global::umbraco.cms.businesslogic.Dictionary.ClearCache();
            //when a dictionary item is updated we must also clear the text cache!
            global::umbraco.cms.businesslogic.language.Item.ClearCache();
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            RuntimeCacheProvider.Current.Clear(typeof(IDictionaryItem));
            global::umbraco.cms.businesslogic.Dictionary.ClearCache();
            //when a dictionary item is removed we must also clear the text cache!
            global::umbraco.cms.businesslogic.language.Item.ClearCache();
            base.Remove(id);
        }
    }
}