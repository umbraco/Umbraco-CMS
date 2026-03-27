using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

/// <summary>
/// Controller responsible for handling operations at the root of the media type tree in the Umbraco CMS Management API.
/// </summary>
[ApiVersion("1.0")]
public class RootMediaTypeTreeController : MediaTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootMediaTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="mediaTypeService">Service used for managing media types in the Umbraco CMS.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootMediaTypeTreeController(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService, mediaTypeService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public RootMediaTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMediaTypeService mediaTypeService)
        : base(entityService, flagProviders, mediaTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media type items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of media type items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<MediaTypeTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(skip, take);
    }
}
