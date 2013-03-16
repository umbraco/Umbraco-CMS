using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Handles User cache invalidation/refreshing
    /// </summary>
    public sealed class UserCacheRefresher : ICacheRefresher
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
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(CacheKeys.UserCacheKey);
        }

        public void Refresh(int id)
        {
            Remove(id);
        }

        public void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(string.Format("{0}{1}", CacheKeys.UserCacheKey, id.ToString())); 
        }

        public void Refresh(Guid id)
        {
            
        }
    }
}