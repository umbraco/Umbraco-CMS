using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web.Models.Trees;
using umbraco;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.Trees
{
    public abstract class ContentTreeControllerBase : TreeController
    {
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
        protected abstract int UserStartNode { get; }

        protected abstract TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings);
        
        protected abstract MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings);

        protected abstract UmbracoObjectTypes UmbracoObjectType { get; }

        protected IEnumerable<IUmbracoEntity> GetChildEntities(string id)
        {
            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new InvalidCastException("The id for the media tree must be an integer");
            }

            //if a request is made for the root node data but the user's start node is not the default, then
            // we need to return their start node data
            if (iid == Constants.System.Root && UserStartNode != Constants.System.Root)
            {
                //just return their single start node, it will show up under the 'Content' label
                var startNode = Services.EntityService.Get(UserStartNode, UmbracoObjectType);
                return new[] {startNode};
            }

            return Services.EntityService.GetChildren(iid, UmbracoObjectType).ToArray();
        }

        /// <summary>
        /// This will automatically check if the recycle bin needs to be rendered (i.e. its the first level)
        /// and will automatically append it to the result of GetChildNodes.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected sealed override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            if (id == Constants.System.Root.ToInvariantString() && UserStartNode == Constants.System.Root)
            {
                //we need to append the recycle bin to the end (if not in dialog mode)
                var nodes = PerformGetTreeNodes(id, queryStrings);

                if (!IsDialog(queryStrings))
                {
                    nodes.Add(CreateTreeNode(
                        Constants.System.RecycleBinContent.ToInvariantString(),
                        queryStrings,
                        ui.GetText("general", "recycleBin"),
                        "icon-trash",
                        RecycleBinSmells,
                        queryStrings.GetValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/recyclebin"));    
                }

                return nodes;
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
                var menu = new MenuItemCollection();
                menu.Items.Add<ActionEmptyTranscan>(ui.Text("actions", "emptyTrashcan"));
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
            }
        }

        internal IEnumerable<MenuItem> GetAllowedUserMenuItemsForNode(IUmbracoEntity dd)
        {
            var actions = global::umbraco.BusinessLogic.Actions.Action.FromString(UmbracoUser.GetPermissions(dd.Path));

            // A user is allowed to delete their own stuff
            if (dd.CreatorId == UmbracoUser.Id && actions.Contains(ActionDelete.Instance) == false)
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
    }
}