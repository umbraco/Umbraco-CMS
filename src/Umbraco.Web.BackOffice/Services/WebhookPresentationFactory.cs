using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Services;

internal class WebhookPresentationFactory : IWebhookPresentationFactory
{
    private readonly WebhookEventCollection _webhookEventCollection;

    public WebhookPresentationFactory(WebhookEventCollection webhookEventCollection) => _webhookEventCollection = webhookEventCollection;

    public WebhookViewModel Create(IWebhook webhook)
    {
        var target = new WebhookViewModel
        {
            ContentTypeKeys = webhook.ContentTypeKeys,
            Events = webhook.Events.Select(Create).ToArray(),
            Url = webhook.Url,
            Enabled = webhook.Enabled,
            Id = webhook.Id,
            Key = webhook.Key,
            Headers = webhook.Headers,
        };

        return target;
    }

    private WebhookEventViewModel Create(string alias)
    {
        IWebhookEvent? webhookEvent = _webhookEventCollection.FirstOrDefault(x => x.Alias == alias);

        return new WebhookEventViewModel
        {
            EventName = webhookEvent?.EventName ?? alias,
            EventType = webhookEvent?.EventType ?? Constants.WebhookEvents.Types.Other,
            Alias = alias,
        };
    }
}
