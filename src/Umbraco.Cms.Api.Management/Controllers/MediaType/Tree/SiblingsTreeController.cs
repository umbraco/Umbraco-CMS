using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

public class SiblingsTreeController : MediaTypeTreeControllerBase
{
    public SiblingsTreeController(IEntityService entityService, IMediaTypeService mediaTypeService) : base(entityService, mediaTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<MediaTypeTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after)
        => GetSiblings(target, before, after);
}
