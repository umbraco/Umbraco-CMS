using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Webhook;

[ApiVersion("1.0")]
public class ByKeyWebhookController : WebhookControllerBase
{
    private readonly IWebHookService _webhookService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyWebhookController(IWebHookService webhookService, IUmbracoMapper umbracoMapper)
    {
        _webhookService = webhookService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(WebhookResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id)
    {
        Core.Models.Webhook? webhook = await _webhookService.GetAsync(id);
        if (webhook == null)
        {
            return WebhookOperationStatusResult(WebhookOperationStatus.NotFound);
        }

        WebhookResponseModel model = _umbracoMapper.Map<WebhookResponseModel>(webhook)!;
        return await Task.FromResult(Ok(model));
    }
}
