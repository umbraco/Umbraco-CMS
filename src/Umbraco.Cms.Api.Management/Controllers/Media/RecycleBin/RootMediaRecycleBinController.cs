using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

/// <summary>
/// Provides API endpoints for managing items in the root of the media recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class RootMediaRecycleBinController : MediaRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootMediaRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing entities.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    public RootMediaRecycleBinController(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, mediaPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of media items located at the root of the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A <see cref="PagedViewModel{MediaRecycleBinItemResponseModel}"/> containing the media items in the recycle bin root.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets media at the root of the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of media items at the root level of the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<MediaRecycleBinItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
