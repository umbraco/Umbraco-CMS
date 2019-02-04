using System.Web.UI;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using Umbraco.Core.IO;

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
        public UmbracoClientDependencyLoader()
            : base()
        {
            this.AddPath("UmbracoRoot", IOHelper.ResolveUrl(SystemDirectories.Umbraco));
            this.ProviderName = LoaderControlProvider.DefaultName;

        }

        public static ClientDependencyLoader TryCreate(Control parent, out bool isNew)
        {
            if (ClientDependencyLoader.Instance == null)
            {
                var loader = new UmbracoClientDependencyLoader();
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
