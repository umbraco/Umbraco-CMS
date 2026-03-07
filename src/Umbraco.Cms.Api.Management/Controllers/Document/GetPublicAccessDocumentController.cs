using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
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
/// Controller responsible for handling API requests to retrieve documents with public access settings.
/// </summary>
[ApiVersion("1.0")]
public class GetPublicAccessDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPublicAccessDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to resources.</param>
    /// <param name="publicAccessService">Service for managing public access rules for documents.</param>
    /// <param name="publicAccessPresentationFactory">Factory for creating presentation models for public access data.</param>
    public GetPublicAccessDocumentController(
        IAuthorizationService authorizationService,
        IPublicAccessService publicAccessService,
        IPublicAccessPresentationFactory publicAccessPresentationFactory)
    {
        _authorizationService = authorizationService;
        _publicAccessService = publicAccessService;
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
    }

    /// <summary>
    /// Retrieves the public access protection settings for the specified document.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document whose public access settings are to be retrieved.</param>
    /// <returns>An <see cref="IActionResult"/> containing the public access settings if found; otherwise, an error result.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/public-access")]
    [ProducesResponseType(typeof(PublicAccessResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets public access rules for a document.")]
    [EndpointDescription("Gets the public access protection settings for the document identified by the provided Id.")]
    public async Task<IActionResult> GetPublicAccess(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionProtect.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> accessAttempt =
            await _publicAccessService.GetEntryByContentKeyWithoutAncestorsAsync(id);

        if (accessAttempt.Success is false || accessAttempt.Result is null)
        {
            return PublicAccessOperationStatusResult(accessAttempt.Status);
        }

        Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> responseModelAttempt =
            _publicAccessPresentationFactory.CreatePublicAccessResponseModel(accessAttempt.Result);

        if (responseModelAttempt.Success is false)
        {
            return PublicAccessOperationStatusResult(responseModelAttempt.Status);
        }

        return Ok(responseModelAttempt.Result);
    }
}
