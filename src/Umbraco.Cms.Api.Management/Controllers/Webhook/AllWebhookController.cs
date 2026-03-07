using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

    /// <summary>
    /// Controller responsible for managing webhook operations in the management API.
    /// Provides endpoints to create, update, delete, and retrieve webhooks.
    /// </summary>
[ApiVersion("1.0")]
public class AllWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

/// <summary>
/// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Webhook.AllWebhookController"/> class.
/// </summary>
/// <param name="webhookService">Service used to manage webhooks.</param>
/// <param name="webhookPresentationFactory">Factory for creating webhook presentation models.</param>
    public AllWebhookController(IWebhookService webhookService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    /// <summary>
    /// Retrieves a paginated list of all webhooks.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of webhooks to skip before starting to collect the result set. Defaults to 0.</param>
    /// <param name="take">The maximum number of webhooks to return. Defaults to 100.</param>
    /// <returns>A <see cref="PagedViewModel{WebhookResponseModel}"/> containing the total count and the collection of webhook response models.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<WebhookResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of webhooks.")]
    [EndpointDescription("Gets a paginated collection of all webhooks.")]
    public async Task<ActionResult<PagedViewModel<WebhookResponseModel>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        PagedModel<IWebhook> result = await _webhookService.GetAllAsync(skip, take);
        IWebhook[] webhooks = result.Items.ToArray();
        var viewModel = new PagedViewModel<WebhookResponseModel>
        {
            Total = result.Total,
            Items = webhooks.Select(_webhookPresentationFactory.CreateResponseModel),
        };

        return Ok(viewModel);
    }
}
