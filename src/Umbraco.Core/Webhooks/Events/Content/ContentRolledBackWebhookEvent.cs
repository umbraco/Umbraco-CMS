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
/// Webhook event that fires when content is rolled back to a previous version.
/// </summary>
[WebhookEvent("Content Rolled Back", Constants.WebhookEvents.Types.Content)]
public class ContentRolledBackWebhookEvent : WebhookEventContentBase<ContentRolledBackNotification, IContent>
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentRolledBackWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="contentCache">The published content cache.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
    public ContentRolledBackWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ContentRolledBack;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentRolledBackNotification notification) =>
        new List<IContent> { notification.Entity };

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IContent entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
