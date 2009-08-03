using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.presentation.ClientDependency.Controls;
using umbraco.presentation.ClientDependency.Providers;
using umbraco.presentation.ClientDependency.Config;
using System.Web.UI;

namespace umbraco.uicontrols
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
            this.AddPath("UmbracoClient", GlobalSettings.ClientPath);
            this.AddPath("UmbracoRoot", GlobalSettings.Path);
            this.ProviderName = PageHeaderProvider.DefaultName;            
        }

        public static new ClientDependencyLoader TryCreate(Control parent, out bool isNew)
        {
            if (ClientDependencyLoader.Instance == null)
            {
                UmbracoClientDependencyLoader loader = new UmbracoClientDependencyLoader();
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
