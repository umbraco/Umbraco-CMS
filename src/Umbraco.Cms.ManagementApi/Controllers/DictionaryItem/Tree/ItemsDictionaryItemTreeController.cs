using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.DictionaryItem.Tree;

public class ItemsDictionaryItemTreeController : DictionaryItemTreeControllerBase
{
    public ItemsDictionaryItemTreeController(IEntityService entityService, ILocalizationService localizationService)
        : base(entityService, localizationService)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FolderTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FolderTreeItemViewModel>>> Items([FromQuery(Name = "key")] Guid[] keys)
    {
        IDictionaryItem[] dictionaryItems = LocalizationService.GetDictionaryItemsByIds(keys).ToArray();

        EntityTreeItemViewModel[] viewModels = MapTreeItemViewModels(null, dictionaryItems);

        return await Task.FromResult(Ok(viewModels));
    }
}
