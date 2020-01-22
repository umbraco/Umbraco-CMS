using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
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
        public UmbracoClientDependencyLoader(IGlobalSettings globalSettings)
            : base()
        {
            this.AddPath("UmbracoRoot", Current.IOHelper.ResolveUrl(globalSettings.UmbracoPath));
            this.ProviderName = LoaderControlProvider.DefaultName;

        }

        public static ClientDependencyLoader TryCreate(Control parent, out bool isNew)
        {
            if (ClientDependencyLoader.Instance == null)
            {
                var loader = new UmbracoClientDependencyLoader(Current.Factory.GetInstance<IGlobalSettings>());
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
