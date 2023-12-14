using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Composition;

[ApiVersion("1.0")]
public class AvailableCompositionController : DocumentTypeControllerBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IDocumentTypeEditingPresentationFactory _presentationFactory;

    public AvailableCompositionController(IContentTypeService contentTypeService, IDocumentTypeEditingPresentationFactory presentationFactory)
    {
        _contentTypeService = contentTypeService;
        _presentationFactory = presentationFactory;
    }

    [HttpPost("available-compositions")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<AvailableDocumentTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AvailableCompositions(DocumentTypeCompositionRequestModel compositionModel)
    {
        var contentType = await _contentTypeService.GetAsync(compositionModel.Id); // NB: different for media/member (media/member service)

        IContentTypeComposition[] allContentTypes = _contentTypeService.GetAll().Cast<IContentTypeComposition>().ToArray(); // NB: different for media/member (media/member service)
        var currentCompositionAliases = compositionModel.CompositeIds.Any()
            ? _contentTypeService.GetAll(compositionModel.CompositeIds).Select(x => x.Alias).ToArray()
            : Array.Empty<string>();

        ContentTypeAvailableCompositionsResults availableCompositions = _contentTypeService.GetAvailableCompositeContentTypes(
            contentType,
            allContentTypes,
            currentCompositionAliases,
            compositionModel.CurrentPropertyAliases.ToArray(),
            compositionModel.IsElement);

        var responseModels = _presentationFactory.CreateCompositionModels(availableCompositions.Results);

        return Ok(responseModels);
    }
}
