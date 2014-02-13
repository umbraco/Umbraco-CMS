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
    [ClientDependency(ClientDependencyType.Javascript, "ui/json2.js", "UmbracoClient", Priority = 2, Group = 0)]
    [ClientDependency(ClientDependencyType.Javascript, "ui/base2.js", "UmbracoClient", Priority = 3, Group = 0)]
    [ClientDependency(ClientDependencyType.Javascript, "UI/knockout.js", "UmbracoClient", Priority = 4, Group = 0)]
    [ClientDependency(ClientDependencyType.Javascript, "UI/knockout.mapping.js", "UmbracoClient", Priority = 5, Group = 0)]
    public class JsApplicationLib : Control
    {
    }
}
