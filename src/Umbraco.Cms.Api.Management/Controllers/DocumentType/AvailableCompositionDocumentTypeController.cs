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

/// <summary>
/// Controller responsible for retrieving and managing document types that are available for composition in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class AvailableCompositionDocumentTypeController : DocumentTypeControllerBase
{
    private readonly IContentTypeEditingService _contentTypeEditingService;
    private readonly IDocumentTypeEditingPresentationFactory _presentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableCompositionDocumentTypeController"/> class.
    /// </summary>
    /// <param name="contentTypeEditingService">Service for editing content types.</param>
    /// <param name="presentationFactory">Factory for creating document type editing presentations.</param>
    public AvailableCompositionDocumentTypeController(IContentTypeEditingService contentTypeEditingService, IDocumentTypeEditingPresentationFactory presentationFactory)
    {
        _contentTypeEditingService = contentTypeEditingService;
        _presentationFactory = presentationFactory;
    }

    /// <summary>
    /// Retrieves a list of document types that can be assigned as compositions to the specified document type.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="compositionModel">The request model containing the target document type's ID and its current composition and property details.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of available document type compositions as response models.</returns>
    [HttpPost("available-compositions")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<AvailableDocumentTypeCompositionResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets available compositions.")]
    [EndpointDescription("Gets a collection of document types that are available to use as compositions for the specified document type.")]
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
