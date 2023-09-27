using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

public class WebHookController : UmbracoAuthorizedJsonController
{
    private readonly IWebHookService _webHookService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly WebhookEventCollection _webhookEventCollection;

    public WebHookController(IWebHookService webHookService, IUmbracoMapper umbracoMapper, WebhookEventCollection webhookEventCollection)
    {
        _webHookService = webHookService;
        _umbracoMapper = umbracoMapper;
        _webhookEventCollection = webhookEventCollection;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int skip = 0, int take = int.MaxValue)
    {
        PagedModel<Webhook> webhooks = await _webHookService.GetAllAsync(skip, take);

        List<WebhookViewModel> webhookViewModels = _umbracoMapper.MapEnumerable<Webhook, WebhookViewModel>(webhooks.Items);

        return Ok(webhookViewModels);
    }

    [HttpPut]
    public async Task<IActionResult> Update(WebhookViewModel webhookViewModel)
    {
        Webhook updateModel = _umbracoMapper.Map<Webhook>(webhookViewModel)!;

        await _webHookService.UpdateAsync(updateModel);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Create(WebhookViewModel webhookViewModel)
    {
        Webhook webhook = _umbracoMapper.Map<Webhook>(webhookViewModel)!;
        await _webHookService.CreateAsync(webhook);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetByKey(Guid key)
    {
        Webhook? webhook = await _webHookService.GetAsync(key);

        return webhook is null ? NotFound() : Ok(webhook);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid key)
    {
        await _webHookService.DeleteAsync(key);

        return Ok();
    }

    [HttpGet]
    public IActionResult GetEvents()
    {
        List<WebhookEventViewModel> viewModels = _umbracoMapper.MapEnumerable<IWebhookEvent, WebhookEventViewModel>(_webhookEventCollection.AsEnumerable());
        return Ok(viewModels);
    }
}
