using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

/// <summary>
/// Provides API endpoints for managing allowed media type items within the media type item context.
/// </summary>
[ApiVersion("1.0")]
public class AllowedMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedMediaTypeItemController"/> class.
    /// </summary>
    /// <param name="mediaTypeEditingService">Service for editing media types.</param>
    /// <param name="mapper">The Umbraco object mapper.</param>
    public AllowedMediaTypeItemController(IMediaTypeEditingService mediaTypeEditingService, IUmbracoMapper mapper)
    {
        _mediaTypeEditingService = mediaTypeEditingService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paged collection of allowed media type items for a given file extension.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="fileExtension">The file extension used to filter allowed media types.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedModel{MediaTypeItemResponseModel}"/> representing the allowed media types.</returns>
    [HttpGet("allowed")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media type items.")]
    [EndpointDescription("Gets a collection of allowed media type items for the specified file extension.")]
    public async Task<IActionResult> Item(CancellationToken cancellationToken, string fileExtension, int skip = 0, int take = 100)
    {
        PagedModel<IMediaType> mediaTypes = await _mediaTypeEditingService.GetMediaTypesForFileExtensionAsync(fileExtension, skip, take);

        var result = new PagedModel<MediaTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes.Items),
            Total = mediaTypes.Total
        };
        return Ok(result);
    }
}
