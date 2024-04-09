using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[ApiVersion("1.0")]
public class ByKeyWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    public ByKeyWebhookController(IWebhookService webhookService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(WebhookResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
