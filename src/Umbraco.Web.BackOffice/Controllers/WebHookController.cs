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

    public WebHookController(IWebHookService webHookService, IUmbracoMapper umbracoMapper)
    {
        _webHookService = webHookService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<Webhook> webhooks = await _webHookService.GetAllAsync();

        List<WebhookViewModel> webhookViewModels = _umbracoMapper.MapEnumerable<Webhook, WebhookViewModel>(webhooks);

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
        // Load the assembly containing your webhook event classes
        Assembly assembly = typeof(IWebhookEvent).Assembly;

        // Get all types in the assembly that implement IWebhookEvent
        var webhookEventTypes = assembly
            .GetTypes()
            .Where(type => typeof(IWebhookEvent).IsAssignableFrom(type) && !type.IsAbstract);

        // Create instances of each type and select the EventName property value for each instance
        var eventNames = webhookEventTypes
            .Select(type =>
            {
                if (Activator.CreateInstance(type) is IWebhookEvent webhookEvent)
                {
                    return webhookEvent.EventName;
                }

                return null;
            })
            .Where(eventName => !string.IsNullOrEmpty(eventName))
            .ToArray();

        return Ok(eventNames);
    }
}
