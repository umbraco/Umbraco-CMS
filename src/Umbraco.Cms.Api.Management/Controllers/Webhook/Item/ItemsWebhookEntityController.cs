using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Language.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook.Item;

[ApiVersion("1.0")]
public class ItemsWebhookEntityController : WebhookEntityControllerBase
{
    private readonly IWebHookService _webhookService;
    private readonly IUmbracoMapper _mapper;

    public ItemsWebhookEntityController(IWebHookService webhookService, IUmbracoMapper mapper)
    {
        _webhookService = webhookService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<WebhookItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Items([FromQuery(Name = "isoCode")] HashSet<string> isoCodes)
    {
        IEnumerable<ILanguage> languages = await _languageService.GetMultipleAsync(isoCodes);
        List<WebhookItemResponseModel> entityResponseModels = _mapper.MapEnumerable<ILanguage, WebhooktemResponseModel>(languages);
        return Ok(entityResponseModels);
    }
}
