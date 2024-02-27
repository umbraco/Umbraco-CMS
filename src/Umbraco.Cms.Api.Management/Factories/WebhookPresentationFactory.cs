using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Api.Management.Factories;

internal class WebhookPresentationFactory : IWebhookPresentationFactory
{
    private readonly WebhookEventCollection _webhookEventCollection;

    public WebhookPresentationFactory(WebhookEventCollection webhookEventCollection) => _webhookEventCollection = webhookEventCollection;

    public WebhookResponseModel CreateResponseModel(IWebhook webhook)
    {
        var target = new WebhookResponseModel
        {
            Events = webhook.Events.Select(Create).ToArray(),
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
        var target = new Webhook(webhookRequestModel.Url, webhookRequestModel.Enabled, webhookRequestModel.ContentTypeKeys, webhookRequestModel.Events, webhookRequestModel.Headers);
        return target;
    }

    public IWebhook CreateWebhook(UpdateWebhookRequestModel webhookRequestModel)
    {
        var target = new Webhook(webhookRequestModel.Url, webhookRequestModel.Enabled, webhookRequestModel.ContentTypeKeys, webhookRequestModel.Events, webhookRequestModel.Headers);
        return target;
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
