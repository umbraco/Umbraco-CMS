using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

    /// <summary>
    /// Provides API endpoints for managing media type items within folders.
    /// </summary>
[ApiVersion("1.0")]
public class FolderMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderMediaTypeItemController"/> class, which manages folder media type items in the Umbraco CMS.
    /// </summary>
    /// <param name="mediaTypeEditingService">An instance of <see cref="IMediaTypeEditingService"/> used to edit media types.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between models.</param>
    public FolderMediaTypeItemController(IMediaTypeEditingService mediaTypeEditingService, IUmbracoMapper mapper)
    {
        _mediaTypeEditingService = mediaTypeEditingService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paginated list of media type folder items.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged collection of <see cref="MediaTypeItemResponseModel"/> representing media type folders.</returns>
    [HttpGet("folders")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media type folder items.")]
    [EndpointDescription("Gets a paginated collection of media type folder items.")]
    public async Task<IActionResult> Folders(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<IMediaType> mediaTypes = await _mediaTypeEditingService.GetFolderMediaTypes(skip, take);

        var result = new PagedModel<MediaTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes.Items),
            Total = mediaTypes.Total
        };
        return Ok(result);
    }
}
