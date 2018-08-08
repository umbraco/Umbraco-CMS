using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary>
    /// The core libs that have no dependencies
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "lib/umbraco/NamespaceManager.js", "UmbracoRoot", Priority = 0, Group = 0)]
    public class JsApplicationLib : Control
    {
    }
}
