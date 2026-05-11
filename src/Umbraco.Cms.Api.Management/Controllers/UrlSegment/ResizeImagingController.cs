using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UrlSegment;

/// <summary>
/// Controller for handling image resizing operations for URL segments.
/// </summary>
[ApiVersion("1.0")]
public class ResizeImagingController : ImagingControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IReziseImageUrlFactory _reziseImageUrlFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeImagingController"/> class.
    /// </summary>
    /// <param name="mediaService">The media service used to perform media-related operations.</param>
    /// <param name="reziseImageUrlFactory">The factory responsible for generating URLs for resized images.</param>
    public ResizeImagingController(IMediaService mediaService, IReziseImageUrlFactory reziseImageUrlFactory)
    {
        _mediaService = mediaService;
        _reziseImageUrlFactory = reziseImageUrlFactory;
    }

    /// <summary>
    /// Retrieves a collection of URLs for resized versions of the specified images, using the provided dimensions and options.
    /// </summary>
    /// <param name="ids">A set of unique identifiers for the images to be resized.</param>
    /// <param name="height">The target height, in pixels, for the resized images. Defaults to 200.</param>
    /// <param name="width">The target width, in pixels, for the resized images. Defaults to 200.</param>
    /// <param name="mode">The cropping mode to apply when resizing the images. Optional.</param>
    /// <param name="format">The desired output image format (e.g., "jpg", "png"). Optional.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with a collection of <see cref="MediaUrlInfoResponseModel"/> objects, each representing a resized image URL.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("resize/urls")]
    [ProducesResponseType(typeof(IEnumerable<MediaUrlInfoResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets URLs for image resizing.")]
    [EndpointDescription("Gets a collection of URLs for resizing images with the provided dimensions and options.")]
    public Task<IActionResult> Urls([FromQuery(Name = "id")] HashSet<Guid> ids, int height = 200, int width = 200, ImageCropMode? mode = null, string? format = null)
    {
        IEnumerable<IMedia> items = _mediaService.GetByIds(ids);
        var options = new ImageResizeOptions(height, width, mode, format);

        return Task.FromResult<IActionResult>(Ok(_reziseImageUrlFactory.CreateUrlSets(items, options)));
    }
}
