using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

/// <summary>
/// Provides API endpoints for creating document blueprints based on existing documents.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public class CreateDocumentBlueprintFromDocumentController : DocumentBlueprintControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentBlueprintEditingService _contentBlueprintEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentBlueprintFromDocumentController"/> class with the specified dependencies.
    /// </summary>
    /// <param name="authorizationService">The service used to authorize access to controller actions.</param>
    /// <param name="contentBlueprintEditingService">The service responsible for editing content blueprints.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information.</param>
    public CreateDocumentBlueprintFromDocumentController(
        IAuthorizationService authorizationService,
        IContentBlueprintEditingService contentBlueprintEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentBlueprintEditingService = contentBlueprintEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    ///     Creates a blueprint from a content item.
    /// </summary>
    [HttpPost("from-document")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a document blueprint from an existing document.")]
    [EndpointDescription("Creates a new document blueprint based on an existing document identified by the provided Id.")]
    public async Task<IActionResult> CreateFromDocument(CancellationToken cancellationToken, CreateDocumentBlueprintFromDocumentRequestModel fromDocumentRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionCreateBlueprintFromContent.ActionLetter, fromDocumentRequestModel.Document.Id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentCreateResult, ContentEditingOperationStatus> result =
            await _contentBlueprintEditingService.CreateFromContentAsync(
                fromDocumentRequestModel.Document.Id,
                fromDocumentRequestModel.Name,
                fromDocumentRequestModel.Id,
                CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDocumentBlueprintController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
