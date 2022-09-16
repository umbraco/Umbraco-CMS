using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.DocumentType.Tree;

public class RootDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    public RootDocumentTypeTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DocumentTypeTreeItemViewModel>>> Root(long pageNumber = 0, int pageSize = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(pageNumber, pageSize);
    }
}
