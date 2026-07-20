using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

/// <summary>
/// API controller responsible for managing and handling webhook event requests.
/// </summary>
[ApiVersion("1.0")]
public class EventsWebhookController : WebhookControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly WebhookEventCollection _webhookEventCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Webhook.EventsWebhookController"/> class, providing dependencies for handling webhook events.
    /// </summary>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping Umbraco objects.</param>
    /// <param name="webhookEventCollection">A collection of available webhook events to be managed by the controller.</param>
    public EventsWebhookController(IUmbracoMapper umbracoMapper, WebhookEventCollection webhookEventCollection)
    {
        _umbracoMapper = umbracoMapper;
        _webhookEventCollection = webhookEventCollection;
    }

    /// <summary>
    /// Retrieves a paginated collection of available webhook events that can be subscribed to.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>A <see cref="PagedViewModel{WebhookEventViewModel}"/> containing the paginated list of webhook event view models.</returns>
    [HttpGet("events")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<WebhookEventViewModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of webhook events.")]
    [EndpointDescription("Gets a paginated collection of available webhook events that can be subscribed to.")]
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
