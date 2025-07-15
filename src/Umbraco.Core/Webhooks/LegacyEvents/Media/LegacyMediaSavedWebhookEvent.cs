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

[WebhookEvent("Media Saved", Constants.WebhookEvents.Types.Media)]
public class LegacyMediaSavedWebhookEvent : WebhookEventContentBase<MediaSavedNotification, IMedia>
{
    private readonly IPublishedMediaCache _mediaCache;
    private readonly IApiMediaBuilder _apiMediaBuilder;

    public LegacyMediaSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IPublishedMediaCache mediaCache,
        IApiMediaBuilder apiMediaBuilder)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _mediaCache = mediaCache;
        _apiMediaBuilder = apiMediaBuilder;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.MediaSave;

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaSavedNotification notification) => notification.SavedEntities;

    protected override object? ConvertEntityToRequestPayload(IMedia entity)
    {
        IPublishedContent? publishedContent = _mediaCache.GetById(entity.Key);
        return publishedContent is null ? null : _apiMediaBuilder.Build(publishedContent);
    }
}
