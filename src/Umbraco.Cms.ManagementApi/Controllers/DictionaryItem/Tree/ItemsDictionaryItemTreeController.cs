using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.DictionaryItem.Tree;

public class ItemsDictionaryItemTreeController : DictionaryItemTreeControllerBase
{
    public ItemsDictionaryItemTreeController(IEntityService entityService, ILocalizationService localizationService)
        : base(entityService, localizationService)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FolderTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<FolderTreeItemViewModel>>> Items([FromQuery(Name = "key")] Guid[] keys)
    {
        // TODO: either make EntityService support relation types, or make LocalizationService able to query multiple relation types
        // - for now this workaround works somewhat, as long as a reasonable amount of items are requested.
        IDictionaryItem[] dictionaryItems = keys.Select(key => LocalizationService.GetDictionaryItemById(key))
            .WhereNotNull()
            .ToArray();

        EntityTreeItemViewModel[] viewModels = MapTreeItemViewModels(null, dictionaryItems);

        PagedViewModel<EntityTreeItemViewModel> result = PagedViewModel(viewModels, viewModels.Length);
        return await Task.FromResult(Ok(result));
    }
}
