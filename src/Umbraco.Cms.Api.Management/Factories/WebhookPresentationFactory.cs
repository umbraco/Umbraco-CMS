using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Logs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Api.Management.Factories;

internal class WebhookPresentationFactory : IWebhookPresentationFactory
{
    private readonly WebhookEventCollection _webhookEventCollection;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILocalizedTextService _localizedTextService;

    public WebhookPresentationFactory(
        WebhookEventCollection webhookEventCollection,
        IHostingEnvironment hostingEnvironment,
        ILocalizedTextService localizedTextService)
    {
        _webhookEventCollection = webhookEventCollection;
        _hostingEnvironment = hostingEnvironment;
        _localizedTextService = localizedTextService;
    }

    public WebhookResponseModel CreateResponseModel(IWebhook webhook)
    {
        var target = new WebhookResponseModel
        {
            Events = webhook.Events.Select(Create).ToArray(),
            Name = webhook.Name,
            Description = webhook.Description,
            Url = webhook.Url,
            Enabled = webhook.Enabled,
            Id = webhook.Key,
            Headers = webhook.Headers,
            ContentTypeKeys = webhook.ContentTypeKeys,
        };

        return target;
    }

    public IWebhook CreateWebhook(CreateWebhookRequestModel webhookRequestModel)
    {
        var target = new Webhook(webhookRequestModel.Url, webhookRequestModel.Enabled, webhookRequestModel.ContentTypeKeys, webhookRequestModel.Events, webhookRequestModel.Headers)
        {
            Key = webhookRequestModel.Id ?? Guid.NewGuid(),
            Name = webhookRequestModel.Name,
            Description = webhookRequestModel.Description,
        };
        return target;
    }

    public IWebhook CreateWebhook(UpdateWebhookRequestModel webhookRequestModel, Guid existingWebhookkey)
    {
        var target = new Webhook(webhookRequestModel.Url, webhookRequestModel.Enabled, webhookRequestModel.ContentTypeKeys, webhookRequestModel.Events, webhookRequestModel.Headers)
        {
            Key = existingWebhookkey,
            Name = webhookRequestModel.Name,
            Description = webhookRequestModel.Description,
        };
        return target;
    }

    public WebhookLogResponseModel CreateResponseModel(WebhookLog webhookLog)
    {
        var webhookLogResponseModel = new WebhookLogResponseModel
        {
            Date = webhookLog.Date, EventAlias = webhookLog.EventAlias, Key = webhookLog.Key, RequestBody = webhookLog.RequestBody ?? string.Empty,
            RetryCount = webhookLog.RetryCount,
            Url = webhookLog.Url,
            RequestHeaders = webhookLog.RequestHeaders,
            WebhookKey = webhookLog.WebhookKey,
            IsSuccessStatusCode = webhookLog.IsSuccessStatusCode
        };

        if (_hostingEnvironment.IsDebugMode)
        {
            webhookLogResponseModel.ExceptionOccured = webhookLog.ExceptionOccured;
            webhookLogResponseModel.ResponseBody = webhookLog.ResponseBody;
            webhookLogResponseModel.ResponseHeaders = webhookLog.ResponseHeaders;
            webhookLogResponseModel.StatusCode = webhookLog.StatusCode;
        }
        else
        {
            webhookLogResponseModel.ResponseBody = _localizedTextService.Localize("webhooks", "toggleDebug", Thread.CurrentThread.CurrentUICulture);
            webhookLogResponseModel.StatusCode = webhookLog.StatusCode is "OK (200)" ? webhookLog.StatusCode : _localizedTextService.Localize("webhooks", "statusNotOk", Thread.CurrentThread.CurrentUICulture);
        }

        return webhookLogResponseModel;
    }

    private WebhookEventResponseModel Create(string alias)
    {
        IWebhookEvent? webhookEvent = _webhookEventCollection.FirstOrDefault(x => x.Alias == alias);

        return new WebhookEventResponseModel
        {
            EventName = webhookEvent?.EventName ?? alias,
            EventType = webhookEvent?.EventType ?? Constants.WebhookEvents.Types.Other,
            Alias = alias,
        };
    }
}
