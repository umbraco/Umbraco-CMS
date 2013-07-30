using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
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

        internal MenuItemCollection GetUserMenuItemsForNode(UmbracoEntity dd)
        {
            var actions = global::umbraco.BusinessLogic.Actions.Action.FromString(UmbracoUser.GetPermissions(dd.Path));

            // A user is allowed to delete their own stuff
            if (dd.CreatorId == UmbracoUser.Id && actions.Contains(ActionDelete.Instance) == false)
                actions.Add(ActionDelete.Instance);

            return new MenuItemCollection(actions.Select(x => new MenuItem(x)));
        }

    }
}