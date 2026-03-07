using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

    /// <summary>
    /// Provides API endpoints for managing media types using their unique key identifier.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyMediaTypeController"/> class, which handles operations for media types identified by key.
    /// </summary>
    /// <param name="mediaTypeService">Service used to manage media types.</param>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    public ByKeyMediaTypeController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a media type by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the media type.</param>
    /// <returns>An <see cref="IActionResult"/> containing the <see cref="MediaTypeResponseModel"/> if found; otherwise, a not found result.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(MediaTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a media type.")]
    [EndpointDescription("Gets a media type identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IMediaType? mediaType = await _mediaTypeService.GetAsync(id);
        if (mediaType == null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        MediaTypeResponseModel model = _umbracoMapper.Map<MediaTypeResponseModel>(mediaType)!;
        return Ok(model);
    }
}
