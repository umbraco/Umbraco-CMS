using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

/// <summary>
/// Serves as the base controller for API endpoints that manage webhooks in the Umbraco CMS.
/// </summary>
[VersionedApiBackOfficeRoute("webhook")]
[ApiExplorerSettings(GroupName = "Webhook")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
[RequiresPermissionsFor(Constants.Permissions.Entities.Webhook.Resource)]
public abstract class WebhookControllerBase : ManagementApiControllerBase
{
    protected IActionResult WebhookOperationStatusResult(WebhookOperationStatus status) =>
        status switch
        {
            WebhookOperationStatus.NotFound => WebhookNotFound(),
            WebhookOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the webhook operation.")
                .Build()),
            WebhookOperationStatus.NoEvents => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("No events specified")
                .WithDetail("The webhook must be configured to listen to at least one event.")
                .Build()),
            WebhookOperationStatus.InvalidHeaders => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid headers")
                .WithDetail($"Header names must be valid HTTP header names, header values must contain only printable ASCII characters, and the following headers cannot be set: {string.Join(", ", Constants.WebhookEvents.ReservedHeaderNames)}.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown webhook operation status.")
                .Build()),
        };

    protected IActionResult WebhookNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The webhook could not be found")
        .Build());
}
