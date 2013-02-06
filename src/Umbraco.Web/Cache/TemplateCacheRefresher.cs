using System;
using Umbraco.Core;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public class TemplateCacheRefresher : ICacheRefresher
    {
        private const string TemplateCacheKey = "template";

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
                string.Format("{0}{1}", TemplateCacheKey, id));
        }

    }
}