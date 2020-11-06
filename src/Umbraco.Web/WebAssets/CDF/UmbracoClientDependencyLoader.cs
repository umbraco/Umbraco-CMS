using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;

namespace Umbraco.Web.WebAssets.CDF
{
    /// <summary>
    /// Used to load in all client dependencies for Umbraco.
    /// Ensures that both UmbracoClient and UmbracoRoot paths are added to the loader.
    /// </summary>
    public class UmbracoClientDependencyLoader : ClientDependencyLoader
    {
        /// <summary>
        /// Set the defaults
        /// </summary>
        public UmbracoClientDependencyLoader(GlobalSettings globalSettings, IIOHelper ioHelper)
            : base()
        {
            this.AddPath("UmbracoRoot", ioHelper.ResolveUrl(globalSettings.UmbracoPath));
            this.ProviderName = LoaderControlProvider.DefaultName;

        }

        public static ClientDependencyLoader TryCreate(Control parent, out bool isNew, GlobalSettings globalSettings, IIOHelper ioHelper)
        {
            if (ClientDependencyLoader.Instance == null)
            {
                var loader = new UmbracoClientDependencyLoader(globalSettings, ioHelper);
                parent.Controls.Add(loader);
                isNew = true;
                return loader;
            }
            else
            {
                isNew = false;
                return ClientDependencyLoader.Instance;
            }

        }

    }
}
