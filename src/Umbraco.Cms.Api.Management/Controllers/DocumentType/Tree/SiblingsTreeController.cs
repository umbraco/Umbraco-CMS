using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

public class SiblingsTreeController : DocumentTypeTreeControllerBase
{
    public SiblingsTreeController(IEntityService entityService, IContentTypeService contentTypeService) : base(entityService, contentTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<DocumentTypeTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after) =>
        GetSiblings(target, before, after);
}
