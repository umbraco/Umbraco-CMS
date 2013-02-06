using System;
using Umbraco.Core;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
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
            ApplicationContext.Current.ApplicationCache.ClearCacheForTemplate(id);
        }

        public void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheForTemplate(id);
        }

    }
}