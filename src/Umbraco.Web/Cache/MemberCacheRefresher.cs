using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public class MemberCacheRefresher : ICacheRefresher
    {

        public Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.MemberCacheRefresherId); }
        }

        public string Name
        {
            get { return "Clears Member Cache from umbraco.library"; }
        }

        public void RefreshAll()
        {
        }

        public void Refresh(int id)
        {
            ClearCache(id);
        }

        public void Remove(int id)
        {
            ClearCache(id);
        }

        public void Refresh(Guid id)
        {
            
        }

        private void ClearCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.
                ClearCacheByKeySearch(string.Format("UL_{0}_{1}", CacheKeys.GetMemberCacheKey, id));
        }

    }
}