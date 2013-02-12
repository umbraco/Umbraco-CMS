using System;
using Umbraco.Core;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Handles User cache invalidation/refreshing
    /// </summary>
    public class UserCacheRefresher : ICacheRefresher
    {
        public Guid UniqueIdentifier
        {
            get { return Guid.Parse(DistributedCache.UserCacheRefresherId); }
        }
        public string Name
        {
            get { return "User cache refresher"; }
        }

        public void RefreshAll()
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch("UmbracoUser");
        }

        public void Refresh(int id)
        {
            Remove(id);
        }

        public void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(string.Format("UmbracoUser{0}", id.ToString())); 
        }

        public void Refresh(Guid id)
        {
            
        }
    }
}