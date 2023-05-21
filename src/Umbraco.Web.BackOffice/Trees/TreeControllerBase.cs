using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Trees;

/// <summary>
///     A base controller reference for non-attributed trees (un-registered).
/// </summary>
/// <remarks>
///     Developers should generally inherit from TreeController.
/// </remarks>
[AngularJsonOnlyConfiguration]
public abstract class TreeControllerBase : UmbracoAuthorizedApiController, ITree
{
    // TODO: Need to set this, but from where?
    //       Presumably not injecting as this will be a base controller for package/solution developers.
    private readonly UmbracoApiControllerTypeCollection _apiControllers;
    private readonly IEventAggregator _eventAggregator;

    protected TreeControllerBase(UmbracoApiControllerTypeCollection apiControllers, IEventAggregator eventAggregator)
    {
        _apiControllers = apiControllers;
        _eventAggregator = eventAggregator;
    }

    /// <summary>
    ///     The name to display on the root node
    /// </summary>
    public abstract string? RootNodeDisplayName { get; }

    /// <inheritdoc />
    public abstract string? TreeGroup { get; }

    /// <inheritdoc />
    public abstract string TreeAlias { get; }

    /// <inheritdoc />
    public abstract string? TreeTitle { get; }

    /// <inheritdoc />
    public abstract TreeUse TreeUse { get; }

    /// <inheritdoc />
    public abstract string SectionAlias { get; }

    /// <inheritdoc />
    public abstract int SortOrder { get; }

    /// <inheritdoc />
    public abstract bool IsSingleNodeTree { get; }

    /// <summary>
    ///     The method called to render the contents of the tree structure
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings">
    ///     All of the query string parameters passed from jsTree
    /// </param>
    /// <remarks>
    ///     We are allowing an arbitrary number of query strings to be passed in so that developers are able to persist custom
    ///     data from the front-end
    ///     to the back end to be used in the query for model data.
    /// </remarks>
    protected abstract ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings);

    /// <summary>
    ///     Returns the menu structure for the node
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    protected abstract ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings);

    /// <summary>
    ///     The method called to render the contents of the tree structure
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings">
    ///     All of the query string parameters passed from jsTree
    /// </param>
    /// <remarks>
    ///     If overriden, GetTreeNodes will not be called
    ///     We are allowing an arbitrary number of query strings to be passed in so that developers are able to persist custom
    ///     data from the front-end
    ///     to the back end to be used in the query for model data.
    /// </remarks>
    protected virtual async Task<ActionResult<TreeNodeCollection>> GetTreeNodesAsync(
        string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings) =>
        GetTreeNodes(id, queryStrings);

    /// <summary>
    ///     Returns the menu structure for the node
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    /// <remarks>
    ///     If overriden, GetMenuForNode will not be called
    /// </remarks>
    protected virtual async Task<ActionResult<MenuItemCollection>> GetMenuForNodeAsync(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings) =>
        GetMenuForNode(id, queryStrings);

    /// <summary>
    ///     Returns the root node for the tree
    /// </summary>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    public async Task<ActionResult<TreeNode?>> GetRootNode(
        [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection? queryStrings)
    {
        if (queryStrings == null)
        {
            queryStrings = FormCollection.Empty;
        }

        ActionResult<TreeNode?> nodeResult = CreateRootNode(queryStrings);
        if (!(nodeResult.Result is null))
        {
            return nodeResult.Result;
        }

        TreeNode? node = nodeResult.Value;

        if (node is not null)
        {
            // Add the tree alias to the root
            node.AdditionalData["treeAlias"] = TreeAlias;
            AddQueryStringsToAdditionalData(node, queryStrings);

            // Check if the tree is searchable and add that to the meta data as well
            if (this is ISearchableTree)
            {
                node.AdditionalData.Add("searchable", "true");
            }

            // Now update all data based on some of the query strings, like if we are running in dialog mode
            if (IsDialog(queryStrings))
            {
                node.RoutePath = "#";
            }

            await _eventAggregator.PublishAsync(new RootNodeRenderingNotification(node, queryStrings, TreeAlias));
        }

        return node;
    }

    /// <summary>
    ///     The action called to render the contents of the tree structure
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings">
    ///     All of the query string parameters passed from jsTree
    /// </param>
    /// <returns>JSON markup for jsTree</returns>
    /// <remarks>
    ///     We are allowing an arbitrary number of query strings to be passed in so that developers are able to persist custom
    ///     data from the front-end
    ///     to the back end to be used in the query for model data.
    /// </remarks>
    public async Task<ActionResult<TreeNodeCollection?>> GetNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection? queryStrings)
    {
        if (queryStrings == null)
        {
            queryStrings = FormCollection.Empty;
        }

        ActionResult<TreeNodeCollection> nodesResult = await GetTreeNodesAsync(id, queryStrings);

        if (!(nodesResult.Result is null))
        {
            return nodesResult.Result;
        }

        TreeNodeCollection? nodes = nodesResult.Value;

        if (nodes is not null)
        {
            foreach (TreeNode node in nodes)
            {
                AddQueryStringsToAdditionalData(node, queryStrings);
            }

            // Now update all data based on some of the query strings, like if we are running in dialog mode
            if (IsDialog(queryStrings))
            {
                foreach (TreeNode node in nodes)
                {
                    node.RoutePath = "#";
                }
            }

            // Raise the event
            await _eventAggregator.PublishAsync(new TreeNodesRenderingNotification(nodes, queryStrings, TreeAlias, id));
        }

        return nodes;
    }

    /// <summary>
    ///     The action called to render the menu for a tree node
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    public async Task<ActionResult<MenuItemCollection?>> GetMenu(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
    {
        if (queryStrings == null)
        {
            queryStrings = FormCollection.Empty;
        }

        ActionResult<MenuItemCollection>? menuResult = await GetMenuForNodeAsync(id, queryStrings);
        if (!(menuResult?.Result is null))
        {
            return menuResult.Result;
        }

        MenuItemCollection? menu = menuResult?.Value;

        if (menu is not null)
        {
            //raise the event
            await _eventAggregator.PublishAsync(new MenuRenderingNotification(id, menu, queryStrings, TreeAlias));
        }

        return menu;
    }

    /// <summary>
    ///     Helper method to create a root model for a tree
    /// </summary>
    /// <returns></returns>
    protected virtual ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        var rootNodeAsString = Constants.System.RootString;
        queryStrings.TryGetValue(TreeQueryStringParameters.Application, out StringValues currApp);

        var node = new TreeNode(
            rootNodeAsString,
            null, //this is a root node, there is no parent
            Url.GetTreeUrl(_apiControllers, GetType(), rootNodeAsString, queryStrings),
            Url.GetMenuUrl(_apiControllers, GetType(), rootNodeAsString, queryStrings))
        {
            HasChildren = true,
            RoutePath = currApp,
            Name = RootNodeDisplayName
        };

        return node;
    }

    /// <summary>
    ///     The AdditionalData of a node is always populated with the query string data, this method performs this
    ///     operation and ensures that special values are not inserted or that duplicate keys are not added.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="queryStrings"></param>
    protected void AddQueryStringsToAdditionalData(TreeNode node, FormCollection queryStrings)
    {
        foreach (KeyValuePair<string, StringValues> q in queryStrings.Where(x =>
                     node.AdditionalData.ContainsKey(x.Key) == false))
        {
            node.AdditionalData.Add(q.Key, q.Value);
        }
    }

    /// <summary>
    ///     If the request is for a dialog mode tree
    /// </summary>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    protected bool IsDialog(FormCollection queryStrings)
    {
        queryStrings.TryGetValue(TreeQueryStringParameters.Use, out StringValues use);
        return use == "dialog";
    }

    #region Create TreeNode methods

    /// <summary>
    ///     Helper method to create tree nodes
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string title)
    {
        var jsonUrl = Url.GetTreeUrl(_apiControllers, GetType(), id, queryStrings);
        var menuUrl = Url.GetMenuUrl(_apiControllers, GetType(), id, queryStrings);
        var node = new TreeNode(id, parentId, jsonUrl, menuUrl) { Name = title };
        return node;
    }

    /// <summary>
    ///     Helper method to create tree nodes
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="title"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(string id, string parentId, FormCollection? queryStrings, string? title, string? icon)
    {
        var jsonUrl = Url.GetTreeUrl(_apiControllers, GetType(), id, queryStrings);
        var menuUrl = Url.GetMenuUrl(_apiControllers, GetType(), id, queryStrings);
        var node = new TreeNode(id, parentId, jsonUrl, menuUrl) { Name = title, Icon = icon, NodeType = TreeAlias };
        return node;
    }

    /// <summary>
    ///     Helper method to create tree nodes
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="title"></param>
    /// <param name="routePath"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string title, string icon, string routePath)
    {
        var jsonUrl = Url.GetTreeUrl(_apiControllers, GetType(), id, queryStrings);
        var menuUrl = Url.GetMenuUrl(_apiControllers, GetType(), id, queryStrings);
        var node = new TreeNode(id, parentId, jsonUrl, menuUrl) { Name = title, RoutePath = routePath, Icon = icon };
        return node;
    }

    /// <summary>
    ///     Helper method to create tree nodes and automatically generate the json URL + UDI
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="entityObjectType"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="hasChildren"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(IEntitySlim entity, Guid entityObjectType, string parentId, FormCollection? queryStrings, bool hasChildren)
    {
        var contentTypeIcon = entity is IContentEntitySlim contentEntity ? contentEntity.ContentTypeIcon : null;
        TreeNode treeNode = CreateTreeNode(entity.Id.ToInvariantString(), parentId, queryStrings, entity.Name, contentTypeIcon);
        treeNode.Path = entity.Path;
        treeNode.Udi = Udi.Create(ObjectTypes.GetUdiType(entityObjectType), entity.Key);
        treeNode.HasChildren = hasChildren;
        treeNode.Trashed = entity.Trashed;
        return treeNode;
    }

    /// <summary>
    ///     Helper method to create tree nodes and automatically generate the json URL + UDI
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="entityObjectType"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="icon"></param>
    /// <param name="hasChildren"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(IUmbracoEntity entity, Guid entityObjectType, string parentId, FormCollection queryStrings, string icon, bool hasChildren)
    {
        TreeNode treeNode = CreateTreeNode(entity.Id.ToInvariantString(), parentId, queryStrings, entity.Name, icon);
        treeNode.Udi = Udi.Create(ObjectTypes.GetUdiType(entityObjectType), entity.Key);
        treeNode.Path = entity.Path;
        treeNode.HasChildren = hasChildren;
        return treeNode;
    }

    /// <summary>
    ///     Helper method to create tree nodes and automatically generate the json URL
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="title"></param>
    /// <param name="icon"></param>
    /// <param name="hasChildren"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string? title, string? icon, bool hasChildren)
    {
        TreeNode treeNode = CreateTreeNode(id, parentId, queryStrings, title, icon);
        treeNode.HasChildren = hasChildren;
        return treeNode;
    }

    /// <summary>
    ///     Helper method to create tree nodes and automatically generate the json URL
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="title"></param>
    /// <param name="routePath"></param>
    /// <param name="hasChildren"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string? title, string? icon, bool hasChildren, string routePath)
    {
        TreeNode treeNode = CreateTreeNode(id, parentId, queryStrings, title, icon);
        treeNode.HasChildren = hasChildren;
        treeNode.RoutePath = routePath;
        return treeNode;
    }

    /// <summary>
    ///     Helper method to create tree nodes and automatically generate the json URL + UDI
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentId"></param>
    /// <param name="queryStrings"></param>
    /// <param name="title"></param>
    /// <param name="routePath"></param>
    /// <param name="hasChildren"></param>
    /// <param name="icon"></param>
    /// <param name="udi"></param>
    /// <returns></returns>
    public TreeNode CreateTreeNode(string id, string parentId, FormCollection? queryStrings, string? title, string icon, bool hasChildren, string? routePath, Udi udi)
    {
        TreeNode treeNode = CreateTreeNode(id, parentId, queryStrings, title, icon);
        treeNode.HasChildren = hasChildren;
        treeNode.RoutePath = routePath;
        treeNode.Udi = udi;
        return treeNode;
    }

    #endregion
}
