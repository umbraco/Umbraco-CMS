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

[WebhookEvent("Content Saved", Constants.WebhookEvents.Types.Content)]
public class LegacyContentSavedWebhookEvent : WebhookEventContentBase<ContentSavedNotification, IContent>
{
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _contentCache;

    public LegacyContentSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiContentBuilder apiContentBuilder,
        IPublishedContentCache contentCache)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _apiContentBuilder = apiContentBuilder;
        _contentCache = contentCache;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentSaved;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentSavedNotification notification) =>
        notification.SavedEntities;

    protected override object? ConvertEntityToRequestPayload(IContent entity)
    {
        // Get preview/saved version of content
        IPublishedContent? publishedContent = _contentCache.GetById(true, entity.Key);
        return publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);
    }
}
