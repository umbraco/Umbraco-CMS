using System.Net.Http.Formatting;
using umbraco;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Users)]
    [Tree(Constants.Applications.Users, Constants.Trees.Users, "Users", sortOrder: 0)]
    [PluginController("UmbracoTrees")]
    [LegacyBaseTree(typeof(loadUsers))]
    [CoreTree]
    public class UserTreeController : TreeController
    {
        public UserTreeController()
        {
        }

        public UserTreeController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }

        public UserTreeController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper) : base(umbracoContext, umbracoHelper)
        {
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = string.Format("{0}/{1}/{2}", Constants.Applications.Users, Constants.Trees.Users, "overview");
            root.Icon = "icon-users";

            root.HasChildren = false;
            return root;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var baseUrl = Constants.Applications.Users + "/users/";

            var nodes = new TreeNodeCollection();
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();
            return menu;
        }
    }
}