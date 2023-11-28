using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDictionaryOrTemplates)]
// NOTE: at the moment dictionary items (renamed to dictionary tree) aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class DictionaryTreeControllerBase : EntityTreeControllerBase<EntityTreeItemResponseModel>
{
    public DictionaryTreeControllerBase(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService) =>
        DictionaryItemService = dictionaryItemService;

    // dictionary items do not currently have a known UmbracoObjectType, so we'll settle with Unknown for now
    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Unknown;

    protected IDictionaryItemService DictionaryItemService { get; }

    protected async Task<EntityTreeItemResponseModel[]> MapTreeItemViewModels(Guid? parentKey, IDictionaryItem[] dictionaryItems)
    {
        async Task<EntityTreeItemResponseModel> CreateEntityTreeItemViewModelAsync(IDictionaryItem dictionaryItem)
        {
            var hasChildren = (await DictionaryItemService.GetChildrenAsync(dictionaryItem.Key)).Any();
            return new EntityTreeItemResponseModel
            {
                Name = dictionaryItem.ItemKey,
                Id = dictionaryItem.Key,
                Type = Constants.UdiEntityType.DictionaryItem,
                HasChildren = hasChildren,
                IsContainer = false,
                ParentId = parentKey
            };
        }

        var items = new List<EntityTreeItemResponseModel>(dictionaryItems.Length);
        foreach (IDictionaryItem dictionaryItem in dictionaryItems)
        {
            items.Add(await CreateEntityTreeItemViewModelAsync(dictionaryItem));
        }

        return items.ToArray();
    }

    // language service does not (yet) allow pagination of dictionary items, we have to do it in memory for now
    protected async Task<PagedModel<IDictionaryItem>> PaginatedDictionaryItems(long pageNumber, int pageSize, Guid? parentId)
    {
        if (pageSize == 0)
        {
            return new PagedModel<IDictionaryItem>
            {
                Items = Enumerable.Empty<IDictionaryItem>(),
                Total = parentId.HasValue
                    ? await DictionaryItemService.CountChildrenAsync(parentId.Value)
                    : await DictionaryItemService.CountRootAsync()
            };
        }

        IDictionaryItem[] allDictionaryItemsAsArray = parentId.HasValue
            ? (await DictionaryItemService.GetChildrenAsync(parentId.Value)).ToArray()
            : (await DictionaryItemService.GetAtRootAsync()).ToArray();

        IDictionaryItem[] items = allDictionaryItemsAsArray
            .OrderBy(item => item.ItemKey)
            .Skip((int)pageNumber * pageSize)
            .Take(pageSize)
            .ToArray();

        return new PagedModel<IDictionaryItem> { Items = items, Total = items.Length };
    }
}
