using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web._Legacy.Actions;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Users)]
    [Tree(Constants.Applications.Users, Constants.Trees.Users, null)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class UsersTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            var users = new List<IUser>(Services.UserService.GetAll(0, int.MaxValue, out _));
            var currentUser = UmbracoContext.Current.Security.CurrentUser;
            var hideDisabledUsers = UmbracoConfig.For.UmbracoSettings().Security.HideDisabledUsersInBackoffice;

            foreach (var user in users.OrderBy(x => x.IsApproved == false))
            {
                // hide disabled user
                if (user.IsApproved == false && hideDisabledUsers)
                    continue;

                if (user.IsSuper())
                {
                    // only super can see super
                    if (!currentUser.IsSuper()) continue;
                }
                else if (user.IsAdmin())
                {
                    // only admins can see admins
                    if (!currentUser.IsAdmin()) continue;
                }

                var node = CreateTreeNode(
                    user.Id.ToString(CultureInfo.InvariantCulture),
                    "-1",
                    queryStrings,
                    user.Name,
                    "icon-user",
                    false,
                    "/" + queryStrings.GetValue<string>("application") + "/framed/"
                    + Uri.EscapeDataString("users/EditUser.aspx?id=" + user.Id));

                if (user.IsApproved == false)
                    node.CssClasses.Add("not-published");

                nodes.Add(node);
            }

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // Root actions
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    .ConvertLegacyMenuItem(null, "users", queryStrings.GetValue<string>("application"));

                menu.Items.Add<RefreshNode, ActionRefresh>(
                    Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            // If administator, don't create a menu
            if (id == "0")
                return menu;

            menu.Items.Add(new DisableUser()
            {
                Name = Services.TextService.Localize("actions", "disable")
            });

            return menu;
        }
    }
}
