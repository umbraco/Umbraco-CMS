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
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

/// <summary>
/// Controller for updating document blueprints.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class UpdateDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IDocumentBlueprintEditingPresentationFactory _blueprintEditingPresentationFactory;
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.UpdateDocumentBlueprintController"/> class.
    /// </summary>
    /// <param name="blueprintEditingPresentationFactory">Factory used to create blueprint editing presentations.</param>
    /// <param name="contentBlueprintEditingService">Service responsible for content blueprint editing operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office security context.</param>
    public UpdateDocumentBlueprintController(
        IDocumentBlueprintEditingPresentationFactory blueprintEditingPresentationFactory,
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _blueprintEditingPresentationFactory = blueprintEditingPresentationFactory;
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Updates the specified document blueprint with new details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document blueprint to update.</param>
    /// <param name="requestModel">The model containing updated details for the document blueprint.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a document blueprint.")]
    [EndpointDescription("Updates a document blueprint identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, UpdateDocumentBlueprintRequestModel requestModel)
    {
        ContentBlueprintUpdateModel model = _blueprintEditingPresentationFactory.MapUpdateModel(requestModel);

        // We don't need to validate user access because we "only" require access to the Settings section to update blueprints
        Attempt<ContentUpdateResult, ContentEditingOperationStatus> result = await _contentBlueprintEditingService.UpdateAsync(id, model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
