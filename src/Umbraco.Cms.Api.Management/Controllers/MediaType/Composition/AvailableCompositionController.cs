using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Composition;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Composition;

[ApiVersion("1.0")]
public class AvailableCompositionController : MediaTypeControllerBase
{
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IMediaTypeEditingPresentationFactory _presentationFactory;

    public AvailableCompositionController(IMediaTypeEditingService mediaTypeEditingService, IMediaTypeEditingPresentationFactory presentationFactory)
    {
        _mediaTypeEditingService = mediaTypeEditingService;
        _presentationFactory = presentationFactory;
    }

    [HttpPost("available-compositions")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<AvailableMediaTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AvailableCompositions(MediaTypeCompositionRequestModel compositionModel)
    {
        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions = await _mediaTypeEditingService.GetAvailableCompositionsAsync(
            compositionModel.Id,
            compositionModel.CurrentCompositeIds,
            compositionModel.CurrentPropertyAliases);

        IEnumerable<AvailableMediaTypeCompositionResponseModel> responseModels = _presentationFactory.MapCompositionModels(availableCompositions);

        return Ok(responseModels);
    }
}
