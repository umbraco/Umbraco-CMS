using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.Trees
{
    public abstract class ContentTreeControllerBase : TreeApiController
    {
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