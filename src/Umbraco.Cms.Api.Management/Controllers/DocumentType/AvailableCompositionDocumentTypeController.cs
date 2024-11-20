using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class AvailableCompositionDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeEditingService _contentTypeEditingService;
    private readonly IDocumentTypeEditingPresentationFactory _presentationFactory;

    public AvailableCompositionDocumentTypeController(IContentTypeEditingService contentTypeEditingService, IDocumentTypeEditingPresentationFactory presentationFactory)
    {
        _contentTypeEditingService = contentTypeEditingService;
        _presentationFactory = presentationFactory;
    }

    [HttpPost("available-compositions")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<AvailableDocumentTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AvailableCompositions(
        CancellationToken cancellationToken,
        DocumentTypeCompositionRequestModel compositionModel)
    {
        IEnumerable<ContentTypeAvailableCompositionsResult> availableCompositions = await _contentTypeEditingService.GetAvailableCompositionsAsync(
            compositionModel.Id,
            compositionModel.CurrentCompositeIds,
            compositionModel.CurrentPropertyAliases,
            compositionModel.IsElement);

        IEnumerable<AvailableDocumentTypeCompositionResponseModel> responseModels = _presentationFactory.MapCompositionModels(availableCompositions);

        return Ok(responseModels);
    }
}
