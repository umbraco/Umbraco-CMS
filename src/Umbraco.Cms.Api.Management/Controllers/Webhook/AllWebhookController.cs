using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[ApiVersion("1.0")]
public class AllWebhookController : WebhookControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    public AllWebhookController(IWebhookService webhookService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<WebhookResponseModel>), StatusCodes.Status200OK)]
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
            Items = webhooks.Select(x => _webhookPresentationFactory.CreateResponseModel(x)),
        };

        return Ok(viewModel);
    }
}
