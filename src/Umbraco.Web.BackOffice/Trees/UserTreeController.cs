using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[Authorize(Policy = AuthorizationPolicies.TreeAccessUsers)]
[Tree(Constants.Applications.Users, Constants.Trees.Users, SortOrder = 0, IsSingleNodeTree = true)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public class UserTreeController : TreeController
{
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

    public UserTreeController(
        IMenuItemCollectionFactory menuItemCollectionFactory,
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator
    ) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator) =>
        _menuItemCollectionFactory = menuItemCollectionFactory;

    /// <summary>
    ///     Helper method to create a root model for a tree
    /// </summary>
    /// <returns></returns>
    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        ActionResult<TreeNode?> rootResult = base.CreateRootNode(queryStrings);
        if (!(rootResult.Result is null))
        {
            return rootResult.Result;
        }

        TreeNode? root = rootResult.Value;

        if (root is not null)
        {
            // this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Users}/{Constants.Trees.Users}/users";
            root.Icon = Constants.Icons.UserGroup;
            root.HasChildren = false;
        }

        return root;
    }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings) =>
        //full screen app without tree nodes
        TreeNodeCollection.Empty;

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings) =>
        //doesn't have a menu, this is a full screen app without tree nodes
        _menuItemCollectionFactory.Create();
}
