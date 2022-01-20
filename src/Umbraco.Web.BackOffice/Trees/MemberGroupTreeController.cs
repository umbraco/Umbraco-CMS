using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMemberGroups)]
    [Tree(Constants.Applications.Members, Constants.Trees.MemberGroups, SortOrder = 1)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    [CoreTree]
    public class MemberGroupTreeController : MemberTypeAndGroupTreeControllerBase
    {
        private readonly IMemberGroupService _memberGroupService;

        public MemberGroupTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            IMemberGroupService memberGroupService,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator)
            => _memberGroupService = memberGroupService;

        protected override IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings)
            => _memberGroupService.GetAll()
                .OrderBy(x => x.Name)
                .Select(dt => CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, Constants.Icons.MemberGroup, false));

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            ActionResult<TreeNode> rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            TreeNode root = rootResult.Value;

            //check if there are any groups
            root.HasChildren = _memberGroupService.GetAll().Any();
            return root;
        }
    }
}
