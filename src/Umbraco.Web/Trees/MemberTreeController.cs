using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Security;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
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
            _isUmbracoProvider = _provider.IsUmbracoMembershipProvider();
        }

        private readonly MembershipProvider _provider;
        private readonly bool _isUmbracoProvider;

        /// <summary>
        /// Gets an individual tree node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        [HttpQueryStringFilter("queryStrings")]
        public TreeNode GetTreeNode(string id, FormDataCollection queryStrings)
        {   
            var node = GetSingleTreeNode(id, queryStrings);

            //add the tree alias to the node since it is standalone (has no root for which this normally belongs)
            node.AdditionalData["treeAlias"] = TreeAlias;
            return node;
        }

        protected TreeNode GetSingleTreeNode(string id, FormDataCollection queryStrings)
        {
            if (_isUmbracoProvider)
            {
                Guid asGuid;
                if (Guid.TryParse(id, out asGuid) == false)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                var member = Services.MemberService.GetByKey(asGuid);
                if (member == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                var node = CreateTreeNode(
                    member.Key.ToString("N"),
                    "-1",
                    queryStrings,
                    member.Name,
                    "icon-user",
                    false);

                node.AdditionalData.Add("contentType", member.ContentTypeAlias);

                return node;
            }
            else
            {
                var member = _provider.GetUser(id, false);
                if (member == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                var node = CreateTreeNode(
                    member.ProviderUserKey.ToString(),
                    "-1",
                    queryStrings,
                    member.UserName,
                    "icon-user",
                    false);

                return node;    
            }

            
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                nodes.Add(
                        CreateTreeNode("all-members", id, queryStrings, "All Members", "icon-users", false,
                            queryStrings.GetValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/all-members"));

                if (_isUmbracoProvider)
                {
                    nodes.AddRange(Services.MemberTypeService.GetAll()
                        .Select(memberType =>
                            CreateTreeNode(memberType.Alias, id, queryStrings, memberType.Name, "icon-users", false,
                                queryStrings.GetValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/" + memberType.Alias)));
                }
            }

            //There is no menu for any of these nodes
            nodes.ForEach(x => x.MenuUrl = null);

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
                //the AD provider - and potentially all other providers will use the asterisk syntax.
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
                    // we'll create it based on the ActionNew so it maintains the same icon properties, name, etc...
                    var createMenuItem = new MenuItem(ActionNew.Instance);
                    //we want to go to this route: /member/member/edit/-1?create=true
                    createMenuItem.NavigateToRoute("/member/member/edit/-1?create=true");
                    menu.Items.Add(createMenuItem);
                }
                
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }

            //add delete option for all members
            menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));

            return menu;
        }
    }
}