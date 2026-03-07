using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

    /// <summary>
    /// API controller responsible for handling operations related to the siblings of a media type in the media type tree.
    /// </summary>
public class SiblingsMediaTypeTreeController : MediaTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsMediaTypeTreeController"/> class, which manages API endpoints for retrieving sibling media types in the tree structure.
    /// </summary>
    /// <param name="entityService">The service used for entity operations.</param>
    /// <param name="mediaTypeService">The service used for media type operations.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsMediaTypeTreeController(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService, mediaTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsMediaTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="mediaTypeService">Service used for managing media types.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsMediaTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMediaTypeService mediaTypeService)
        : base(entityService, flagProviders, mediaTypeService)
    {
    }

    /// <summary>
    /// Retrieves sibling media type tree items for the specified media type ID.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The ID of the media type whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target item.</param>
    /// <param name="after">The number of sibling items to include after the target item.</param>
    /// <param name="foldersOnly">If set to <c>true</c>, only folder items are included in the results.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="MediaTypeTreeItemResponseModel"/> representing the sibling items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media type tree sibling items.")]
    [EndpointDescription("Gets a collection of media type tree items that are siblings of the provided Id.")]
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
