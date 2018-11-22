using System.Web.UI;
using ClientDependency.Core;

namespace Umbraco.Web.UI.Bundles
{
    /// <summary> 
    /// The umb tree libs
    /// </summary>
    [ClientDependency(ClientDependencyType.Javascript, "Tree/jquery.tree.js", "UmbracoClient", Priority = 0, Group = 10)]
    [ClientDependency(ClientDependencyType.Javascript, "Tree/UmbracoContext.js", "UmbracoClient", Priority = 1, Group = 10)]
    [ClientDependency(ClientDependencyType.Javascript, "Tree/jquery.tree.contextmenu.js", "UmbracoClient", Priority = 2, Group = 10)]
    [ClientDependency(ClientDependencyType.Javascript, "Tree/jquery.tree.checkbox.js", "UmbracoClient", Priority = 3, Group = 10)]
    [ClientDependency(ClientDependencyType.Javascript, "Tree/NodeDefinition.js", "UmbracoClient", Priority = 4, Group = 10)]
    [ClientDependency(ClientDependencyType.Javascript, "Tree/UmbracoTree.js", "UmbracoClient", Priority = 5, Group = 10)]
    public class JsUmbracoTree : Control
    {
    }
}