using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

/// <summary>
/// Provides API endpoints for managing individual media type items in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MediaType.Item.ItemMediaTypeItemController"/> class, which manages media type items in the Umbraco CMS API.
    /// </summary>
    /// <param name="mediaTypeService">The service used to manage media types.</param>
    /// <param name="mapper">The mapper used to convert between domain and API models.</param>
    public ItemMediaTypeItemController(IMediaTypeService mediaTypeService, IUmbracoMapper mapper)
    {
        _mediaTypeService = mediaTypeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a collection of media type items corresponding to the specified IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of unique identifiers for the media type items to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="MediaTypeItemResponseModel"/> objects.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media type items.")]
    [EndpointDescription("Gets a collection of media type items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<MediaTypeItemResponseModel>()));
        }

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(ids);
        List<MediaTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(mediaTypes);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
