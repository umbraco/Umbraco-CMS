using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Sorted", Constants.WebhookEvents.Types.Content)]
public class ContentSortedWebhookEvent : WebhookEventBase<ContentSortedNotification>
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    public ContentSortedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IPublishedContentCache contentCache,
        IApiContentBuilder apiContentBuilder)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _contentCache = contentCache;
        _apiContentBuilder = apiContentBuilder;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentSorted;

    public override object? ConvertNotificationToRequestPayload(ContentSortedNotification notification)
        => notification.SortedEntities
            .OrderBy(entity => entity.SortOrder)
            .Select(entity => new
            {
                Id = entity.Key,
                entity.SortOrder,
            });
}
