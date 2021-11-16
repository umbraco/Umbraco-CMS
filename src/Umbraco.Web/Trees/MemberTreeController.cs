using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Security;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Search;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
    // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
    [UmbracoApplicationAuthorize(
        Constants.Applications.Content,
        Constants.Applications.Media,
        Constants.Applications.Members)]
    [Tree(Constants.Applications.Members, Constants.Trees.Members, SortOrder = 0)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureMemberResult")]
    public class MemberTreeController : TreeController, ISearchableTree
    {
        public MemberTreeController(UmbracoTreeSearcher treeSearcher)
        {
            _treeSearcher = treeSearcher;
            _provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
            _isUmbracoProvider = _provider.IsUmbracoMembershipProvider();
        }

        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly MembershipProvider _provider;
        private readonly bool _isUmbracoProvider;

        /// <summary>
        /// Gets an individual tree node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public TreeNode GetTreeNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormDataCollection queryStrings)
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
                    Constants.Icons.Member,
                    false,
                    "",
                    Udi.Create(ObjectTypes.GetUdiType(Constants.ObjectTypes.Member), member.Key));

                node.AdditionalData.Add("contentType", member.ContentTypeAlias);
                node.AdditionalData.Add("isContainer", true);

                return node;
            }
            else
            {
                object providerId = id;
                Guid asGuid;
                if (Guid.TryParse(id, out asGuid))
                {
                    providerId = asGuid;
                }

                var member = _provider.GetUser(providerId, false);
                if (member == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                var node = CreateTreeNode(
                    member.ProviderUserKey.TryConvertTo<Guid>().Result.ToString("N"),
                    "-1",
                    queryStrings,
                    member.UserName,
                    Constants.Icons.Member,
                    false);

                return node;
            }
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.RootString)
            {
                nodes.Add(
                        CreateTreeNode(Constants.Conventions.MemberTypes.AllMembersListId, id, queryStrings, Services.TextService.Localize("member", "allMembers"), Constants.Icons.MemberType, true,
                            queryStrings.GetRequiredValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/" + Constants.Conventions.MemberTypes.AllMembersListId));

                if (_isUmbracoProvider)
                {
                    nodes.AddRange(
                        Services.MemberTypeService.GetAll()
                            .OrderBy(x => x.Name)
                            .Select(memberType =>
                                CreateTreeNode(memberType.Alias, id, queryStrings, memberType.Name, memberType.Icon.IfNullOrWhiteSpace(Constants.Icons.Member), true,
                                    queryStrings.GetRequiredValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/" + memberType.Alias)));
                }
            }

            //There is no menu for any of these nodes
            nodes.ForEach(x => x.MenuUrl = null);
            //All nodes are containers
            nodes.ForEach(x => x.AdditionalData.Add("isContainer", true));

            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.RootString)
            {
                // root actions
                if (_provider.IsUmbracoMembershipProvider())
                {
                    //set default
                    menu.DefaultMenuAlias = ActionNew.ActionAlias;

                    //Create the normal create action
                    menu.Items.Add<ActionNew>(Services.TextService, opensDialog: true);
                }
                else
                {
                    //Create a custom create action - this does not launch a dialog, it just navigates to the create screen
                    // we'll create it based on the ActionNew so it maintains the same icon properties, name, etc...
                    var createMenuItem = new MenuItem(ActionNew.ActionAlias, Services.TextService)
                    {
                        Icon = "add",
                        OpensDialog = true
                    };
                    //we want to go to this route: /member/member/edit/-1?create=true
                    createMenuItem.NavigateToRoute("/member/member/edit/-1?create=true");
                    menu.Items.Add(createMenuItem);
                }

                menu.Items.Add(new RefreshNode(Services.TextService, true));
                return menu;
            }

            //add delete option for all members
            menu.Items.Add<ActionDelete>(Services.TextService, opensDialog: true);

            if (Security.CurrentUser.HasAccessToSensitiveData())
            {
                menu.Items.Add(new ExportMember(Services.TextService));
            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            return _treeSearcher.ExamineSearch(query, UmbracoEntityTypes.Member, pageSize, pageIndex, out totalFound, searchFrom);
        }
    }
}
