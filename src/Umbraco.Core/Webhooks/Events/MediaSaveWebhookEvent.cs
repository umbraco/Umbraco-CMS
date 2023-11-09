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

[WebhookEvent("Media was saved", Constants.WebhookEvents.Types.Media)]
public class MediaSaveWebhookEvent : WebhookEventContentBase<MediaSavedNotification, IMedia>
{
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IApiMediaBuilder _apiMediaBuilder;

    public MediaSaveWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IApiMediaBuilder apiMediaBuilder)
        : base(
            webhookFiringService,
            webHookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _apiMediaBuilder = apiMediaBuilder;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.MediaSave;

    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaSavedNotification notification) => notification.SavedEntities;

    protected override object? ConvertEntityToRequestPayload(IMedia entity)
    {
        if (_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot) is false || publishedSnapshot!.Content is null)
        {
            return null;
        }

        IPublishedContent? publishedContent = publishedSnapshot.Media?.GetById(entity.Key);
        return publishedContent is null ? null : _apiMediaBuilder.Build(publishedContent);
    }
}
