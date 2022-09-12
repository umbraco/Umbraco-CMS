using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.DictionaryItem.Tree;

public class ChildrenDictionaryItemTreeController : DictionaryItemTreeControllerBase
{
    public ChildrenDictionaryItemTreeController(IEntityService entityService, ILocalizationService localizationService)
        : base(entityService, localizationService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedResult<EntityTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EntityTreeItemViewModel>>> Children(Guid parentKey, long pageNumber = 0, int pageSize = 100)
    {
        IDictionaryItem[] dictionaryItems = LocalizationService.GetDictionaryItemChildren(parentKey).ToArray();

        EntityTreeItemViewModel[] viewModels = MapTreeItemViewModels(null, dictionaryItems);

        PagedResult<EntityTreeItemViewModel> result = PagedResult(viewModels, pageNumber, pageSize, viewModels.Length);
        return await Task.FromResult(Ok(result));
    }
}
