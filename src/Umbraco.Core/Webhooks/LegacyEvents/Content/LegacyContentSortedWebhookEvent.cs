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
public class LegacyContentSortedWebhookEvent : WebhookEventBase<ContentSortedNotification>
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    public LegacyContentSortedWebhookEvent(
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
    {
        var sortedEntities = new List<object?>();
        foreach (var entity in notification.SortedEntities)
        {
            IPublishedContent? publishedContent = _contentCache.GetById(entity.Key);
            object? payload = publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);
            sortedEntities.Add(payload);
        }
        return sortedEntities;
    }
}
