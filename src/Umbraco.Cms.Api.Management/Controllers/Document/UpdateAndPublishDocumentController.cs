using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class UpdateAndPublishDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IContentEditingService _contentEditingService;
    private readonly IContentPublishingService _contentPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateAndPublishDocumentController(
        IAuthorizationService authorizationService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IContentEditingService contentEditingService,
        IContentPublishingService contentPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _contentEditingService = contentEditingService;
        _contentPublishingService = contentPublishingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/update-and-publish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAndPublish(
        CancellationToken cancellationToken,
        Guid id,
        UpdateAndPublishDocumentRequestModel requestModel)
    {
        // Authorize both update and publish permissions upfront.
        AuthorizationResult updateAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionUpdate.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        AuthorizationResult publishAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionPublish.ActionLetter, id, requestModel.Cultures.OfType<string>()),
            AuthorizationPolicies.ContentPermissionByResource);

        if (updateAuthorizationResult.Succeeded is false || publishAuthorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        // Update the document.
        ContentUpdateModel updateModel = _documentEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<ContentUpdateResult, ContentEditingOperationStatus> updateResult =
            await _contentEditingService.UpdateAsync(id, updateModel, CurrentUserKey(_backOfficeSecurityAccessor));

        if (updateResult.Success is false)
        {
            return ContentEditingOperationStatusResult(updateResult.Status);
        }

        // If update had validation errors, don't attempt to publish - it will fail.
        if (updateResult.Status == ContentEditingOperationStatus.PropertyValidationError)
        {
            return DocumentPublishingOperationStatusResult(
                ContentPublishingOperationStatus.ContentInvalid,
                invalidPropertyAliases: updateResult.Result.ValidationResult.ValidationErrors.Select(e => e.Alias));
        }

        // Build immediate publish model (no schedule).
        IList<CulturePublishScheduleModel> culturePublishSchedules = GetImmediateCulturePublishSchedule(requestModel.Cultures);

        // Publish the document immediately using the already-loaded content.
        // Skip validation since update succeeded with no validation errors.
        Attempt<ContentPublishingResult, ContentPublishingOperationStatus> publishResult =
            await _contentPublishingService.PublishAsync(
                updateResult.Result.Content!,
                culturePublishSchedules,
                CurrentUserKey(_backOfficeSecurityAccessor),
                skipValidation: true);

        if (publishResult.Success is false)
        {
            return DocumentPublishingOperationStatusResult(publishResult.Status, invalidPropertyAliases: publishResult.Result.InvalidPropertyAliases);
        }

        return Ok();
    }
}
