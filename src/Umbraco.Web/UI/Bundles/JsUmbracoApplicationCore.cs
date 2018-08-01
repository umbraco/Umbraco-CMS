using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary>
    /// The core libs that require JQuery to be loaded
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "Application/UmbracoApplicationActions.js", "UmbracoClient", Priority = 1, Group = 2)]
    public class JsUmbracoApplicationCore : Control
    {
    }
}
