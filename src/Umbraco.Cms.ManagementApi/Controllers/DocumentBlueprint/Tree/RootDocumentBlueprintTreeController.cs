using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.DocumentBlueprint.Tree;

public class RootDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    public RootDocumentBlueprintTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentBlueprintTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentBlueprintTreeItemViewModel>>> Root(int skip = 0, int take = 100)
        => await GetRoot(skip, take);
}
