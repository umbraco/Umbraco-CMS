using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

[ApiVersion("1.0")]
public class ChildrenDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    public ChildrenDocumentBlueprintTreeController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentBlueprintTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
