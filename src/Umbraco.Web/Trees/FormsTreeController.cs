using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Forms)]
    [Tree(Constants.Applications.FormsInstaller, Constants.Trees.Forms, SortOrder = 0, IsSingleNodeTree = true)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class FormsTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            //full screen app without tree nodes
            return TreeNodeCollection.Empty;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            //doesn't have a menu, this is a full screen app without tree nodes
            return MenuItemCollection.Empty;
        }
    }
}
