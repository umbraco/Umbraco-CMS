using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// API controller responsible for handling requests to create documents with public access permissions in Umbraco.
/// </summary>
[ApiVersion("1.0")]
public class CreatePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;
    private readonly IPublicAccessService _publicAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePublicAccessDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to perform permission checks.</param>
    /// <param name="publicAccessPresentationFactory">Factory for creating public access presentation models.</param>
    /// <param name="publicAccessService">Service for managing public access settings.</param>
    public CreatePublicAccessDocumentController(
        IAuthorizationService authorizationService,
        IPublicAccessPresentationFactory publicAccessPresentationFactory,
        IPublicAccessService publicAccessService)
    {
        _authorizationService = authorizationService;
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
        _publicAccessService = publicAccessService;
    }

    /// <summary>
    /// Creates public access rules for the specified document, restricting or allowing access based on the provided access details.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document to protect.</param>
    /// <param name="publicAccessRequestModel">The model containing the public access configuration for the document.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the public access rules were successfully created.</description></item>
    /// <item><description><c>404 Not Found</c> if the document does not exist.</description></item>
    /// </list>
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/public-access")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates public access rules for a document.")]
    [EndpointDescription("Creates public access protection for the document identified by the provided Id.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        Guid id,
        PublicAccessRequestModel publicAccessRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionProtect.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        PublicAccessEntrySlim publicAccessEntrySlim = _publicAccessPresentationFactory.CreatePublicAccessEntrySlim(publicAccessRequestModel, id);

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> saveAttempt = await _publicAccessService.CreateAsync(publicAccessEntrySlim);

        return saveAttempt.Success
            ? CreatedAtId<GetPublicAccessDocumentController>(controller => nameof(controller.GetPublicAccess), id)
            : PublicAccessOperationStatusResult(saveAttempt.Status);
    }
}
