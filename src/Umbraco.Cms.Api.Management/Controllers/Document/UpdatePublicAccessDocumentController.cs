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
/// Controller responsible for managing and updating the public access settings of documents in the CMS.
/// </summary>
[ApiVersion("1.0")]
public class UpdatePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;
    private readonly IPublicAccessService _publicAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePublicAccessDocumentController"/> class, which manages updates to public access settings for documents.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to controller actions.</param>
    /// <param name="publicAccessPresentationFactory">Factory for creating public access presentation models.</param>
    /// <param name="publicAccessService">Service for managing public access rules and settings.</param>
    public UpdatePublicAccessDocumentController(
        IAuthorizationService authorizationService,
        IPublicAccessPresentationFactory publicAccessPresentationFactory,
        IPublicAccessService publicAccessService)
    {
        _authorizationService = authorizationService;
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
        _publicAccessService = publicAccessService;
    }

    /// <summary>
    /// Updates the public access (member protection) settings for a document, controlling which members or member groups can access it.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document whose public access settings are to be updated.</param>
    /// <param name="requestModel">The model containing the new public access settings to apply to the document.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the update operation.
    /// Returns <c>200 OK</c> if the update is successful, or <c>404 Not Found</c> if the document does not exist.
    /// </returns>
    [HttpPut("{id:guid}/public-access")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates public access protection for a document.")]
    [EndpointDescription("Updates the member protection settings for a document, controlling which members or member groups can access it.")]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, PublicAccessRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionProtect.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        PublicAccessEntrySlim publicAccessEntrySlim = _publicAccessPresentationFactory.CreatePublicAccessEntrySlim(requestModel, id);

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> updateAttempt = await _publicAccessService.UpdateAsync(publicAccessEntrySlim);

        return updateAttempt.Success
            ? Ok()
            : PublicAccessOperationStatusResult(updateAttempt.Status);
    }
}
