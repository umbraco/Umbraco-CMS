using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// Provides API endpoints for retrieving and managing available composition media types in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class AvailableCompositionMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IMediaTypeEditingPresentationFactory _presentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableCompositionMediaTypeController"/> class.
    /// </summary>
    /// <param name="mediaTypeEditingService">Service for editing media types.</param>
    /// <param name="presentationFactory">Factory for creating media type editing presentations.</param>
    public AvailableCompositionMediaTypeController(IMediaTypeEditingService mediaTypeEditingService, IMediaTypeEditingPresentationFactory presentationFactory)
    {
        _mediaTypeEditingService = mediaTypeEditingService;
        _presentationFactory = presentationFactory;
    }

    /// <summary>
    /// Retrieves a collection of media types that can be assigned as compositions to the specified media type.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="compositionModel">A <see cref="MediaTypeCompositionRequestModel"/> containing the target media type ID and its current composition and property details.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> containing a collection of <see cref="AvailableMediaTypeCompositionResponseModel"/> representing the available media type compositions.</returns>
    [HttpPost("available-compositions")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<AvailableMediaTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets available compositions.")]
    [EndpointDescription("Gets a collection of media types that are available to use as compositions for the specified media type.")]
    public async Task<IActionResult> AvailableCompositions(
        CancellationToken cancellationToken,
        MediaTypeCompositionRequestModel compositionModel)
    {
        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions = await _mediaTypeEditingService.GetAvailableCompositionsAsync(
            compositionModel.Id,
            compositionModel.CurrentCompositeIds,
            compositionModel.CurrentPropertyAliases);

        IEnumerable<AvailableMediaTypeCompositionResponseModel> responseModels = _presentationFactory.MapCompositionModels(availableCompositions);

        return Ok(responseModels);
    }
}
