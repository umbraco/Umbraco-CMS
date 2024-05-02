using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[ApiVersion("1.0")]
public class EventsWebhookController : WebhookControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly WebhookEventCollection _webhookEventCollection;

    public EventsWebhookController(IUmbracoMapper umbracoMapper, WebhookEventCollection webhookEventCollection)
    {
        _umbracoMapper = umbracoMapper;
        _webhookEventCollection = webhookEventCollection;
    }

    [HttpGet("events")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<WebhookEventViewModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<PagedViewModel<WebhookEventViewModel>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        List<WebhookEventViewModel> events = _umbracoMapper.MapEnumerable<IWebhookEvent, WebhookEventViewModel>(_webhookEventCollection.AsEnumerable());

        var pagedModel = new PagedViewModel<WebhookEventViewModel>
        {
            Items = events.Skip(skip).Take(take),
            Total = events.Count,
        };

        return Task.FromResult<ActionResult<PagedViewModel<WebhookEventViewModel>>>(Ok(pagedModel));
    }
}
