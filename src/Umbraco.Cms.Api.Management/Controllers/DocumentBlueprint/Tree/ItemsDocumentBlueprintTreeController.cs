using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

public class ItemsDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    public ItemsDocumentBlueprintTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentBlueprintTreeItemResponseModel>>> Items([FromQuery(Name = "id")] Guid[] ids)
        => await GetItems(ids);
}
