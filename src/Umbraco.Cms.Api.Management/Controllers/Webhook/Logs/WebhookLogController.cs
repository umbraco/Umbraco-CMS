using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Logs;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Logs;

[ApiVersion("1.0")]
public class WebhookLogController : WebhookLogControllerBase
{
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    public WebhookLogController(IWebhookLogService webhookLogService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookLogService = webhookLogService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    [HttpGet("{id:guid}/logs")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<WebhookLogResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logs(CancellationToken cancellationToken, Guid id, int skip = 0, int take = 100)
    {
        PagedModel<WebhookLog> logs = await _webhookLogService.Get(id, skip, take);
        PagedViewModel<WebhookLogResponseModel> viewModel = CreatePagedWebhookLogResponseModel(logs, _webhookPresentationFactory);
        return Ok(viewModel);
    }
}
