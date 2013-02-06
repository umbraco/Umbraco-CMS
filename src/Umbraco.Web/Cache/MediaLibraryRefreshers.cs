using System;
using Umbraco.Core;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    public class MediaLibraryRefreshers : ICacheRefresher
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
            ApplicationContext.Current.ApplicationCache.ClearLibraryCacheForMedia(id, false);
        }

        public void Remove(int id)
        {
        }

        public void Refresh(Guid id)
        {
        }

    }
}