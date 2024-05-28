using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDictionaryOrTemplates)]
// NOTE: at the moment dictionary items (renamed to dictionary tree) aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class DictionaryTreeControllerBase : NamedEntityTreeControllerBase<NamedEntityTreeItemResponseModel>
{
    public DictionaryTreeControllerBase(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService) =>
        DictionaryItemService = dictionaryItemService;

    // dictionary items do not currently have a known UmbracoObjectType, so we'll settle with Unknown for now
    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Unknown;

    protected IDictionaryItemService DictionaryItemService { get; }

    protected async Task<IEnumerable<NamedEntityTreeItemResponseModel>> MapTreeItemViewModels(IEnumerable<IDictionaryItem> dictionaryItems)
        => await MapTreeItemViewModelsAsync(dictionaryItems).ToArrayAsync();

    protected override async Task<ActionResult<IEnumerable<NamedEntityTreeItemResponseModel>>> GetAncestors(Guid descendantKey, bool includeSelf = true)
    {
        IDictionaryItem? dictionaryItem = await DictionaryItemService.GetAsync(descendantKey);
        if (dictionaryItem is null)
        {
            // this looks weird - but we actually mimic how the rest of the ancestor (and children) endpoints actually work
            return Ok(Enumerable.Empty<NamedEntityTreeItemResponseModel>());
        }

        var ancestors = new List<IDictionaryItem>();
        if (includeSelf)
        {
            ancestors.Add(dictionaryItem);
        }

        while (dictionaryItem?.ParentId is not null)
        {
            dictionaryItem = await DictionaryItemService.GetAsync(dictionaryItem.ParentId.Value);
            if (dictionaryItem is not null)
            {
                ancestors.Add(dictionaryItem);
            }
        }

        NamedEntityTreeItemResponseModel[] viewModels = await MapTreeItemViewModelsAsync(ancestors).ToArrayAsync();

        return Ok(viewModels.Reverse());
    }

    private async IAsyncEnumerable<NamedEntityTreeItemResponseModel> MapTreeItemViewModelsAsync(IEnumerable<IDictionaryItem> dictionaryItems)
    {
        foreach (IDictionaryItem dictionaryItem in dictionaryItems)
        {
            yield return await CreateEntityTreeItemViewModelAsync(dictionaryItem);
        }
    }

    private async Task<NamedEntityTreeItemResponseModel> CreateEntityTreeItemViewModelAsync(IDictionaryItem dictionaryItem)
    {
        var hasChildren = await DictionaryItemService.CountChildrenAsync(dictionaryItem.Key) > 0;
        return new NamedEntityTreeItemResponseModel
        {
            Name = dictionaryItem.ItemKey,
            Id = dictionaryItem.Key,
            HasChildren = hasChildren,
            Parent = dictionaryItem.ParentId.HasValue
                ? new ReferenceByIdModel
                {
                    Id = dictionaryItem.ParentId.Value
                }
                : null
        };
    }
}
