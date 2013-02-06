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
                return new Guid("DD12B6A0-14B9-46e8-8800-C154F74047C8");
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