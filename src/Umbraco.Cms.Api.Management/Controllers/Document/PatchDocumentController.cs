using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.OperationStatus;
using Umbraco.Cms.Api.Management.Patchers;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class PatchDocumentController : PatchDocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly DocumentPatcher _documentPatcher;
    private readonly IDocumentEditingPresentationFactory _presentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public PatchDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        DocumentPatcher documentPatcher,
        IDocumentEditingPresentationFactory presentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _contentEditingService = contentEditingService;
        _documentPatcher = documentPatcher;
        _presentationFactory = presentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPatch("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [Consumes("application/json-patch+json")]
    public async Task<IActionResult> Patch(
        CancellationToken cancellationToken,
        Guid id,
        PatchDocumentRequestModel requestModel)
        => await HandleRequest(id, requestModel, async () =>
        {
            // Map request model to domain model
            ContentPatchModel patchModel = _presentationFactory.MapPatchModel(requestModel);

            // Apply PATCH operations to create an update model
            Attempt<ContentUpdateModel, ContentPatchingOperationStatus> patchResult =
                await _documentPatcher.ApplyPatchAsync(id, patchModel, CurrentUserKey(_backOfficeSecurityAccessor));

            if (!patchResult.Success)
            {
                return ContentPatchingOperationStatusResult(patchResult.Status);
            }

            // Use the standard update method to save the patched content
            Attempt<ContentUpdateResult, ContentEditingOperationStatus> updateResult =
                await _contentEditingService.UpdateAsync(id, patchResult.Result!, CurrentUserKey(_backOfficeSecurityAccessor));

            return updateResult.Success
                ? Ok()
                : ContentEditingOperationStatusResult(updateResult.Status);
        });
}
