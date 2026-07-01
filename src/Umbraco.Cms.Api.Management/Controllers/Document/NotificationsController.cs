using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides API endpoints for managing notifications related to documents.
/// </summary>
[ApiVersion("1.0")]
public class NotificationsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentNotificationPresentationFactory _documentNotificationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions for document notifications.</param>
    /// <param name="contentEditingService">Service responsible for editing and managing document content.</param>
    /// <param name="documentNotificationPresentationFactory">Factory for creating presentation models for document notifications.</param>
    public NotificationsController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentNotificationPresentationFactory documentNotificationPresentationFactory)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _documentNotificationPresentationFactory = documentNotificationPresentationFactory;
    }

    /// <summary>
    /// Retrieves notifications for the specified document.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document.</param>
    /// <returns>An <see cref="IActionResult"/> containing the notifications for the specified document, or a 404 response if not found.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/notifications")]
    [ProducesResponseType(typeof(IEnumerable<DocumentNotificationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets notifications for a document.")]
    [EndpointDescription("Gets the notifications for the document identified by the provided Id.")]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IContent? content = await _contentEditingService.GetAsync(id);
        return content != null
            ? Ok(await _documentNotificationPresentationFactory.CreateNotificationModelsAsync(content))
            : DocumentNotFound();
    }
}
