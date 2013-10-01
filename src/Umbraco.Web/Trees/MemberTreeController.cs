using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees.Menu;
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
                    nodes.Add(CreateTreeNode(charString, queryStrings, charString, "icon-folder-close", true));
                }
                //list out 'Others' if the membership provider is umbraco
                if (Member.InUmbracoMemberMode())
                {
                    nodes.Add(CreateTreeNode("others", queryStrings, "Others", "icon-folder-close", true));
                }
            }
            else
            {
                //if it is a letter
                if (id.Length == 1 && char.IsLower(id, 0))
                {
                    if (Member.InUmbracoMemberMode())
                    {
                        //get the members from our member data layer
                        nodes.AddRange(
                            Member.getMemberFromFirstLetter(id.ToCharArray()[0])
                                        .Select(m => CreateTreeNode(m.UniqueId.ToString("N"), queryStrings, m.Text, "icon-user")));
                    }
                    else
                    {
                        //get the members from the provider
                        int total;
                        nodes.AddRange(
                            Membership.Provider.FindUsersByName(id + "%", 0, 9999, out total).Cast<MembershipUser>()
                                      .Select(m => CreateTreeNode(m.ProviderUserKey.ToString(), queryStrings, m.UserName, "icon-user")));
                    }
                }
                else if (id == "others")
                {
                    //others will only show up when in umbraco membership mode
                    nodes.AddRange(
                        Member.getAllOtherMembers()
                                    .Select(m => CreateTreeNode(m.Id.ToInvariantString(), queryStrings, m.Text, "icon-user")));
                }
            }
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //set default
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // root actions         
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<RefreshNode, ActionRefresh>(true);
                return menu;
            }

            menu.AddMenuItem<ActionDelete>();
            menu.AddMenuItem<RefreshNode, ActionRefresh>(true);
            return menu;
        }
    }
}