using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.SectionAccessForMemberTree)]
    [Tree(Constants.Applications.Members, Constants.Trees.Members, SortOrder = 0)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    [SearchableTree("searchResultFormatter", "configureMemberResult")]
    public class MemberTreeController : TreeController, ISearchableTree, ITreeNodeController
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public MemberTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            UmbracoTreeSearcher treeSearcher,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _treeSearcher = treeSearcher;
            _menuItemCollectionFactory = menuItemCollectionFactory;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
        }

        /// <summary>
        /// Gets an individual tree node
        /// </summary>
        public ActionResult<TreeNode> GetTreeNode([FromRoute]string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]FormCollection queryStrings)
        {
            ActionResult<TreeNode> node = GetSingleTreeNode(id, queryStrings);

            if (!(node.Result is null))
            {
                return node.Result;
            }

            //add the tree alias to the node since it is standalone (has no root for which this normally belongs)
            node.Value.AdditionalData["treeAlias"] = TreeAlias;
            return node;
        }

        protected ActionResult<TreeNode> GetSingleTreeNode(string id, FormCollection queryStrings)
        {
            Guid asGuid;
            if (Guid.TryParse(id, out asGuid) == false)
            {
                return NotFound();
            }

            var member = _memberService.GetByKey(asGuid);
            if (member == null)
            {
                return NotFound();
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

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.RootString)
            {
                nodes.Add(
                        CreateTreeNode(Constants.Conventions.MemberTypes.AllMembersListId, id, queryStrings, LocalizedTextService.Localize("member/allMembers"), Constants.Icons.MemberType, true,
                            queryStrings.GetRequiredValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/" + Constants.Conventions.MemberTypes.AllMembersListId));

                nodes.AddRange(_memberTypeService.GetAll()
                        .Select(memberType =>
                            CreateTreeNode(memberType.Alias, id, queryStrings, memberType.Name, memberType.Icon.IfNullOrWhiteSpace(Constants.Icons.Member), true,
                                queryStrings.GetRequiredValue<string>("application") + TreeAlias.EnsureStartsWith('/') + "/list/" + memberType.Alias)));
            }

            //There is no menu for any of these nodes
            nodes.ForEach(x => x.MenuUrl = null);
            //All nodes are containers
            nodes.ForEach(x => x.AdditionalData.Add("isContainer", true));

            return nodes;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menu = _menuItemCollectionFactory.Create();

            if (id == Constants.System.RootString)
            {
                // root actions
                //set default
                menu.DefaultMenuAlias = ActionNew.ActionAlias;

                //Create the normal create action
                menu.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);

                menu.Items.Add(new RefreshNode(LocalizedTextService, true));
                return menu;
            }

            //add delete option for all members
            menu.Items.Add<ActionDelete>(LocalizedTextService, opensDialog: true);

            if (_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.HasAccessToSensitiveData())
            {
                menu.Items.Add(new ExportMember(LocalizedTextService));
            }

            return menu;
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            return _treeSearcher.ExamineSearch(query, UmbracoEntityTypes.Member, pageSize, pageIndex, out totalFound, searchFrom);
        }
    }
}
