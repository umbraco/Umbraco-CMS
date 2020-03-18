using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Composing;

namespace Umbraco.Web.JavaScript
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
        public UmbracoClientDependencyLoader(IGlobalSettings globalSettings, IIOHelper ioHelper)
            : base()
        {
            this.AddPath("UmbracoRoot", ioHelper.ResolveUrl(globalSettings.UmbracoPath));
            this.ProviderName = LoaderControlProvider.DefaultName;

        }

        public static ClientDependencyLoader TryCreate(Control parent, out bool isNew, IGlobalSettings globalSettings, IIOHelper ioHelper)
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
