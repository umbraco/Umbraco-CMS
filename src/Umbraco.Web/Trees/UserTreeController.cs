using System.Net.Http.Formatting;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoTreeAuthorize(Constants.Trees.Users)]
    [Tree(Constants.Applications.Users, Constants.Trees.Users, null, sortOrder: 0)]
    [PluginController("UmbracoTrees")]
    [LegacyBaseTree(typeof(loadUsers))]
    [CoreTree]
    public class UserTreeController : TreeController
    {
        public UserTreeController()
        {
        }

        public UserTreeController(UmbracoContext umbracoContext) : base(umbracoContext)
        {
        }

        public UserTreeController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper) : base(umbracoContext, umbracoHelper)
        {
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            //this will load in a custom UI instead of the dashboard for the root node
            root.RoutePath = string.Format("{0}/{1}/{2}", Constants.Applications.Users, Constants.Trees.Users, "overview");
            root.Icon = "icon-users";

            root.HasChildren = false;
            return root;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //Create User
                var createMenuItem = menu.Items.CreateMenuItem<ActionNew>(Services.TextService.Localize("actions/create"));
                createMenuItem.Icon = "add";
                createMenuItem.NavigateToRoute("users/users/overview?subview=users&create=true");
                menu.Items.Add(createMenuItem);
                
                //This is the same setting used in the global JS for 'showUserInvite'
                if (EmailSender.CanSendRequiredEmail)
                {
                    //Invite User (Action import closest type of action to an invite user)
                    var inviteMenuItem = menu.Items.CreateMenuItem<ActionImport>(Services.TextService.Localize("user/invite"));
                    inviteMenuItem.Icon = "message-unopened";
                    inviteMenuItem.NavigateToRoute("users/users/overview?subview=users&invite=true");

                    menu.Items.Add(inviteMenuItem);
                }

                return menu;
            }

            //There is no context menu options for editing a specific user
            //Also we no longer list each user in the tree & in theory never hit this
            return menu;
        }
    }
}
