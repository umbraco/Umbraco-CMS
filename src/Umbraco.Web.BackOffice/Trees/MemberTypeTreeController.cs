using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Trees
{
    [CoreTree]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
    [Tree(Constants.Applications.Settings, Constants.Trees.MemberTypes, SortOrder = 2, TreeGroup = Constants.Trees.Groups.Settings)]
    [PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
    public class MemberTypeTreeController : MemberTypeAndGroupTreeControllerBase, ISearchableTree
    {
        private readonly UmbracoTreeSearcher _treeSearcher;
        private readonly IMemberTypeService _memberTypeService;

        public MemberTypeTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IMenuItemCollectionFactory menuItemCollectionFactory,
            UmbracoTreeSearcher treeSearcher,
            IMemberTypeService memberTypeService,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator, memberTypeService)
        {
            _treeSearcher = treeSearcher;
            _memberTypeService = memberTypeService;
        }

        protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
        {
            var rootResult = base.CreateRootNode(queryStrings);
            if (!(rootResult.Result is null))
            {
                return rootResult;
            }
            var root = rootResult.Value;

            if (root is not null)
            {
                // Check if there are any member types
                root.HasChildren = _memberTypeService.GetAll().Any();
            }

            return root;
        }

        protected override IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings)
        {
            return _memberTypeService.GetAll()
                .OrderBy(x => x.Name)
                .Select(dt => CreateTreeNode(dt, Constants.ObjectTypes.MemberType, id, queryStrings, dt?.Icon ?? Constants.Icons.MemberType, false));
        }

        public IEnumerable<SearchResultEntity?> Search(string query, int pageSize, long pageIndex, out long totalFound, string? searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.MemberType, query, pageSize, pageIndex, out totalFound, searchFrom);

    }
}
