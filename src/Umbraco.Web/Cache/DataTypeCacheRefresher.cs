using System;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure member cache is updated when members change
    /// </summary>    
    public sealed class DataTypeCacheRefresher : CacheRefresherBase<DataTypeCacheRefresher>
    {

        protected override DataTypeCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.DataTypeCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Clears data type cache"; }
        }

        public override void Refresh(int id)
        {
            ClearCache(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearCache(id);
            base.Remove(id);
        }

        private void ClearCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.
                               ClearCacheByKeySearch(string.Format("{0}{1}", CacheKeys.DataTypeCacheKey, id));            
        }
    }
}