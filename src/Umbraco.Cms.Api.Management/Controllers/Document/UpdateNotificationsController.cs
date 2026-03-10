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

[ApiVersion("1.0")]
public class UpdateNotificationsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly INotificationService _notificationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    [ActivatorUtilitiesConstructor]
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
