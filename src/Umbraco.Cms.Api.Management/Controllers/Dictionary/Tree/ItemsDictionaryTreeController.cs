using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

public class ItemsDictionaryTreeController : DictionaryTreeControllerBase
{
    public ItemsDictionaryTreeController(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService, dictionaryItemService)
    {
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FolderTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FolderTreeItemViewModel>>> Items([FromQuery(Name = "key")] Guid[] keys)
    {
        IDictionaryItem[] dictionaryItems = (await DictionaryItemService.GetManyAsync(keys)).ToArray();

        EntityTreeItemViewModel[] viewModels = await MapTreeItemViewModels(null, dictionaryItems);

        return await Task.FromResult(Ok(viewModels));
    }
}
