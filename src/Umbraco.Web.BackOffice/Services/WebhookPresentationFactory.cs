using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Services;

internal class WebhookPresentationFactory : IWebhookPresentationFactory
{
    private readonly WebhookEventCollection _webhookEventCollection;

    public WebhookPresentationFactory(WebhookEventCollection webhookEventCollection) => _webhookEventCollection = webhookEventCollection;

    public WebhookViewModel Create(Webhook webhook)
    {
        var target = new WebhookViewModel();
        target.ContentTypeKeys = webhook.ContentTypeKeys;
        target.Events = webhook.Events.Select(Create).ToArray();
        target.Url = webhook.Url;
        target.Enabled = webhook.Enabled;
        target.Key = webhook.Key;
        target.Headers = webhook.Headers;

        return target;
    }

    private WebhookEventViewModel Create(string eventName)
    {
        IWebhookEvent? webhookEvent = _webhookEventCollection.FirstOrDefault(x => x.EventName == eventName);
        return new WebhookEventViewModel
        {
            EventName = eventName,
            EventType = webhookEvent?.EventType ?? WebhookEventType.None,
        };
    }
}
