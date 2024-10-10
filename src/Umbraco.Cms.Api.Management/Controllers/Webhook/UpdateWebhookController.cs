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

[ApiVersion("1.0")]
public class UpdateWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;


    public UpdateWebhookController(
        IWebhookService webhookService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
