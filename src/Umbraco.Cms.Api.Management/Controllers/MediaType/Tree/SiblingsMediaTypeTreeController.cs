using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
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
    public SiblingsMediaTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMediaTypeService mediaTypeService)
        : base(entityService, flagProviders, mediaTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<MediaTypeTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetSiblings(target, before, after);
    }
}
