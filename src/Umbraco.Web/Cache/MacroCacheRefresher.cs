using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using System.Linq;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure macro cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class MacroCacheRefresher : ICacheRefresher<Macro>, ICacheRefresher<macro>
    {
        internal static string[] GetAllMacroCacheKeys()
        {
            return new[]
                {
                    CacheKeys.MacroCacheKey,
                    CacheKeys.MacroControlCacheKey,
                    CacheKeys.MacroHtmlCacheKey,
                    CacheKeys.MacroHtmlDateAddedCacheKey,
                    CacheKeys.MacroControlDateAddedCacheKey
                };
        }

        internal static string[] GetCacheKeysForAlias(string alias)
        {
            return GetAllMacroCacheKeys().Select(x => x + alias).ToArray();
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
            ApplicationContext.Current.ApplicationCache.ClearCacheObjectTypes<MacroCacheContent>();
            GetAllMacroCacheKeys().ForEach(
                    prefix =>
                    ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(prefix));
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
                GetCacheKeysForAlias(instance.Alias).ForEach(
                    alias =>
                    ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(alias));

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
            GetCacheKeysForAlias(m.Alias).ForEach(
                    alias =>
                    ApplicationContext.Current.ApplicationCache.ClearCacheByKeySearch(alias));
        }
    }
}