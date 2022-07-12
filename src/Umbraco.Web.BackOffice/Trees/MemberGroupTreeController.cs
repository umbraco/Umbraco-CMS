using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Web.BackOffice.Trees;

[Authorize(Policy = AuthorizationPolicies.TreeAccessMemberGroups)]
[Tree(Constants.Applications.Members, Constants.Trees.MemberGroups, SortOrder = 1)]
[PluginController(Constants.Web.Mvc.BackOfficeTreeArea)]
[CoreTree]
public class MemberGroupTreeController : MemberTypeAndGroupTreeControllerBase
{
    private readonly IMemberGroupService _memberGroupService;

    [
        ActivatorUtilitiesConstructor]
    public MemberGroupTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IMemberGroupService memberGroupService,
        IEventAggregator eventAggregator,
        IMemberTypeService memberTypeService)
        : base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator,
            memberTypeService)
        => _memberGroupService = memberGroupService;

    [Obsolete("Use ctor with all params")]
    public MemberGroupTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IMenuItemCollectionFactory menuItemCollectionFactory,
        IMemberGroupService memberGroupService,
        IEventAggregator eventAggregator)
        : this(localizedTextService,
            umbracoApiControllerTypeCollection,
            menuItemCollectionFactory,
            memberGroupService,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>())
    {
    }

    protected override IEnumerable<TreeNode> GetTreeNodesFromService(string id, FormCollection queryStrings)
        => _memberGroupService.GetAll()
            .OrderBy(x => x.Name)
            .Select(dt =>
                CreateTreeNode(dt.Id.ToString(), id, queryStrings, dt.Name, Constants.Icons.MemberGroup, false));

    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        ActionResult<TreeNode?> rootResult = base.CreateRootNode(queryStrings);
        if (!(rootResult.Result is null))
        {
            return rootResult.Result;
        }

        TreeNode? root = rootResult.Value;

        if (root is not null)
        {
            // Check if there are any groups
            root.HasChildren = _memberGroupService.GetAll().Any();
        }

        return root;
    }
}
