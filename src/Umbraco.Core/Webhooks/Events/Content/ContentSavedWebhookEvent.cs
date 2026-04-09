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
/// Webhook event that fires when content is saved.
/// </summary>
[WebhookEvent("Content Saved", Constants.WebhookEvents.Types.Content)]
public class ContentSavedWebhookEvent : WebhookEventContentBase<ContentSavedNotification, IContent>
{
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _contentCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
    /// <param name="contentCache">The published content cache.</param>
    public ContentSavedWebhookEvent(
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentSaved;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentSavedNotification notification) =>
        notification.SavedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IContent entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
