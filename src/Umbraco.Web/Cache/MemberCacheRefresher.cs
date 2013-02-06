using System;
using Umbraco.Core;
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
            const string getmemberCacheKey = "GetMember";

            ApplicationContext.Current.ApplicationCache.
                ClearCacheByKeySearch(string.Format("UL_{0}_{1}", getmemberCacheKey, id));
        }

        public void Remove(int id)
        {
        }

        public void Refresh(Guid id)
        {
        }

    }
}