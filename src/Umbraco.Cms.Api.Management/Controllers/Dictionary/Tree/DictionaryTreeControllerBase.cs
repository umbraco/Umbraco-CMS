using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
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
                Icon = Constants.Icons.Dictionary,
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
    protected IDictionaryItem[] PaginatedDictionaryItems(long pageNumber, int pageSize, IEnumerable<IDictionaryItem> allDictionaryItems, out long totalItems)
    {
        IDictionaryItem[] allDictionaryItemsAsArray = allDictionaryItems.ToArray();

        totalItems = allDictionaryItemsAsArray.Length;
        return allDictionaryItemsAsArray
            .OrderBy(item => item.ItemKey)
            .Skip((int)pageNumber * pageSize)
            .Take(pageSize)
            .ToArray();
    }
}
