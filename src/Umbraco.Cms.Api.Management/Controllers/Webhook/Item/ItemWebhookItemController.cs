using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Item;

/// <summary>
/// Provides API endpoints for managing webhooks associated with items.
/// </summary>
[ApiVersion("1.0")]
public class ItemWebhookItemController : WebhookItemControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemWebhookItemController"/> class, responsible for managing webhook items.
    /// </summary>
    /// <param name="webhookService">The service used to handle webhook operations.</param>
    /// <param name="mapper">The mapper used for mapping Umbraco objects.</param>
    public ItemWebhookItemController(IWebhookService webhookService, IUmbracoMapper mapper)
    {
        _webhookService = webhookService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a collection of webhook items corresponding to the specified IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of webhook item IDs to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with the collection of webhook items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<WebhookItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of webhook items.")]
    [EndpointDescription("Gets a collection of webhook items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<WebhookItemResponseModel>());
        }

        IEnumerable<IWebhook?> webhooks = await _webhookService.GetMultipleAsync(ids);
        List<WebhookItemResponseModel> entityResponseModels = _mapper.MapEnumerable<IWebhook?, WebhookItemResponseModel>(webhooks);
        return Ok(entityResponseModels);
    }
}
