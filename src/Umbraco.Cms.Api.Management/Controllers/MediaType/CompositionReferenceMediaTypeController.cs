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
    /// Provides API endpoints for managing media types that are used as composition references in the system.
    /// </summary>
[ApiVersion("1.0")]
public class CompositionReferenceMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositionReferenceMediaTypeController"/> class, which manages media type composition references in the Umbraco CMS API.
    /// </summary>
    /// <param name="mediaTypeService">Service used for operations related to media types.</param>
    /// <param name="umbracoMapper">The mapper used to map between Umbraco domain models and API models.</param>
    public CompositionReferenceMediaTypeController(IMediaTypeService mediaTypeService, IUmbracoMapper umbracoMapper)
    {
        _mediaTypeService = mediaTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/composition-references")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets composition references.")]
    [EndpointDescription("Gets a collection of media types that reference the specified media type as a composition.")]
    public async Task<IActionResult> CompositionReferences(CancellationToken cancellationToken, Guid id)
    {
        var mediaType = await _mediaTypeService.GetAsync(id);

        if (mediaType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        IEnumerable<IMediaType> composedOf = _mediaTypeService.GetComposedOf(mediaType.Id);
        List<MediaTypeCompositionResponseModel> responseModels = _umbracoMapper.MapEnumerable<IMediaType, MediaTypeCompositionResponseModel>(composedOf);

        return Ok(responseModels);
    }
}
