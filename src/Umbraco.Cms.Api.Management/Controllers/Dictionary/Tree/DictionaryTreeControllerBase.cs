using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/dictionary")]
[ApiExplorerSettings(GroupName = "Dictionary")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDictionaryOrTemplates)]
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

    protected async Task<IEnumerable<NamedEntityTreeItemResponseModel>> MapTreeItemViewModels(Guid? parentKey, IEnumerable<IDictionaryItem> dictionaryItems)
    {
        async Task<NamedEntityTreeItemResponseModel> CreateEntityTreeItemViewModelAsync(IDictionaryItem dictionaryItem)
        {
            var hasChildren = await DictionaryItemService.CountChildrenAsync(dictionaryItem.Key) > 0;
            return new NamedEntityTreeItemResponseModel
            {
                Name = dictionaryItem.ItemKey,
                Id = dictionaryItem.Key,
                Type = Constants.UdiEntityType.DictionaryItem,
                HasChildren = hasChildren,
                Parent = parentKey.HasValue
                    ? new ReferenceByIdModel
                    {
                        Id = parentKey.Value
                    }
                    : null
            };
        }

        return await Task.WhenAll(dictionaryItems.Select(CreateEntityTreeItemViewModelAsync));
    }
}
