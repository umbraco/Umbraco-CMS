using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.DictionaryItem.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DictionaryItem}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.DictionaryItem))]
// NOTE: at the moment dictionary items aren't supported by EntityService, so we have little use of the
// tree controller base. We'll keep it though, in the hope that we can mend EntityService.
public class DictionaryItemTreeControllerBase : EntityTreeControllerBase<EntityTreeItemViewModel>
{
    public DictionaryItemTreeControllerBase(IEntityService entityService, ILocalizationService localizationService)
        : base(entityService) =>
        LocalizationService = localizationService;

    // dictionary items do not currently have a known UmbracoObjectType, so we'll settle with Unknown for now
    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Unknown;

    protected ILocalizationService LocalizationService { get; }

    protected EntityTreeItemViewModel[] MapTreeItemViewModels(Guid? parentKey, IDictionaryItem[] dictionaryItems)
        => dictionaryItems.Select(dictionaryItem => new EntityTreeItemViewModel
        {
            Icon = Constants.Icons.RelationType,
            Name = dictionaryItem.ItemKey,
            Key = dictionaryItem.Key,
            Type = Constants.UdiEntityType.DictionaryItem,
            HasChildren = false,
            IsContainer = LocalizationService.GetDictionaryItemChildren(dictionaryItem.Key).Any(),
            ParentKey = parentKey
        }).ToArray();

    // localization service does not (yet) allow pagination of dictionary items, we have to do it in memory for now
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
