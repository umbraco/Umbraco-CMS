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
/// Webhook event that fires when content is sorted.
/// </summary>
[WebhookEvent("Content Sorted", Constants.WebhookEvents.Types.Content)]
public class ContentSortedWebhookEvent : WebhookEventBase<ContentSortedNotification>
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentSortedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="contentCache">The published content cache.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentSorted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentSortedNotification notification)
        => notification.SortedEntities
            .OrderBy(entity => entity.SortOrder)
            .Select(entity => new
            {
                Id = entity.Key,
                entity.SortOrder,
            });
}
