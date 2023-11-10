using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.BackOffice.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class WebhookController : UmbracoAuthorizedJsonController
{
    private readonly IWebhookService _webhookService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly WebhookEventCollection _webhookEventCollection;
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebhookPresentationFactory _webhookPresentationFactory;

    public WebhookController(IWebhookService webhookService, IUmbracoMapper umbracoMapper, WebhookEventCollection webhookEventCollection, IWebhookLogService webhookLogService, IWebhookPresentationFactory webhookPresentationFactory)
    {
        _webhookService = webhookService;
        _umbracoMapper = umbracoMapper;
        _webhookEventCollection = webhookEventCollection;
        _webhookLogService = webhookLogService;
        _webhookPresentationFactory = webhookPresentationFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int skip = 0, int take = int.MaxValue)
    {
        PagedModel<Webhook> webhooks = await _webhookService.GetAllAsync(skip, take);

        IEnumerable<WebhookViewModel> webhookViewModels = webhooks.Items.Select(_webhookPresentationFactory.Create);

        return Ok(webhookViewModels);
    }

    [HttpPut]
    public async Task<IActionResult> Update(WebhookViewModel webhookViewModel)
    {
        Webhook updateModel = _umbracoMapper.Map<Webhook>(webhookViewModel)!;

        await _webhookService.UpdateAsync(updateModel);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Create(WebhookViewModel webhookViewModel)
    {
        Webhook webhook = _umbracoMapper.Map<Webhook>(webhookViewModel)!;
        await _webhookService.CreateAsync(webhook);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetByKey(Guid key)
    {
        Webhook? webhook = await _webhookService.GetAsync(key);

        return webhook is null ? NotFound() : Ok(webhook);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid key)
    {
        await _webhookService.DeleteAsync(key);

        return Ok();
    }

    [HttpGet]
    public IActionResult GetEvents()
    {
        List<WebhookEventViewModel> viewModels = _umbracoMapper.MapEnumerable<IWebhookEvent, WebhookEventViewModel>(_webhookEventCollection.AsEnumerable());
        return Ok(viewModels);
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(int skip = 0, int take = int.MaxValue)
    {
        PagedModel<WebhookLog> logs = await _webhookLogService.Get(skip, take);
        List<WebhookLogViewModel> mappedLogs = _umbracoMapper.MapEnumerable<WebhookLog, WebhookLogViewModel>(logs.Items);
        return Ok(new PagedResult<WebhookLogViewModel>(logs.Total, 0, 0)
        {
            Items = mappedLogs,
        });
    }
}
