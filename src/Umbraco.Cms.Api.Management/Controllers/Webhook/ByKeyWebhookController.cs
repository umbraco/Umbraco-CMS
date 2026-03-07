using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

    /// <summary>
    /// Controller for managing webhooks by their unique key.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyWebhookController"/> class.
    /// </summary>
    /// <param name="webhookService">Service for managing webhook operations.</param>
    /// <param name="webhookPresentationFactory">Factory for creating webhook presentation models.</param>
    public ByKeyWebhookController(IWebhookService webhookService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(WebhookResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a webhook.")]
    [EndpointDescription("Gets a webhook identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IWebhook? webhook = await _webhookService.GetAsync(id);
        if (webhook is null)
        {
            return WebhookOperationStatusResult(WebhookOperationStatus.NotFound);
        }

        WebhookResponseModel model = _webhookPresentationFactory.CreateResponseModel(webhook);
        return Ok(model);
    }
}
