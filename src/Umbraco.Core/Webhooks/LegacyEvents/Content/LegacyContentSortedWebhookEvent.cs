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

/// <summary>
/// Legacy webhook event that fires when content is sorted, using the legacy payload format.
/// </summary>
[WebhookEvent("Content Sorted", Constants.WebhookEvents.Types.Content)]
public class LegacyContentSortedWebhookEvent : WebhookEventBase<ContentSortedNotification>
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyContentSortedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="contentCache">The published content cache.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentSorted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentSortedNotification notification)
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
