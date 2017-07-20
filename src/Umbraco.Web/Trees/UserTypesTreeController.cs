using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web._Legacy.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.UserTypes)]
    [Tree(Constants.Applications.Users, Constants.Trees.UserTypes, null, sortOrder: 1)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class UserTypesTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            var userTypes = Services.UserService.GetAllUserTypes();
            userTypes = userTypes.OrderBy(ut => ut.Name);

            foreach (var userType in userTypes)
            {
                nodes.Add(
                    CreateTreeNode(
                        userType.Id.ToString(CultureInfo.InvariantCulture),
                        "-1",
                        queryStrings,
                        userType.Name,
                        "icon-users",
                        false,
                        "/" + queryStrings.GetValue<string>("application") + "/framed/"
                        + Uri.EscapeDataString("users/EditUserType.aspx?id=" + userType.Id)));
            }

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions
                menu.Items.Add<ActionNew>(Services.TextService.Localize("actions", ActionNew.Instance.Alias))
                    .ConvertLegacyMenuItem(null, "userTypes", queryStrings.GetValue<string>("application"));

                menu.Items.Add<RefreshNode, ActionRefresh>(Services.TextService.Localize("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            // delete user type
            menu.Items.Add<ActionDelete>(Services.TextService.Localize("actions", ActionDelete.Instance.Alias))
                .ConvertLegacyMenuItem(null, "userTypes", queryStrings.GetValue<string>("application"));

            return menu;
        }
    }
}
