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
public class MediaSavedWebhookEvent : WebhookEventContentBase<MediaSavedNotification, IMedia>
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IApiMediaBuilder _apiMediaBuilder;

    public MediaSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiMediaBuilder apiMediaBuilder)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        this._publishedSnapshotAccessor = publishedSnapshotAccessor;
        this._apiMediaBuilder = apiMediaBuilder;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.MediaSave;

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaSavedNotification notification) => notification.SavedEntities;

    protected override object? ConvertEntityToRequestPayload(IMedia entity)
    {
        if (this._publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot) is false || publishedSnapshot!.Content is null)
        {
            return null;
        }

        IPublishedContent? publishedContent = publishedSnapshot.Media?.GetById(entity.Key);
        return publishedContent is null ? null : this._apiMediaBuilder.Build(publishedContent);
    }
}
