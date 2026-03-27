using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

/// <summary>
/// Controller responsible for managing child media items within the media recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenMediaRecycleBinController : MediaRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenMediaRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for performing operations on entities.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    public ChildrenMediaRecycleBinController(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, mediaPresentationFactory)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media items in the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of media items that are children of the provided parent in the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<MediaRecycleBinItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentId, skip, take);
}
