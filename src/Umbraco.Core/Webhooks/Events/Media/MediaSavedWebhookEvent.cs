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
/// Webhook event that fires when media is saved.
/// </summary>
[WebhookEvent("Media Saved", Constants.WebhookEvents.Types.Media)]
public class MediaSavedWebhookEvent : WebhookEventContentBase<MediaSavedNotification, IMedia>
{
    private readonly IPublishedMediaCache _mediaCache;
    private readonly IApiMediaBuilder _apiMediaBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="mediaCache">The published media cache.</param>
    /// <param name="apiMediaBuilder">The API media builder.</param>
    public MediaSavedWebhookEvent(
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.MediaSave;

    /// <inheritdoc />
    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaSavedNotification notification)
        => notification.SavedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IMedia entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
