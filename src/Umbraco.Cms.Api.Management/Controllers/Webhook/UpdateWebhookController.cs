using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

    /// <summary>
    /// Controller responsible for handling HTTP requests related to updating webhooks.
    /// Provides endpoints for modifying existing webhook configurations.
    /// </summary>
[ApiVersion("1.0")]
public class UpdateWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;


    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Webhook.UpdateWebhookController"/> class with the specified webhook service and presentation factory.
    /// </summary>
    /// <param name="webhookService">An instance of <see cref="IWebhookService"/> used to manage webhooks.</param>
    /// <param name="webhookPresentationFactory">An instance of <see cref="IWebhookPresentationFactory"/> used to create webhook presentation models.</param>
    public UpdateWebhookController(
        IWebhookService webhookService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    /// <summary>
    /// Updates a webhook identified by the provided Id with the details from the request model.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the webhook to update.</param>
    /// <param name="updateWebhookRequestModel">The request model containing updated webhook details.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Updates a webhook.")]
    [EndpointDescription("Updates a webhook identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateWebhookRequestModel updateWebhookRequestModel)
    {
        IWebhook? current = await _webhookService.GetAsync(id);
        if (current is null)
        {
            return WebhookNotFound();
        }

        IWebhook updated = _webhookPresentationFactory.CreateWebhook(updateWebhookRequestModel, id);

        Attempt<IWebhook, WebhookOperationStatus> result = await _webhookService.UpdateAsync(updated);

        return result.Success
            ? Ok()
            : WebhookOperationStatusResult(result.Status);
    }
}
