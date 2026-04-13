using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

/// <summary>
/// Controller responsible for handling requests to delete webhooks in the management API.
/// </summary>
[ApiVersion("1.0")]
public class DeleteWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteWebhookController"/> class, responsible for handling webhook deletion operations.
    /// </summary>
    /// <param name="webhookService">Service used to manage webhook operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public DeleteWebhookController(IWebhookService webhookService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _webhookService = webhookService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>Deletes a webhook identified by the provided Id.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the webhook to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Deletes a webhook.")]
    [EndpointDescription("Deletes a webhook identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IWebhook?, WebhookOperationStatus> result = await _webhookService.DeleteAsync(id);

        return result.Success
            ? Ok()
            : WebhookOperationStatusResult(result.Status);
    }
}
