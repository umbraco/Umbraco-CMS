using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Trees;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Search;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Trees
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
            IMemberTypeService memberTypeService)
            : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory)
        {
            _treeSearcher = treeSearcher;
            _memberTypeService = memberTypeService;
        }


        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            //check if there are any member types
            root.HasChildren = _memberTypeService.GetAll().Any();
            return root;
        }
        protected override IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings)
        {
            return _memberTypeService.GetAll()
                .OrderBy(x => x.Name)
                .Select(dt => CreateTreeNode(dt, Constants.ObjectTypes.MemberType, id, queryStrings, dt?.Icon ?? Constants.Icons.MemberType, false));
        }

        public IEnumerable<SearchResultEntity> Search(string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
            => _treeSearcher.EntitySearch(UmbracoObjectTypes.MemberType, query, pageSize, pageIndex, out totalFound, searchFrom);

    }
}
