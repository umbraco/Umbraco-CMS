using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public sealed class TemplateCacheRefresher : ICacheRefresher
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