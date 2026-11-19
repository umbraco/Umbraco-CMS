using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UrlSegment;

/// <summary>
/// Controller for handling image resizing operations for URL segments.
/// </summary>
[ApiVersion("1.0")]
public class ResizeImagingController : ImagingControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IResizeImageUrlFactory _resizeImageUrlFactory;
    private readonly IMediaPermissionService _mediaPermissionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeImagingController"/> class.
    /// </summary>
    /// <param name="mediaService">The media service used to perform media-related operations.</param>
    /// <param name="resizeImageUrlFactory">The factory responsible for generating URLs for resized images.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public ResizeImagingController(IMediaService mediaService, IResizeImageUrlFactory resizeImageUrlFactory)
        : this(
            mediaService,
            resizeImageUrlFactory,
            StaticServiceProvider.Instance.GetRequiredService<IMediaPermissionService>(),
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSecurityAccessor>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeImagingController"/> class.
    /// </summary>
    /// <param name="mediaService">The media service used to perform media-related operations.</param>
    /// <param name="resizeImageUrlFactory">The factory responsible for generating URLs for resized images.</param>
    /// <param name="mediaPermissionService">Service for determining the media the current user can access.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the current back office user.</param>
    [ActivatorUtilitiesConstructor]
    public ResizeImagingController(
        IMediaService mediaService,
        IResizeImageUrlFactory resizeImageUrlFactory,
        IMediaPermissionService mediaPermissionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaService = mediaService;
        _resizeImageUrlFactory = resizeImageUrlFactory;
        _mediaPermissionService = mediaPermissionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
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
    [NonAction]
    [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 19.")]
    public Task<IActionResult> Urls(HashSet<Guid> ids, int height = 200, int width = 200, ImageCropMode? mode = null, string? format = null)
        => Urls(CancellationToken.None, ids, null, height, width, mode, format);

    /// <summary>
    /// Retrieves a collection of URLs for resized versions of the specified images the current user has access to, using the provided dimensions and options.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of unique identifiers for the images to be resized.</param>
    /// <param name="dataTypeId">
    /// The optional identifier of the data type the request originates from. Media outside the user's start nodes
    /// is only included when this data type is configured to ignore user start nodes.
    /// </param>
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
    public async Task<IActionResult> Urls(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids,
        [FromQuery] Guid? dataTypeId,
        int height = 200,
        int width = 200,
        ImageCropMode? mode = null,
        string? format = null)
    {
        ISet<Guid> authorizedIds = await _mediaPermissionService.FilterAuthorizedAccessAsync(
            CurrentUser(_backOfficeSecurityAccessor),
            ids,
            dataTypeId);
        IEnumerable<IMedia> items = _mediaService.GetByIds(authorizedIds);
        var options = new ImageResizeOptions(height, width, mode, format);

        return Ok(_resizeImageUrlFactory.CreateUrlSets(items, options));
    }
}
