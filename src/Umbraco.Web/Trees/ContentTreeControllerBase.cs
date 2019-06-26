using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using System.Globalization;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Services;

namespace Umbraco.Web.Trees
{
    public abstract class ContentTreeControllerBase : TreeController
    {

        #region Actions

        /// <summary>
        /// Gets an individual tree node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public TreeNode GetTreeNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormDataCollection queryStrings)
        {
            int asInt;
            Guid asGuid = Guid.Empty;
            if (int.TryParse(id, out asInt) == false)
            {
                if (Guid.TryParse(id, out asGuid) == false)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }
            }

            var entity = asGuid == Guid.Empty
                    ? Services.EntityService.Get(asInt, UmbracoObjectType)
                    : Services.EntityService.GetByKey(asGuid, UmbracoObjectType);
            if (entity == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            var node = GetSingleTreeNode(entity, entity.ParentId.ToInvariantString(), queryStrings);

            //add the tree alias to the node since it is standalone (has no root for which this normally belongs)
            node.AdditionalData["treeAlias"] = TreeAlias;
            return node;
        }

        #endregion

        /// <summary>
        /// Ensure the noAccess metadata is applied for the root node if in dialog mode and the user doesn't have path access to it
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var node = base.CreateRootNode(queryStrings);

            if (IsDialog(queryStrings) && UserStartNodes.Contains(Constants.System.Root) == false && IgnoreUserStartNodes(queryStrings) == false)
            {
                node.AdditionalData["noAccess"] = true;
            }

            return node;
        }

        protected abstract TreeNode GetSingleTreeNode(IUmbracoEntity e, string parentId, FormDataCollection queryStrings);

        /// <summary>
        /// Returns a <see cref="TreeNode"/> for the <see cref="IUmbracoEntity"/> and
        /// attaches some meta data to the node if the user doesn't have start node access to it when in dialog mode
        /// </summary>
        /// <param name="e"></param>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        internal TreeNode GetSingleTreeNodeWithAccessCheck(IUmbracoEntity e, string parentId, FormDataCollection queryStrings,
            int[] startNodeIds, string[] startNodePaths, bool ignoreUserStartNodes)
        {
            bool hasPathAccess;
            var entityIsAncestorOfStartNodes = UserExtensions.IsInBranchOfStartNode(e.Path, startNodeIds, startNodePaths, out hasPathAccess);
            if (ignoreUserStartNodes == false && entityIsAncestorOfStartNodes == false)
                return null;

            var treeNode = GetSingleTreeNode(e, parentId, queryStrings);
            if (treeNode == null)
            {
                //this means that the user has NO access to this node via permissions! They at least need to have browse permissions to see
                //the node so we need to return null;
                return null;
            }
            if (ignoreUserStartNodes == false && hasPathAccess == false)
            {
                treeNode.AdditionalData["noAccess"] = true;
            }
            return treeNode;
        }

        private void GetUserStartNodes(out int[] startNodeIds, out string[] startNodePaths)
        {
            switch (RecycleBinId)
            {
                case Constants.System.RecycleBinMedia:
                    startNodeIds = Security.CurrentUser.CalculateMediaStartNodeIds(Services.EntityService);
                    startNodePaths = Security.CurrentUser.GetMediaStartNodePaths(Services.EntityService);
                    break;
                case Constants.System.RecycleBinContent:
                    startNodeIds = Security.CurrentUser.CalculateContentStartNodeIds(Services.EntityService);
                    startNodePaths = Security.CurrentUser.GetContentStartNodePaths(Services.EntityService);
                    break;
                default:
                    throw new NotSupportedException("Path access is only determined on content or media");
            }
        }

        /// <summary>
        /// Returns the
        /// </summary>
        protected abstract int RecycleBinId { get; }

        /// <summary>
        /// Returns true if the recycle bin has items in it
        /// </summary>
        protected abstract bool RecycleBinSmells { get; }

        /// <summary>
        /// Returns the user's start node for this tree
        /// </summary>
        protected abstract int[] UserStartNodes { get; }

        protected virtual TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            var rootIdString = Constants.System.Root.ToString(CultureInfo.InvariantCulture);
            var hasAccessToRoot = UserStartNodes.Contains(Constants.System.Root);

            var startNodeId = queryStrings.HasKey(TreeQueryStringParameters.StartNodeId)
                ? queryStrings.GetValue<string>(TreeQueryStringParameters.StartNodeId)
                : string.Empty;

            var ignoreUserStartNodes = IgnoreUserStartNodes(queryStrings);

            if (string.IsNullOrEmpty(startNodeId) == false && startNodeId != "undefined" && startNodeId != rootIdString)
            {
                // request has been made to render from a specific, non-root, start node
                id = startNodeId;

                // ensure that the user has access to that node, otherwise return the empty tree nodes collection
                // TODO: in the future we could return a validation statement so we can have some UI to notify the user they don't have access
                if (ignoreUserStartNodes == false && HasPathAccess(id, queryStrings) == false)
                {
                    LogHelper.Warn<ContentTreeControllerBase>("User " + Security.CurrentUser.Username + " does not have access to node with id " + id);
                    return nodes;
                }

                // if the tree is rendered...
                // - in a dialog: render only the children of the specific start node, nothing to do
                // - in a section: if the current user's start nodes do not contain the root node, we need
                //   to include these start nodes in the tree too, to provide some context - i.e. change
                //   start node back to root node, and then GetChildEntities method will take care of the rest.
                if (IsDialog(queryStrings) == false && hasAccessToRoot == false)
                    id = rootIdString;
            }

            // get child entities - if id is root, but user's start nodes do not contain the
            // root node, this returns the start nodes instead of root's children
            var entities = GetChildEntities(id, queryStrings).ToList();

            //get the current user start node/paths
            GetUserStartNodes(out var userStartNodes, out var userStartNodePaths);

            nodes.AddRange(entities.Select(x => GetSingleTreeNodeWithAccessCheck(x, id, queryStrings, userStartNodes, userStartNodePaths, ignoreUserStartNodes)).Where(x => x != null));

            // if the user does not have access to the root node, what we have is the start nodes,
            // but to provide some context we also need to add their topmost nodes when they are not
            // topmost nodes themselves (level > 1).
            if (id == rootIdString && hasAccessToRoot == false)
            {
                var topNodeIds = entities.Where(x => x.Level > 1).Select(GetTopNodeId).Where(x => x != 0).Distinct().ToArray();
                if (topNodeIds.Length > 0)
                {
                    var topNodes = Services.EntityService.GetAll(UmbracoObjectType, topNodeIds.ToArray());
                    nodes.AddRange(topNodes.Select(x => GetSingleTreeNodeWithAccessCheck(x, id, queryStrings, userStartNodes, userStartNodePaths, ignoreUserStartNodes)).Where(x => x != null));
                }
            }

            return nodes;
        }

        private static readonly char[] Comma = { ',' };

        private int GetTopNodeId(IUmbracoEntity entity)
        {
            int id;
            var parts = entity.Path.Split(Comma, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 && int.TryParse(parts[1], out id) ? id : 0;
        }

        protected abstract MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings);

        protected abstract UmbracoObjectTypes UmbracoObjectType { get; }

        protected IEnumerable<IUmbracoEntity> GetChildEntities(string id, FormDataCollection queryStrings)
        {
            // try to parse id as an integer else use GetEntityFromId
            // which will grok Guids, Udis, etc and let use obtain the id
            if (int.TryParse(id, out var entityId) == false)
            {
                var entity = GetEntityFromId(id);
                if (entity == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                entityId = entity.Id;
            }

            return GetChildrenFromEntityService(entityId);
        }

        private IEnumerable<IUmbracoEntity> GetChildrenFromEntityService(int entityId)
            => Services.EntityService.GetChildren(entityId, UmbracoObjectType).ToList();

        /// <summary>
        /// Returns true or false if the current user has access to the node based on the user's allowed start node (path) access
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        //we should remove this in v8, it's now here for backwards compat only
        protected abstract bool HasPathAccess(string id, FormDataCollection queryStrings);

        /// <summary>
        /// Returns true or false if the current user has access to the node based on the user's allowed start node (path) access
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected bool HasPathAccess(IUmbracoEntity entity, FormDataCollection queryStrings)
        {
            if (entity == null) return false;
            return Security.CurrentUser.HasPathAccess(entity, Services.EntityService, RecycleBinId);
        }

        /// <summary>
        /// Ensures the recycle bin is appended when required (i.e. user has access to the root and it's not in dialog mode)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is overwritten strictly to render the recycle bin, it should serve no other purpose
        /// </remarks>
        protected sealed override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            //check if we're rendering the root
            if (id == Constants.System.Root.ToInvariantString() && UserStartNodes.Contains(Constants.System.Root))
            {
                var altStartId = string.Empty;

                if (queryStrings.HasKey(TreeQueryStringParameters.StartNodeId))
                    altStartId = queryStrings.GetValue<string>(TreeQueryStringParameters.StartNodeId);

                //check if a request has been made to render from a specific start node
                if (string.IsNullOrEmpty(altStartId) == false && altStartId != "undefined" && altStartId != Constants.System.Root.ToString(CultureInfo.InvariantCulture))
                {
                    id = altStartId;
                }

                var nodes = GetTreeNodesInternal(id, queryStrings);

                //only render the recycle bin if we are not in dialog and the start id id still the root
                if (IsDialog(queryStrings) == false && id == Constants.System.Root.ToInvariantString())
                {
                    nodes.Add(CreateTreeNode(
                        RecycleBinId.ToInvariantString(),
                        id,
                        queryStrings,
                        ui.GetText("general", "recycleBin"),
                        "icon-trash",
                        RecycleBinSmells,
                        queryStrings.GetValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/recyclebin"));

                }

                return nodes;
            }

            return GetTreeNodesInternal(id, queryStrings);
        }

        /// <summary>
        /// Check to see if we should return children of a container node
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is required in case a user has custom start nodes that are children of a list view since in that case we'll need to render the tree node. In normal cases we don't render
        /// children of a list view.
        /// </remarks>
        protected bool ShouldRenderChildrenOfContainer(IUmbracoEntity e)
        {
            var isContainer = e.IsContainer();

            var renderChildren = e.HasChildren() && (isContainer == false);

            //Here we need to figure out if the node is a container and if so check if the user has a custom start node, then check if that start node is a child
            // of this container node. If that is true, the HasChildren must be true so that the tree node still renders even though this current node is a container/list view.
            if (isContainer && UserStartNodes.Length > 0 && UserStartNodes.Contains(Constants.System.Root) == false)
            {                    
                var startNodes = Services.EntityService.GetAll(UmbracoObjectType, UserStartNodes);
                //if any of these start nodes' parent is current, then we need to render children normally so we need to switch some logic and tell
                // the UI that this node does have children and that it isn't a container
                if (startNodes.Any(x => x.ParentId == e.Id))
                {
                    renderChildren = true;
                }
            }

            return renderChildren;
        }

        /// <summary>
        /// Before we make a call to get the tree nodes we have to check if they can actually be rendered
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        /// <remarks>
        /// Currently this just checks if it is a container type, if it is we cannot render children. In the future this might check for other things.
        /// </remarks>
        private TreeNodeCollection GetTreeNodesInternal(string id, FormDataCollection queryStrings)
        {
            var current = GetEntityFromId(id);

            //before we get the children we need to see if this is a container node

            //test if the parent is a listview / container
            if (current != null && ShouldRenderChildrenOfContainer(current) == false)
            {
                //no children!
                return new TreeNodeCollection();
            }

            return PerformGetTreeNodes(id, queryStrings);
        }

        /// <summary>
        /// Checks if the menu requested is for the recycle bin and renders that, otherwise renders the result of PerformGetMenuForNode
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected sealed override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            if (RecycleBinId.ToInvariantString() == id)
            {
                // get the default assigned permissions for this user
                var actions = ActionsResolver.Current.FromActionSymbols(Security.CurrentUser.GetPermissions(Constants.System.RecycleBinContentString, Services.UserService)).ToList();

                var menu = new MenuItemCollection();
                // only add empty recycle bin if the current user is allowed to delete by default 
                if (actions.Contains(ActionDelete.Instance))
                {
                    menu.Items.Add<ActionEmptyTranscan>(ui.Text("actions", "emptyTrashcan"));
                }
                menu.Items.Add<ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            return PerformGetMenuForNode(id, queryStrings);
        }

        /// <summary>
        /// Based on the allowed actions, this will filter the ones that the current user is allowed
        /// </summary>
        /// <param name="menuWithAllItems"></param>
        /// <param name="userAllowedMenuItems"></param>
        /// <returns></returns>
        protected void FilterUserAllowedMenuItems(MenuItemCollection menuWithAllItems, IEnumerable<MenuItem> userAllowedMenuItems)
        {
            var userAllowedActions = userAllowedMenuItems.Where(x => x.Action != null).Select(x => x.Action).ToArray();

            var notAllowed = menuWithAllItems.Items.Where(
                a => (a.Action != null
                      && a.Action.CanBePermissionAssigned
                      && (a.Action.CanBePermissionAssigned == false || userAllowedActions.Contains(a.Action) == false)))
                                             .ToArray();

            //remove the ones that aren't allowed.
            foreach (var m in notAllowed)
            {
                menuWithAllItems.Items.Remove(m);
                // if the disallowed action is set as default action, make sure to reset the default action as well
                if (menuWithAllItems.DefaultMenuAlias == m.Alias)
                {
                    menuWithAllItems.DefaultMenuAlias = null;
                }
            }
        }

        internal IEnumerable<MenuItem> GetAllowedUserMenuItemsForNode(IUmbracoEntity dd)
        {
            var actions = ActionsResolver.Current.FromActionSymbols(Security.CurrentUser.GetPermissions(dd.Path, Services.UserService))
                .ToList();

            // A user is allowed to delete their own stuff
            if (dd.CreatorId == Security.GetUserId() && actions.Contains(ActionDelete.Instance) == false)
                actions.Add(ActionDelete.Instance);

            return actions.Select(x => new MenuItem(x));
        }

        /// <summary>
        /// Determins if the user has access to view the node/document
        /// </summary>
        /// <param name="doc">The Document to check permissions against</param>
        /// <param name="allowedUserOptions">A list of MenuItems that the user has permissions to execute on the current document</param>
        /// <remarks>By default the user must have Browse permissions to see the node in the Content tree</remarks>
        /// <returns></returns>
        internal bool CanUserAccessNode(IUmbracoEntity doc, IEnumerable<MenuItem> allowedUserOptions)
        {
            return allowedUserOptions.Select(x => x.Action).OfType<ActionBrowse>().Any();
        }

        /// <summary>
        /// this will parse the string into either a GUID or INT
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Tuple<Guid?, int?> GetIdentifierFromString(string id)
        {
            Guid idGuid;
            int idInt;
            Udi idUdi;

            if (Guid.TryParse(id, out idGuid))
            {
                return new Tuple<Guid?, int?>(idGuid, null);
            }
            if (int.TryParse(id, out idInt))
            {
                return new Tuple<Guid?, int?>(null, idInt);
            }
            if (Udi.TryParse(id, out idUdi))
            {
                var guidUdi = idUdi as GuidUdi;
                if (guidUdi != null)
                    return new Tuple<Guid?, int?>(guidUdi.Guid, null);
            }

            return null;
        }

        /// <summary>
        /// Get an entity via an id that can be either an integer, Guid or UDI
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// This object has it's own contextual cache for these lookups
        /// </remarks>
        internal IUmbracoEntity GetEntityFromId(string id)
        {
            return _entityCache.GetOrAdd(id, s =>
            {
                IUmbracoEntity entity;

                Guid idGuid;
                int idInt;
                Udi idUdi;

                if (Guid.TryParse(s, out idGuid))
                {
                    entity = Services.EntityService.GetByKey(idGuid, UmbracoObjectType);
                }
                else if (int.TryParse(s, out idInt))
                {
                    entity = Services.EntityService.Get(idInt, UmbracoObjectType);
                }
                else if (Udi.TryParse(s, out idUdi))
                {
                    var guidUdi = idUdi as GuidUdi;
                    entity = guidUdi != null ? Services.EntityService.GetByKey(guidUdi.Guid, UmbracoObjectType) : null;
                }
                else
                {
                    return null;
                }

                return entity;
            });


        }

        private readonly ConcurrentDictionary<string, IUmbracoEntity> _entityCache = new ConcurrentDictionary<string, IUmbracoEntity>();

        /// <summary>
        /// If the request should allows a user to choose nodes that they normally don't have access to
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        internal bool IgnoreUserStartNodes(FormDataCollection queryStrings)
        {
            var dataTypeId = queryStrings.GetValue<Guid?>(TreeQueryStringParameters.DataTypeId);
            if (dataTypeId.HasValue)
            {
                return Services.DataTypeService.IsDataTypeIgnoringUserStartNodes(dataTypeId.Value);
            }

            return false;
        }
    }
}
