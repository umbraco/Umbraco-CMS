using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary>
    /// The core Jquery libs
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "lib/jquery/jquery.min.js", "UmbracoRoot", Priority = 1, Group = 1)]
    [ClientDependency(ClientDependencyType.Javascript, "lib/jquery-migrate/jquery-migrate.min.js", "UmbracoRoot", Priority = 2, Group = 1)]
    public class JsJQueryCore : Control
    {
    }
}
