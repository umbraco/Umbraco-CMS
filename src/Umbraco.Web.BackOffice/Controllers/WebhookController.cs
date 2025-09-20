using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.BackOffice.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessWebhooks)]
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
        PagedModel<IWebhook> webhooks = await _webhookService.GetAllAsync(skip, take);

        IEnumerable<WebhookViewModel> webhookViewModels = webhooks.Items.Select(_webhookPresentationFactory.Create);

        return Ok(webhookViewModels);
    }

    [HttpPut]
    public async Task<IActionResult> Update(WebhookViewModel webhookViewModel)
    {
        IWebhook webhook = _umbracoMapper.Map<IWebhook>(webhookViewModel)!;

        Attempt<IWebhook, WebhookOperationStatus> result = await _webhookService.UpdateAsync(webhook);
        return result.Success ? Ok(_webhookPresentationFactory.Create(webhook)) : WebhookOperationStatusResult(result.Status);
    }

    [HttpPost]
    public async Task<IActionResult> Create(WebhookViewModel webhookViewModel)
    {
        IWebhook webhook = _umbracoMapper.Map<IWebhook>(webhookViewModel)!;
        Attempt<IWebhook, WebhookOperationStatus> result = await _webhookService.CreateAsync(webhook);
        return result.Success ? Ok(_webhookPresentationFactory.Create(webhook)) : WebhookOperationStatusResult(result.Status);
    }

    [HttpGet]
    public async Task<IActionResult> GetByKey(Guid key)
    {
        IWebhook? webhook = await _webhookService.GetAsync(key);

        return webhook is null ? NotFound() : Ok(_webhookPresentationFactory.Create(webhook));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid key)
    {
        Attempt<IWebhook?, WebhookOperationStatus> result = await _webhookService.DeleteAsync(key);
        return result.Success ? Ok() : WebhookOperationStatusResult(result.Status);
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

    private IActionResult WebhookOperationStatusResult(WebhookOperationStatus status) =>
        status switch
        {
            WebhookOperationStatus.CancelledByNotification => ValidationProblem(new SimpleNotificationModel(new BackOfficeNotification[]
                {
                    new("Cancelled by notification", "The operation was cancelled by a notification", NotificationStyle.Error),
                })),
            WebhookOperationStatus.NotFound => NotFound("Could not find the webhook"),
            WebhookOperationStatus.NoEvents => ValidationProblem(new SimpleNotificationModel(new BackOfficeNotification[]
            {
                new("No events", "The webhook does not have any events", NotificationStyle.Error),
            })),
            _ => StatusCode(StatusCodes.Status500InternalServerError),

        };
}
