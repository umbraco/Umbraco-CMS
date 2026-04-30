using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

/// <summary>
/// Controller responsible for managing tree operations involving the children of media types in the Umbraco CMS.
/// Provides endpoints for retrieving and interacting with child media type nodes within the media type tree structure.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenMediaTypeTreeController : MediaTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenMediaTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used to manage and retrieve entities within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities in the tree.</param>
    /// <param name="mediaTypeService">Service used to manage media types in the Umbraco CMS.</param>
    ///
    public ChildrenMediaTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMediaTypeService mediaTypeService)
        : base(entityService, flagProviders, mediaTypeService)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of media type tree items that are children of the specified parent media type.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent media type.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="foldersOnly">If <c>true</c>, only folder items are returned; otherwise, all child items are included.</param>
    /// <returns>A paged view model containing the child media type tree items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media type tree child items.")]
    [EndpointDescription("Gets a paginated collection of media type tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<MediaTypeTreeItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
