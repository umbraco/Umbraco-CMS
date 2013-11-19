using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.member;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    //TODO: Upgrade thsi to use the new Member Service!

    [LegacyBaseTree(typeof (loadMembers))]
    [Tree(Constants.Applications.Members, Constants.Trees.Members, "Members")]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class MemberTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //list out all the letters
                for (var i = 97; i < 123; i++)
                {
                    var charString = ((char) i).ToString(CultureInfo.InvariantCulture);
                    var folder = CreateTreeNode(charString, id, queryStrings, charString, "icon-folder-close", true);
                    folder.NodeType = "member-folder";
                    nodes.Add(folder);
                }
                //list out 'Others' if the membership provider is umbraco
                if (Membership.Provider.Name == Constants.Conventions.Member.UmbracoMemberProviderName)
                {
                    var folder = CreateTreeNode("others", id, queryStrings, "Others", "icon-folder-close", true);
                    folder.NodeType = "member-folder";
                    nodes.Add(folder);
                }
            }
            else
            {
                //if it is a letter
                if (id.Length == 1 && char.IsLower(id, 0))
                {
                    if (Membership.Provider.Name == Constants.Conventions.Member.UmbracoMemberProviderName)
                    {
                        //get the members from our member data layer
                        nodes.AddRange(
                            Member.getMemberFromFirstLetter(id.ToCharArray()[0])
                                        .Select(m => CreateTreeNode(m.UniqueId.ToString("N"), id, queryStrings, m.Text, "icon-user")));
                    }
                    else
                    {
                        //get the members from the provider
                        int total;
                        nodes.AddRange(
                            Membership.Provider.FindUsersByName(id + "%", 0, 9999, out total).Cast<MembershipUser>()
                                      .Select(m => CreateTreeNode(GetNodeIdForCustomProvider(m.ProviderUserKey), id, queryStrings, m.UserName, "icon-user")));
                    }
                }
                else if (id == "others")
                {
                    //others will only show up when in umbraco membership mode
                    nodes.AddRange(
                        Member.getAllOtherMembers()
                                    .Select(m => CreateTreeNode(m.Id.ToInvariantString(), id, queryStrings, m.Text, "icon-user")));
                }
            }
            return nodes;
        }

        /// <summary>
        /// We'll see if it is a GUID, if so we'll ensure to format it without hyphens
        /// </summary>
        /// <param name="providerUserKey"></param>
        /// <returns></returns>
        private string GetNodeIdForCustomProvider(object providerUserKey)
        {
            if (providerUserKey == null) throw new ArgumentNullException("providerUserKey");
            var guidAttempt = providerUserKey.TryConvertTo<Guid>();
            if (guidAttempt.Success)
            {
                return guidAttempt.Result.ToString("N");
            }
            return providerUserKey.ToString();
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions      
                if (Membership.Provider.Name == Constants.Conventions.Member.UmbracoMemberProviderName)
                {
                    //set default
                    menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                    //Create the normal create action
                    menu.Items.Add<ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                }
                else
                {
                    //Create a custom create action - this does not launch a dialog, it just navigates to the create screen
                    // we'll create it baesd on the ActionNew so it maintains the same icon properties, name, etc...
                    var createMenuItem = new MenuItem(ActionNew.Instance);
                    //we want to go to this route: /member/member/edit/-1?create=true
                    createMenuItem.NavigateToRoute("/member/member/edit/-1?create=true");
                    menu.Items.Add(createMenuItem);
                }
                
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            Guid guid;
            if (Guid.TryParse(id, out guid))
            {
                menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
            }
            else
            {
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), false);    
            }
            return menu;
        }
    }
}