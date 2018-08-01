using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary>
    /// The core libs that have no dependencies
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "Application/NamespaceManager.js", "UmbracoClient", Priority = 0, Group = 0)]
    [ClientDependency(ClientDependencyType.Javascript, "Application/UmbracoUtils.js", "UmbracoClient", Priority = 1, Group = 0)]
    public class JsApplicationLib : Control
    {
    }
}
