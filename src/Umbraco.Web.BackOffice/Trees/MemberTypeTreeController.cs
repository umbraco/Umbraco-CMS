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

namespace Umbraco.Cms.Web.BackOffice.Trees;

[CoreTree]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberTypes)]
[Tree(Constants.Applications.Settings, Constants.Trees.MemberTypes, SortOrder = 2,
    TreeGroup = Constants.Trees.Groups.Settings)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
public class MemberTypeTreeController : MemberTypeAndGroupTreeControllerBase, ISearchableTree
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly UmbracoTreeSearcher _treeSearcher;

    public MemberTypeTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        UmbracoTreeSearcher treeSearcher,
        IMemberTypeService memberTypeService,
        IEventAggregator eventAggregator)
        : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator,
            memberTypeService)
    {
        _treeSearcher = treeSearcher;
        _memberTypeService = memberTypeService;
    }

    public async Task<EntitySearchResults> SearchAsync(string query, int pageSize, long pageIndex,
        string? searchFrom = null)
    {
        IEnumerable<SearchResultEntity?> results = _treeSearcher.EntitySearch(UmbracoObjectTypes.MemberType, query,
            pageSize, pageIndex, out var totalFound, searchFrom);
        return new EntitySearchResults(results, totalFound);
    }

    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        ActionResult<TreeNode?> rootResult = base.CreateRootNode(queryStrings);
        if (!(rootResult.Result is null))
        {
            return rootResult;
        }

        TreeNode? root = rootResult.Value;

        if (root is not null)
        {
            // Check if there are any member types
            root.HasChildren = _memberTypeService.GetAll().Any();
        }

        return root;
    }

    protected override IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings) =>
        _memberTypeService.GetAll()
            .OrderBy(x => x.Name)
            .Select(dt => CreateTreeNode(dt, Constants.ObjectTypes.MemberType, id, queryStrings,
                dt?.Icon ?? Constants.Icons.MemberType, false));
}
