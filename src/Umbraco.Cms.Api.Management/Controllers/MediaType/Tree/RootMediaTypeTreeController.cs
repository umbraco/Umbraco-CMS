using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="entitySearchService">Service for searching entities.</param>
    /// <param name="idKeyMap">Maps between integer identifiers and keys.</param>
    /// <param name="mediaTypeService">Service for managing media types.</param>
    public RootMediaTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IEntitySearchService entitySearchService, IIdKeyMap idKeyMap, IMediaTypeService mediaTypeService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap, mediaTypeService)
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
