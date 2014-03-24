using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Security;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.member;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    //TODO: Upgrade thsi to use the new Member Service!

    //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
    // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
    [UmbracoApplicationAuthorize(
        Constants.Applications.Content,
        Constants.Applications.Media,
        Constants.Applications.Members)]
    [LegacyBaseTree(typeof (loadMembers))]
    [Tree(Constants.Applications.Members, Constants.Trees.Members, "Members")]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class MemberTreeController : TreeController
    {
        public MemberTreeController()
        {
            _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
        }

        private MembershipProvider _provider;

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
                if (_provider.IsUmbracoMembershipProvider())
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
                    if (_provider.IsUmbracoMembershipProvider())
                    {
                        int totalRecs;
                        var foundMembers = Services.MemberService.FindMembersByDisplayName(
                            id.ToCharArray()[0].ToString(CultureInfo.InvariantCulture), 0, int.MaxValue, out totalRecs, StringPropertyMatchType.StartsWith)
                            .ToArray();

                        //get the members from our member data layer
                        nodes.AddRange(
                            foundMembers
                                .Select(m => CreateTreeNode(m.Key.ToString("N"), id, queryStrings, m.Name, "icon-user")));
                    }
                    else
                    {
                        //get the members from the provider
                        int total;
                        nodes.AddRange(
                            FindUsersByName(char.Parse(id)).Cast<MembershipUser>()
                                      .Select(m => CreateTreeNode(GetNodeIdForCustomProvider(m.ProviderUserKey), id, queryStrings, m.UserName, "icon-user")));
                    }
                }
                else if (id == "others")
                {
                    //others will only show up when in umbraco membership mode
                    //TODO: We don't have a new API for this because we want to get rid of how this is displayed
                    nodes.AddRange(
                        Member.getAllOtherMembers()
                                    .Select(m => CreateTreeNode(m.UniqueId.ToString("N"), id, queryStrings, m.Text, "icon-user")));
                }
            }
            return nodes;
        }

        /// <summary>
        /// Allows for developers to override this in case their provider does some funky stuff to search
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        /// <remarks>
        /// We're going to do a special check here - for active dir provider or sql provider
        /// </remarks>
        protected virtual MembershipUserCollection FindUsersByName(char letter)
        {
            int total;
            if (_provider is SqlMembershipProvider)
            {
                //this provider uses the % syntax
                return _provider.FindUsersByName(letter + "%", 0, 9999, out total);
            }
            else
            {
                //the AD provider - and potentiall all other providers will use the asterisk syntax.
                return _provider.FindUsersByName(letter + "*", 0, 9999, out total);
            }
            
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
                if (_provider.IsUmbracoMembershipProvider())
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