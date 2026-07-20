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
/// Legacy webhook event that fires when content is published, using the legacy payload format.
/// </summary>
[WebhookEvent("Content Published", Constants.WebhookEvents.Types.Content)]
public class LegacyContentPublishedWebhookEvent : WebhookEventContentBase<ContentPublishedNotification, IContent>
{
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _publishedContentCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyContentPublishedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
    /// <param name="publishedContentCache">The published content cache.</param>
    public LegacyContentPublishedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiContentBuilder apiContentBuilder,
        IPublishedContentCache publishedContentCache)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _apiContentBuilder = apiContentBuilder;
        _publishedContentCache = publishedContentCache;
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentPublish;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) => notification.PublishedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IContent entity)
    {
        IPublishedContent? publishedContent = _publishedContentCache.GetById(entity.Key);
        return publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);
    }
}
