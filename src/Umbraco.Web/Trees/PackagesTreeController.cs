using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Packages)]
    [Tree(Constants.Applications.Packages, Constants.Trees.Packages, SortOrder = 0, IsSingleNodeTree = true)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class PackagesTreeController : TreeController
    {
        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Packages}/{Constants.Trees.Packages}/repo";
            root.Icon = Constants.Icons.Packages;

            root.HasChildren = false;
            return root;
        }


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
