using System;
using System.Web.UI;
using ClientDependency.Core;
using umbraco;
using umbraco.cms.businesslogic.datatype;
using System.Web.UI.HtmlControls;

namespace umbraco.editorControls
{
    /// <summary>
    /// Extension methods for embedded resources
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public static class ResourceExtensions
    {
        /// <summary>
        /// Registers the embedded client resource.
        /// </summary>
        /// <param name="ctl">The control.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="type">The type.</param>
        public static void RegisterEmbeddedClientResource(this Control ctl, string resourceName, ClientDependencyType type)
        {
            ctl.RegisterEmbeddedClientResource(ctl.GetType(), resourceName, type);
        }

        /// <summary>
        /// Registers the embedded client resource.
        /// </summary>
        /// <param name="ctl">The control.</param>
        /// <param name="resourceContainer">The resource container.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="type">The type.</param>
        public static void RegisterEmbeddedClientResource(this Control ctl, Type resourceContainer, string resourceName, ClientDependencyType type)
        {
            ctl.Page.RegisterEmbeddedClientResource(resourceContainer, resourceName, type);
        }

        /// <summary>
        /// Registers the embedded client resource.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="resourceContainer">The type containing the embedded resource</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="type">The type.</param>
        public static void RegisterEmbeddedClientResource(this Page page, Type resourceContainer, string resourceName, ClientDependencyType type)
        {
            var target = page.Header;

            // if there's no <head runat="server" /> don't throw an exception.
            if (target != null)
            {
                switch (type)
                {
                    case ClientDependencyType.Css:
                        // get the urls for the embedded resources
                        var resourceUrl = page.ClientScript.GetWebResourceUrl(resourceContainer, resourceName);
                        var link = new HtmlLink();
                        link.Attributes.Add("href", resourceUrl);
                        link.Attributes.Add("type", "text/css");
                        link.Attributes.Add("rel", "stylesheet");
                        target.Controls.Add(link);
                        break;

                    case ClientDependencyType.Javascript:
                        page.ClientScript.RegisterClientScriptResource(resourceContainer, resourceName);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}