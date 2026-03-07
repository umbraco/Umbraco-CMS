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
    /// API controller responsible for handling HTTP requests related to the creation of webhooks in the management API.
    /// </summary>
[ApiVersion("1.0")]
public class CreateWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWebhookController"/> class, which handles webhook creation operations in the management API.
    /// </summary>
    /// <param name="webhookService">Service used to manage webhook logic and persistence.</param>
    /// <param name="webhookPresentationFactory">Factory for creating webhook presentation models.</param>
    public CreateWebhookController(
        IWebhookService webhookService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new webhook.")]
    [EndpointDescription("Creates a new webhook with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateWebhookRequestModel createWebhookRequestModel)
    {
        IWebhook created = _webhookPresentationFactory.CreateWebhook(createWebhookRequestModel);

        Attempt<IWebhook, WebhookOperationStatus> result = await _webhookService.CreateAsync(created);

        return result.Success
            ? CreatedAtId<ByKeyWebhookController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : WebhookOperationStatusResult(result.Status);
    }
}
