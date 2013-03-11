using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public class MediaCacheRefresher : ICacheRefresher
    {
        public Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.MediaCacheRefresherId); }
        }

        public string Name
        {
            get { return "Clears Media Cache from umbraco.library"; }
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

        private static void ClearCache(int id)
        {
            var m = ApplicationContext.Current.Services.MediaService.GetById(id);
            if (m == null) return;

            foreach (var idPart in m.Path.Split(','))
            {
                ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(
                    string.Format("UL_{0}_{1}_True", CacheKeys.GetMediaCacheKey, idPart));

                // Also clear calls that only query this specific item!
                if (idPart == m.Id.ToString())
                    ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(
                        string.Format("UL_{0}_{1}", CacheKeys.GetMediaCacheKey, id));

            }
        }

    }
}