using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using umbraco;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.Trees
{
    public abstract class ContentTreeControllerBase : TreeApiController
    {
        /// <summary>
        /// Returns the 
        /// </summary>
        protected abstract int RecycleBinId { get; }

        /// <summary>
        /// Returns true if the recycle bin has items in it
        /// </summary>
        protected abstract bool RecycleBinSmells { get; }

        protected abstract TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings);
        
        protected abstract MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings);

        /// <summary>
        /// This will automatically check if the recycle bin needs to be rendered (i.e. its the first level)
        /// and will automatically append it to the result of GetChildNodes.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected sealed override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            if (id == Constants.System.Root.ToInvariantString())
            {
                //we need to append the recycle bin to the end 
                var nodes = PerformGetTreeNodes(id, queryStrings);
                nodes.Add(CreateTreeNode(
                    Constants.System.RecycleBinContent.ToInvariantString(),
                    queryStrings,
                    ui.GetText("general", "recycleBin"),
                    "icon-trash",
                    RecycleBinSmells));
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
                menu.AddMenuItem<ActionEmptyTranscan>();
                menu.AddMenuItem<ActionRefresh>(true);
                return menu;
            }
            return PerformGetMenuForNode(id, queryStrings);
        }

        /// <summary>
        /// Based on the allowed actions, this will filter the ones that the current user is allowed
        /// </summary>
        /// <param name="allMenuItems"></param>
        /// <param name="userAllowedMenuItems"></param>
        /// <returns></returns>
        protected MenuItemCollection GetUserAllowedMenuItems(IEnumerable<MenuItem> allMenuItems, IEnumerable<MenuItem> userAllowedMenuItems)
        {
            var userAllowedActions = userAllowedMenuItems.Where(x => x.Action != null).Select(x => x.Action).ToArray();
            return new MenuItemCollection(allMenuItems.Where(
                a => (a.Action == null
                      || a.Action.CanBePermissionAssigned == false
                      || (a.Action.CanBePermissionAssigned && userAllowedActions.Contains(a.Action)))));
        }

        internal MenuItemCollection GetUserMenuItemsForNode(IUmbracoEntity dd)
        {
            var actions = global::umbraco.BusinessLogic.Actions.Action.FromString(UmbracoUser.GetPermissions(dd.Path));

            // A user is allowed to delete their own stuff
            if (dd.CreatorId == UmbracoUser.Id && actions.Contains(ActionDelete.Instance) == false)
                actions.Add(ActionDelete.Instance);

            return new MenuItemCollection(actions.Select(x => new MenuItem(x)));
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