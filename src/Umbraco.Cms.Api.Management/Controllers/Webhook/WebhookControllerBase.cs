using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[VersionedApiBackOfficeRoute("webhook")]
[ApiExplorerSettings(GroupName = "Webhook")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
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
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown webhook operation status.")
                .Build()),
        };

    protected IActionResult WebhookNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The webhook could not be found")
        .Build());
}
