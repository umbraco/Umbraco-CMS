using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Item;

[ApiVersion("1.0")]
public class ItemWebhookItemController : WebhookItemControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly IUmbracoMapper _mapper;

    public ItemWebhookItemController(IWebhookService webhookService, IUmbracoMapper mapper)
    {
        _webhookService = webhookService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<WebhookItemResponseModel>), StatusCodes.Status200OK)]
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
