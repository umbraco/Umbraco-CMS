using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// API controller responsible for handling operations related to the creation of content documents in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class CreateAndPublishDocumentController : CreateDocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAndPublishDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to document creation and publishing operations.</param>
    /// <param name="documentEditingPresentationFactory">Factory for creating document editing presentation models.</param>
    /// <param name="contentEditingService">Service responsible for content editing functionality.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CreateAndPublishDocumentController(
        IAuthorizationService authorizationService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _authorizationService = authorizationService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new document using the specified request model, and subsequently publishes the document in the cultures provided.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="requestModel">The details of the document to create.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPost("create-and-publish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates and publishes a new document.")]
    [EndpointDescription("Creates and publishes a new document with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateAndPublishDocumentRequestModel requestModel)
        => await HandleRequest(requestModel, async () =>
        {
            // The base HandleRequest verifies the user can create under the parent.
            // Creating-and-publishing additionally requires publish permission, so we check that here.
            AuthorizationResult publishAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
                User,
                ContentPermissionResource.WithKeys(ActionPublish.ActionLetter, requestModel.Parent?.Id, requestModel.CulturesToPublish),
                AuthorizationPolicies.ContentPermissionByResource);

            if (publishAuthorizationResult.Succeeded is false)
            {
                return Forbidden();
            }

            ContentCreateModel model = _documentEditingPresentationFactory.MapCreateModel(requestModel);
            Attempt<ContentCreateResult, ContentEditingOperationStatus> result =
                await _contentEditingService.CreateAndPublishAsync(model, requestModel.CulturesToPublish, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? CreatedAtId<ByKeyDocumentController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
                : ContentEditingOperationStatusResult(result.Status);
        });
}
