using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Used to invalidate/refresh the cache for macros
    /// </summary>
    public class MacroCacheRefresher : ICacheRefresher<Macro>, ICacheRefresher<macro>
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

        public void Refresh(int id)
        {
            if (id <= 0) return;
            var m = new Macro(id);
            Remove(m);
        }

        public void Remove(int id)
        {
            if (id <= 0) return;
            var m = new Macro(id);
            Remove(m);
        }

        public void Refresh(Macro instance)
        {
            Remove(instance);
        }

        public void Remove(Macro instance)
        {
            if (instance != null && instance.Id > 0)
            {
                GetCacheKeys(instance.Alias).ForEach(
                    alias =>
                    ApplicationContext.Current.ApplicationCache.ClearCacheItem(alias));

            }
        }

        public void Refresh(macro instance)
        {
            Remove(instance);
        }

        public void Remove(macro instance)
        {
            if (instance == null || instance.Model == null) return;
            var m = instance.Model;
            GetCacheKeys(m.Alias).ForEach(
                    alias =>
                    ApplicationContext.Current.ApplicationCache.ClearCacheItem(alias));
        }
    }
}