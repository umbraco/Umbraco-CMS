using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Logs;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Logs;

/// <summary>
/// Provides API endpoints for retrieving and managing webhook log entries.
/// </summary>
[ApiVersion("1.0")]
public class WebhookLogController : WebhookLogControllerBase
{
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Webhook.Logs.WebhookLogController"/> class, responsible for managing webhook log operations.
    /// </summary>
    /// <param name="webhookLogService">Service used to access and manage webhook logs.</param>
    /// <param name="webhookPresentationFactory">Factory for creating webhook presentation models.</param>
    public WebhookLogController(IWebhookLogService webhookLogService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookLogService = webhookLogService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    /// <summary>
    /// Retrieves a paginated list of logs for the specified webhook.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the webhook whose logs are to be retrieved.</param>
    /// <param name="skip">The number of log entries to skip (for pagination).</param>
    /// <param name="take">The maximum number of log entries to return.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains an <see cref="IActionResult"/> with a paginated collection of <see cref="WebhookLogResponseModel"/>.</returns>
    [HttpGet("{id:guid}/logs")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<WebhookLogResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of webhook logs for a specific webhook.")]
    [EndpointDescription("Gets a paginated collection of webhook logs for the webhook identified by the provided Id.")]
    public async Task<IActionResult> Logs(CancellationToken cancellationToken, Guid id, int skip = 0, int take = 100)
    {
        PagedModel<WebhookLog> logs = await _webhookLogService.Get(id, skip, take);
        PagedViewModel<WebhookLogResponseModel> viewModel = CreatePagedWebhookLogResponseModel(logs, _webhookPresentationFactory);
        return Ok(viewModel);
    }
}
