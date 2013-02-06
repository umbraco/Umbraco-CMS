using System;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Used to invalidate/refresh the cache for macros
    /// </summary>
    public class MacroCacheRefresher : ICacheRefresher
    {
        public string Name
        {
            get
            {
                return "Macro cache refresher";
            }
        }

        public Guid UniqueIdentifier
        {
            get
            {
                return new Guid(DistributedCache.MacroCacheRefresherId);
            }
        }

        public void RefreshAll()
        {
        }

        public void Refresh(Guid id)
        {
        }

        void ICacheRefresher.Refresh(int id)
        {
            macro.GetMacro(id).removeFromCache();
        }

        void ICacheRefresher.Remove(int id)
        {
            macro.GetMacro(id).removeFromCache();
        }

    }
}