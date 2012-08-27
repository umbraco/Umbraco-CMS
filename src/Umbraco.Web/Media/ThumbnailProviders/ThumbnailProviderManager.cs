using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;

namespace Umbraco.Web.Media.ThumbnailProviders
{
    public class ThumbnailProviderManager
    {
        private static readonly IList<IThumbnailProvider> ProviderCache = new List<IThumbnailProvider>();

        #region Singleton
        
        private static readonly ThumbnailProviderManager Instance = new ThumbnailProviderManager();

        public static ThumbnailProviderManager Current
        {
            get { return Instance; }
        }

        #endregion

        #region Constructors

        static ThumbnailProviderManager()
        {
            PopulateCache();
        }

        #endregion

        internal IEnumerable<IThumbnailProvider> Providers
        {
            get
            {
                var providers = ProviderCache.ToList();
                providers.Sort((f1, f2) => f1.Priority.CompareTo(f2.Priority));
                return providers;
            }
        }

        internal string GetThumbnailUrl(string fileUrl)
        {
            var provider = Providers.FirstOrDefault(x => x.CanProvideThumbnail(fileUrl));
            return provider != null ? provider.GetThumbnailUrl(fileUrl) : string.Empty;
        }

        private static void PopulateCache()
        {
            var types = TypeFinder.FindClassesOfType<IThumbnailProvider>();

            foreach (var t in types)
            {
                IThumbnailProvider typeInstance = null;

                try
                {
                    //MB: Remove visible check as we've made ThumbnailProviders internal for the time being
                    //MB: We should reinstate once we make them public
                    //if (t.IsVisible)
                    //{
                        typeInstance = Activator.CreateInstance(t) as IThumbnailProvider;
                    //}
                }
                catch { }

                if (typeInstance != null)
                {
                    try
                    {
                        ProviderCache.Add(typeInstance);
                    }
                    catch (Exception ee)
                    {
                        Log.Add(LogTypes.Error, -1, "Can't import IThumbnailProvider '" + t.FullName + "': " + ee);
                    }
                }
            }
        }
    }
}
