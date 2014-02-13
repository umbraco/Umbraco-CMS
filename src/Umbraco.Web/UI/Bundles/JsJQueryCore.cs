using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary>
    /// The core Jquery libs
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient", Priority = 0, Group = 1)]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jqueryui.js", "UmbracoClient", Priority = 1, Group = 1)]
    public class JsJQueryCore : Control
    {
    }
}