using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

public class SiblingsMediaTypeTreeController : MediaTypeTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsMediaTypeTreeController(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService, mediaTypeService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public SiblingsMediaTypeTreeController(IEntityService entityService, SignProviderCollection signProviders, IMediaTypeService mediaTypeService)
        : base(entityService, signProviders, mediaTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<MediaTypeTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after)
        => GetSiblings(target, before, after);
}
