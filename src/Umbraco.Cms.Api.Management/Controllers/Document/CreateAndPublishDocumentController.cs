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
public class CreateAndPublishDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IContentEditingService _contentEditingService;
    private readonly IContentPublishingService _contentPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateAndPublishDocumentController(
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

    [HttpPost("create-and-publish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAndPublish(
        CancellationToken cancellationToken,
        CreateAndPublishDocumentRequestModel requestModel)
    {
        // Authorize both create and publish permissions upfront.
        AuthorizationResult createAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionNew.ActionLetter, requestModel.Parent?.Id),
            AuthorizationPolicies.ContentPermissionByResource);

        AuthorizationResult publishAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionPublish.ActionLetter, requestModel.Parent?.Id, requestModel.Cultures.OfType<string>()),
            AuthorizationPolicies.ContentPermissionByResource);

        if (createAuthorizationResult.Succeeded is false || publishAuthorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        // Create the document.
        ContentCreateModel createModel = _documentEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<ContentCreateResult, ContentEditingOperationStatus> createResult =
            await _contentEditingService.CreateAsync(createModel, CurrentUserKey(_backOfficeSecurityAccessor));

        if (createResult.Success is false)
        {
            return ContentEditingOperationStatusResult(createResult.Status);
        }

        // If create had validation errors, don't attempt to publish - it will fail.
        if (createResult.Status == ContentEditingOperationStatus.PropertyValidationError)
        {
            return DocumentPublishingOperationStatusResult(
                ContentPublishingOperationStatus.ContentInvalid,
                invalidPropertyAliases: createResult.Result.ValidationResult.ValidationErrors.Select(e => e.Alias));
        }

        // Build immediate publish model (no schedule).
        IList<CulturePublishScheduleModel> culturePublishSchedules = GetImmediateCulturePublishSchedule(requestModel.Cultures);

        // Publish the document immediately using the already-loaded content.
        // Skip validation since create succeeded with no validation errors.
        Attempt<ContentPublishingResult, ContentPublishingOperationStatus> publishResult =
            await _contentPublishingService.PublishAsync(
                createResult.Result.Content!,
                culturePublishSchedules,
                CurrentUserKey(_backOfficeSecurityAccessor),
                skipValidation: true);

        if (publishResult.Success is false)
        {
            return DocumentPublishingOperationStatusResult(publishResult.Status, invalidPropertyAliases: publishResult.Result.InvalidPropertyAliases);
        }

        return CreatedAtId<ByKeyDocumentController>(controller => nameof(controller.ByKey), createResult.Result.Content!.Key);
    }
}
