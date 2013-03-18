using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{

    /// <summary>
    /// A cache refresher to ensure template cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class TemplateCacheRefresher : ICacheRefresher
    {
        
        public string Name
        {
            get
            {
                return "Template cache refresher";
            }
        }

        public Guid UniqueIdentifier
        {
            get
            {
                return new Guid(DistributedCache.TemplateRefresherId);
            }
        }

        public void RefreshAll()
        {
        }

        public void Refresh(Guid id)
        {
        }

        public void Refresh(int id)
        {
            RemoveFromCache(id);
        }

        public void Remove(int id)
        {
            RemoveFromCache(id);
        }

        private void RemoveFromCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(
                string.Format("{0}{1}", CacheKeys.TemplateCacheKey, id));
        }

    }
}