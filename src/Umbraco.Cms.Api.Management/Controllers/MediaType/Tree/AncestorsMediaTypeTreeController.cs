using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

[ApiVersion("1.0")]
public class AncestorsMediaTypeTreeController : MediaTypeTreeControllerBase
{
    public AncestorsMediaTypeTreeController(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService, mediaTypeService)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MediaTypeTreeItemResponseModel>>> Ancestors(
        CancellationToken cancellationToken,
        Guid descendantId)
        => await GetAncestors(descendantId);
}
