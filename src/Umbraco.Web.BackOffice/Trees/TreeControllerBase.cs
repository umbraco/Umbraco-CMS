using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Trees;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.ModelBinders;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Trees
{
    /// <summary>
    /// A base controller reference for non-attributed trees (un-registered).
    /// </summary>
    /// <remarks>
    /// Developers should generally inherit from TreeController.
    /// </remarks>
    [AngularJsonOnlyConfiguration]
    public abstract class TreeControllerBase : UmbracoAuthorizedApiController, ITree
    {
        // TODO: Need to set this, but from where?
        //       Presumably not injecting as this will be a base controller for package/solution developers.
        private readonly UmbracoApiControllerTypeCollection _apiControllers;

        protected TreeControllerBase(UmbracoApiControllerTypeCollection apiControllers)
        {
            _apiControllers = apiControllers;
        }

        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be passed in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected abstract TreeNodeCollection GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormCollection queryStrings);

        /// <summary>
        /// Returns the menu structure for the node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected abstract MenuItemCollection GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormCollection queryStrings);

        /// <summary>
        /// The name to display on the root node
        /// </summary>
        public abstract string RootNodeDisplayName { get; }

        /// <inheritdoc />
        public abstract string TreeGroup { get; }

        /// <inheritdoc />
        public abstract string TreeAlias { get; }

        /// <inheritdoc />
        public abstract string TreeTitle { get; }

        /// <inheritdoc />
        public abstract TreeUse TreeUse { get; }

        /// <inheritdoc />
        public abstract string SectionAlias { get; }

        /// <inheritdoc />
        public abstract int SortOrder { get; }

        /// <inheritdoc />
        public abstract bool IsSingleNodeTree { get; }

        /// <summary>
        /// Returns the root node for the tree
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public TreeNode GetRootNode([ModelBinder(typeof(HttpQueryStringModelBinder))]FormCollection queryStrings)
        {
            if (queryStrings == null) queryStrings = FormCollection.Empty;
            var node = CreateRootNode(queryStrings);

            //add the tree alias to the root
            node.AdditionalData["treeAlias"] = TreeAlias;

            AddQueryStringsToAdditionalData(node, queryStrings);

            //check if the tree is searchable and add that to the meta data as well
            if (this is ISearchableTree)
                node.AdditionalData.Add("searchable", "true");

            //now update all data based on some of the query strings, like if we are running in dialog mode
            if (IsDialog(queryStrings))
                node.RoutePath = "#";

            OnRootNodeRendering(this, new TreeNodeRenderingEventArgs(node, queryStrings));

            return node;
        }

        /// <summary>
        /// The action called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <returns>JSON markup for jsTree</returns>
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be passed in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        public TreeNodeCollection GetNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormCollection queryStrings)
        {
            if (queryStrings == null) queryStrings = FormCollection.Empty;
            var nodes = GetTreeNodes(id, queryStrings);

            foreach (var node in nodes)
                AddQueryStringsToAdditionalData(node, queryStrings);

            //now update all data based on some of the query strings, like if we are running in dialog mode
            if (IsDialog(queryStrings))
                foreach (var node in nodes)
                    node.RoutePath = "#";

            //raise the event
            OnTreeNodesRendering(this, new TreeNodesRenderingEventArgs(nodes, queryStrings));

            return nodes;
        }

        /// <summary>
        /// The action called to render the menu for a tree node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public MenuItemCollection GetMenu(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormCollection queryStrings)
        {
            if (queryStrings == null) queryStrings = FormCollection.Empty;
            var menu = GetMenuForNode(id, queryStrings);
            //raise the event
            OnMenuRendering(this, new MenuRenderingEventArgs(id, menu, queryStrings));
            return menu;
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected virtual TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var rootNodeAsString = Constants.System.RootString;
            queryStrings.TryGetValue(TreeQueryStringParameters.Application, out var currApp);

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

        #region Create TreeNode methods

        /// <summary>
        /// Helper method to create tree nodes
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
        /// Helper method to create tree nodes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string title, string icon)
        {
            var jsonUrl = Url.GetTreeUrl(_apiControllers, GetType(), id, queryStrings);
            var menuUrl = Url.GetMenuUrl(_apiControllers, GetType(), id, queryStrings);
            var node = new TreeNode(id, parentId, jsonUrl, menuUrl)
            {
                Name = title,
                Icon = icon,
                NodeType = TreeAlias
            };
            return node;
        }

        /// <summary>
        /// Helper method to create tree nodes
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
        /// Helper method to create tree nodes and automatically generate the json URL + UDI
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityObjectType"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <param name="hasChildren"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(IEntitySlim entity, Guid entityObjectType, string parentId, FormCollection queryStrings, bool hasChildren)
        {
            var contentTypeIcon = entity is IContentEntitySlim contentEntity ? contentEntity.ContentTypeIcon : null;
            var treeNode = CreateTreeNode(entity.Id.ToInvariantString(), parentId, queryStrings, entity.Name, contentTypeIcon);
            treeNode.Path = entity.Path;
            treeNode.Udi = Udi.Create(ObjectTypes.GetUdiType(entityObjectType), entity.Key);
            treeNode.HasChildren = hasChildren;
            treeNode.Trashed = entity.Trashed;
            return treeNode;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json URL + UDI
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
            var treeNode = CreateTreeNode(entity.Id.ToInvariantString(), parentId, queryStrings, entity.Name, icon);
            treeNode.Udi = Udi.Create(ObjectTypes.GetUdiType(entityObjectType), entity.Key);
            treeNode.Path = entity.Path;
            treeNode.HasChildren = hasChildren;
            return treeNode;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json URL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="icon"></param>
        /// <param name="hasChildren"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string title, string icon, bool hasChildren)
        {
            var treeNode = CreateTreeNode(id, parentId, queryStrings, title, icon);
            treeNode.HasChildren = hasChildren;
            return treeNode;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json URL
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="routePath"></param>
        /// <param name="hasChildren"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string title, string icon, bool hasChildren, string routePath)
        {
            var treeNode = CreateTreeNode(id, parentId, queryStrings, title, icon);
            treeNode.HasChildren = hasChildren;
            treeNode.RoutePath = routePath;
            return treeNode;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json URL + UDI
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
        public TreeNode CreateTreeNode(string id, string parentId, FormCollection queryStrings, string title, string icon, bool hasChildren, string routePath, Udi udi)
        {
            var treeNode = CreateTreeNode(id, parentId, queryStrings, title, icon);
            treeNode.HasChildren = hasChildren;
            treeNode.RoutePath = routePath;
            treeNode.Udi = udi;
            return treeNode;
        }

        #endregion

        /// <summary>
        /// The AdditionalData of a node is always populated with the query string data, this method performs this
        /// operation and ensures that special values are not inserted or that duplicate keys are not added.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="queryStrings"></param>
        protected void AddQueryStringsToAdditionalData(TreeNode node, FormCollection queryStrings)
        {
            foreach (var q in queryStrings.Where(x => node.AdditionalData.ContainsKey(x.Key) == false))
                node.AdditionalData.Add(q.Key, q.Value);
        }

        /// <summary>
        /// If the request is for a dialog mode tree
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected bool IsDialog(FormCollection queryStrings)
        {
            queryStrings.TryGetValue(TreeQueryStringParameters.Use, out var use);
            return use == "dialog";
        }

        /// <summary>
        /// An event that allows developers to modify the tree node collection that is being rendered
        /// </summary>
        /// <remarks>
        /// Developers can add/remove/replace/insert/update/etc... any of the tree items in the collection.
        /// </remarks>
        public static event TypedEventHandler<TreeControllerBase, TreeNodesRenderingEventArgs> TreeNodesRendering;

        private static void OnTreeNodesRendering(TreeControllerBase instance, TreeNodesRenderingEventArgs e)
        {
            var handler = TreeNodesRendering;
            handler?.Invoke(instance, e);
        }

        /// <summary>
        /// An event that allows developer to modify the root tree node that is being rendered
        /// </summary>
        public static event TypedEventHandler<TreeControllerBase, TreeNodeRenderingEventArgs> RootNodeRendering;

        // internal for temp class below - kill eventually!
        internal static void OnRootNodeRendering(TreeControllerBase instance, TreeNodeRenderingEventArgs e)
        {
            var handler = RootNodeRendering;
            handler?.Invoke(instance, e);
        }

        /// <summary>
        /// An event that allows developers to modify the menu that is being rendered
        /// </summary>
        /// <remarks>
        /// Developers can add/remove/replace/insert/update/etc... any of the tree items in the collection.
        /// </remarks>
        public static event TypedEventHandler<TreeControllerBase, MenuRenderingEventArgs> MenuRendering;

        private static void OnMenuRendering(TreeControllerBase instance, MenuRenderingEventArgs e)
        {
            var handler = MenuRendering;
            handler?.Invoke(instance, e);
        }
    }
}
