using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary>
    /// The core libs that require JQuery to be loaded
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "lib/umbraco/LegacyUmbClientMgr.js", "UmbracoRoot", Priority = 1, Group = 2)]
    public class JsUmbracoApplicationCore : Control
    {
    }
}
