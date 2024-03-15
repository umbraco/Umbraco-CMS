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

[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
[Tree(Constants.Applications.Settings, Constants.Trees.Webhooks, SortOrder = 9, TreeGroup = Constants.Trees.Groups.Settings)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public class WebhooksTreeController : TreeController
{
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

    public WebhooksTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator,
        IMenuItemCollectionFactory menuItemCollectionFactory)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator) =>
        _menuItemCollectionFactory = menuItemCollectionFactory;

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings) =>
        //We don't have any child nodes & only use the root node to load a custom UI
        new TreeNodeCollection();

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings) =>
        //We don't have any menu item options (such as create/delete/reload) & only use the root node to load a custom UI
        _menuItemCollectionFactory.Create();

    /// <summary>
    ///     Helper method to create a root model for a tree
    /// </summary>
    /// <returns></returns>
    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        ActionResult<TreeNode?> rootResult = base.CreateRootNode(queryStrings);
        if (!(rootResult.Result is null))
        {
            return rootResult;
        }

        TreeNode? root = rootResult.Value;

        if (root is not null)
        {
            // This will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = $"{Constants.Applications.Settings}/{Constants.Trees.Webhooks}/overview";
            root.Icon = Constants.Icons.Webhook;
            root.HasChildren = false;
            root.MenuUrl = null;
        }

        return root;
    }
}
