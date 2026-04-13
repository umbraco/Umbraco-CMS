using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

/// <summary>
/// API controller for managing and retrieving URLs for media items in the Umbraco CMS.
/// Handles operations related to generating, resolving, and returning media URLs.
/// </summary>
[ApiVersion("1.0")]
public class MediaUrlController : MediaControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IMediaUrlFactory _mediaUrlFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaUrlController"/> class.
    /// </summary>
    /// <param name="mediaService">Service for managing media items.</param>
    /// <param name="mediaUrlFactory">Factory for generating URLs for media items.</param>
    public MediaUrlController(
        IMediaService mediaService,
        IMediaUrlFactory mediaUrlFactory)
    {
        _mediaService = mediaService;
        _mediaUrlFactory = mediaUrlFactory;
    }

    /// <summary>
    /// Retrieves the URLs for the specified media items.
    /// </summary>
    /// <param name="ids">A set of unique identifiers (GUIDs) representing the media items to retrieve URLs for. These are provided as query parameters.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is an <see cref="IActionResult"/> containing a collection of <see cref="MediaUrlInfoResponseModel"/> objects with the URLs for each requested media item.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("urls")]
    [ProducesResponseType(typeof(IEnumerable<MediaUrlInfoResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets URLs for media items.")]
    [EndpointDescription("Gets the URLs for the media items identified by the provided Ids.")]
    public Task<IActionResult> GetUrls([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IMedia> items = _mediaService.GetByIds(ids);

        return Task.FromResult<IActionResult>(Ok(_mediaUrlFactory.CreateUrlSets(items)));
    }
}
