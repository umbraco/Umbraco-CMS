using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller for managing document update notifications.
/// </summary>
[ApiVersion("1.0")]
public class UpdateNotificationsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly INotificationService _notificationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    [ActivatorUtilitiesConstructor]
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateNotificationsController"/> class, which manages update notifications for documents.
    /// </summary>
    /// <param name="contentEditingService">Service used for editing content.</param>
    /// <param name="notificationService">Service responsible for handling notifications.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    public UpdateNotificationsController(IAuthorizationService authorizationService, IContentEditingService contentEditingService, INotificationService notificationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _notificationService = notificationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public UpdateNotificationsController(IContentEditingService contentEditingService, INotificationService notificationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>(),
            contentEditingService,
            notificationService,
            backOfficeSecurityAccessor)
    {
    }

    /// <summary>
    /// Updates the notification subscriptions for the current user on the specified document.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document for which to update notification subscriptions.</param>
    /// <param name="updateModel">The request model containing the list of action IDs to subscribe to notifications for.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <c>200 OK</c> if successful, or <c>404 Not Found</c> if the document does not exist.</returns>
    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates notification subscriptions for a document.")]
    [EndpointDescription("Updates which actions the current user is subscribed to receive notifications for on the specified document.")]
    public async Task<IActionResult> UpdateNotifications(CancellationToken cancellationToken, Guid id, UpdateDocumentNotificationsRequestModel updateModel)
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
        if (content == null)
        {
            return DocumentNotFound();
        }

        _notificationService.SetNotifications(_backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser, content, updateModel.SubscribedActionIds);
        return Ok();
    }
}
