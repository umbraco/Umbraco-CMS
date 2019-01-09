using System;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.UserPermissions)]
    [Tree(Constants.Applications.Users, Constants.Trees.UserPermissions, null, sortOrder: 2)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class UserPermissionsTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            long totalUsers;
            nodes.AddRange(
                Services.UserService.GetAll(0, int.MaxValue, out totalUsers)
                    .Where(x => x.Id != Constants.Security.SuperUserId && x.IsApproved)
                    .Select(x => CreateTreeNode(x.Id.ToString(),
                        id,
                        queryStrings,
                        x.Name,
                        "icon-user",
                        false,
                        "/" + queryStrings.GetValue<string>("application") + "/framed/"
                        + Uri.EscapeDataString("users/PermissionEditor.aspx?id=" + x.Id))));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions
                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }

            return menu;
        }
    }
}
