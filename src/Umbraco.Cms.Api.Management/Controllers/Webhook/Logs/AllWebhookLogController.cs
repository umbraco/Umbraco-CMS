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
    /// Controller for retrieving and managing all webhook logs.
    /// </summary>
[ApiVersion("1.0")]
public class AllWebhookLogController : WebhookLogControllerBase
{
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllWebhookLogController"/> class.
    /// </summary>
    /// <param name="webhookLogService">The service used to manage webhook logs.</param>
    /// <param name="webhookPresentationFactory">The factory used to create webhook presentation models.</param>
    public AllWebhookLogController(IWebhookLogService webhookLogService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookLogService = webhookLogService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    /// <summary>
    /// Retrieves a paginated collection of webhook logs across all webhooks.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of log entries to skip before starting to collect the result set. Used for pagination.</param>
    /// <param name="take">The maximum number of log entries to return. Used for pagination.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{WebhookLogResponseModel}"/> representing the paginated webhook logs.
    /// </returns>
    [HttpGet("logs")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<WebhookLogResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of webhook logs.")]
    [EndpointDescription("Gets a paginated collection of webhook logs for all webhooks.")]
    public async Task<IActionResult> Logs(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<WebhookLog> logs = await _webhookLogService.Get(skip, take);
        PagedViewModel<WebhookLogResponseModel> viewModel = CreatePagedWebhookLogResponseModel(logs, _webhookPresentationFactory);
        return Ok(viewModel);
    }
}
