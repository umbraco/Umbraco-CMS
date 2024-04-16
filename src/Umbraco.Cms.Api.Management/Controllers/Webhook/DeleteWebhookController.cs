using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
public class DeleteWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DeleteWebhookController(IWebhookService webhookService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webhookService = webhookService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete($"{{{nameof(id)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IWebhook?, WebhookOperationStatus> result = await _webhookService.DeleteAsync(id);

        return result.Success
            ? Ok()
            : WebhookOperationStatusResult(result.Status);
    }
}
