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

[WebhookEvent("Content Published", Constants.WebhookEvents.Types.Content)]
public class ContentPublishedWebhookEvent : WebhookEventContentBase<ContentPublishedNotification, IContent>
{
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _publishedContentCache;

    public ContentPublishedWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentPublish;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) => notification.PublishedEntities;

    protected override object? ConvertEntityToRequestPayload(IContent entity)
    {
        IPublishedContent? publishedContent = _publishedContentCache.GetById(entity.Key);
        return publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);
    }
}
