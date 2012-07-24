using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core.Interfaces;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;

namespace Umbraco.Web.Media.ThumbnailProviders
{
    public class ThumbnailProviderManager
    {
        private const string CacheKey = "ThumbnailProviderCache";

        internal static IEnumerable<IThumbnailProvider> Providers
        {
            get
            {
                EnsureCache();

                return HttpRuntime.Cache[CacheKey] as List<IThumbnailProvider>;
            }
            set
            {
                HttpRuntime.Cache.Insert(CacheKey, value);
            }
        }

        public static string GetThumbnailUrl(string fileUrl)
        {
            var provider = Providers.FirstOrDefault(x => x.CanProvideThumbnail(fileUrl));
            return provider != null ? provider.GetThumbnailUrl(fileUrl) : string.Empty;
        }

        private static void EnsureCache()
        {
            if (HttpRuntime.Cache[CacheKey] != null)
                return;

            var providers = new List<IThumbnailProvider>();
            var types = TypeFinder.FindClassesOfType<IThumbnailProvider>();

            foreach (var t in types)
            {
                IThumbnailProvider typeInstance = null;

                try
                {
                    if (t.IsVisible)
                    {
                        typeInstance = Activator.CreateInstance(t) as IThumbnailProvider;
                    }
                }
                catch { }

                if (typeInstance != null)
                {
                    try
                    {
                        providers.Add(typeInstance);
                    }
                    catch (Exception ee)
                    {
                        Log.Add(LogTypes.Error, -1, "Can't import IThumbnailProvider '" + t.FullName + "': " + ee);
                    }
                }
            }

            providers.Sort((f1, f2) => f1.Priority.CompareTo(f2.Priority));

            Providers = providers;
        }
    }
}
