using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller responsible for handling update-and-publish operations on documents in the management API.
/// </summary>
[ApiVersion("1.0")]
public class UpdateAndPublishDocumentController : UpdateDocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAndPublishDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service for verifying user permissions.</param>
    /// <param name="contentEditingService">Service for managing content updates.</param>
    /// <param name="documentEditingPresentationFactory">Factory for creating document editing presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office user security context.</param>
    public UpdateAndPublishDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _contentEditingService = contentEditingService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>Updates the specified document with new details provided in the request model, and subsequently publishes the document in the cultures provided.</summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document to update.</param>
    /// <param name="requestModel">The model containing the updated document details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the update operation.</returns>
    [HttpPut("{id:guid}/update-and-publish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates and publishes a document.")]
    [EndpointDescription("Updates and publishes a document identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateAndPublishDocumentRequestModel requestModel)
        => await HandleRequest(id, requestModel, async () =>
        {
            ContentUpdateModel model = _documentEditingPresentationFactory.MapUpdateModel(requestModel);
            Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);
            Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await _contentEditingService.UpdateAndPublishAsync(id, model, requestModel.CulturesToPublish, currentUserKey);

            return result.Success
                ? Ok()
                : ContentEditingOperationStatusResult(result.Status);
        });
}
