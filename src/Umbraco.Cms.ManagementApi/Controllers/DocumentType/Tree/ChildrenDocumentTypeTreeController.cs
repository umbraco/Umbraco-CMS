using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.DocumentType.Tree;

public class ChildrenDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    public ChildrenDocumentTypeTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentTypeTreeItemViewModel>>> Children(Guid parentKey, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentKey, skip, take);
    }
}
