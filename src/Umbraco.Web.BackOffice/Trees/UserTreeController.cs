using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Routing;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Users)]
    [Tree(Constants.Applications.Users, Constants.Trees.Users, SortOrder = 0, IsSingleNodeTree = true)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class UserTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

        public UserTreeController(
            IMenuItemCollectionFactory menuItemCollectionFactory,
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection
            ) : base(localizedTextService, umbracoApiControllerTypeCollection)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Users}/{Constants.Trees.Users}/users";
            root.Icon = Constants.Icons.UserGroup;

            root.HasChildren = false;
            return root;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormCollection queryStrings)
        {
            //full screen app without tree nodes
            return TreeNodeCollection.Empty;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormCollection queryStrings)
        {
            //doesn't have a menu, this is a full screen app without tree nodes
            return _menuItemCollectionFactory.Create();
        }
    }
}
