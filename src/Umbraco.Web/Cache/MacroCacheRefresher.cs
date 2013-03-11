using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Used to invalidate/refresh the cache for macros
    /// </summary>
    public class MacroCacheRefresher : ICacheRefresher<macro>
    {
        internal static string[] GetCacheKeys(string alias)
        {
            return new[] { CacheKeys.MacroRuntimeCacheKey + alias, CacheKeys.UmbracoMacroCacheKey + alias };
        }

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
            if (id <= 0) return;
            var m = new macro(id);
            Remove(m);
        }

        void ICacheRefresher.Remove(int id)
        {
            if (id <= 0) return;
            var m = new macro(id);
            Remove(m);
        }

        public void Refresh(macro instance)
        {
            Remove(instance);
        }

        public void Remove(macro instance)
        {
            if (instance != null && instance.Model != null && instance.Model.Id > 0)
            {
                GetCacheKeys(instance.Model.Alias).ForEach(
                    alias =>
                    ApplicationContext.Current.ApplicationCache.ClearCacheItem(alias));

            }
        }
    }
}