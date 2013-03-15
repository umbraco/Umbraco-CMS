using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public class MediaCacheRefresher : ICacheRefresher<IMedia>
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
            ClearCache(ApplicationContext.Current.Services.MediaService.GetById(id));
        }

        public void Remove(int id)
        {
            ClearCache(ApplicationContext.Current.Services.MediaService.GetById(id));
        }

        public void Refresh(Guid id)
        {
        }

        public void Refresh(IMedia instance)
        {
            ClearCache(instance);
        }

        public void Remove(IMedia instance)
        {
            ClearCache(instance);
        }

        private static void ClearCache(IMedia media)
        {
            if (media == null) return;

            foreach (var idPart in media.Path.Split(','))
            {
                ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(
                    string.Format("{0}_{1}_True", CacheKeys.MediaCacheKey, idPart));

                // Also clear calls that only query this specific item!
                if (idPart == media.Id.ToString())
                    ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(
                        string.Format("{0}_{1}", CacheKeys.MediaCacheKey, media.Id));

            }
        }
    }
}