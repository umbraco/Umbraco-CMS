using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when content is published.
/// </summary>
[WebhookEvent("Content Published", Constants.WebhookEvents.Types.Content)]
public class ContentPublishedWebhookEvent : WebhookEventContentBase<ContentPublishedNotification, IContent>
{
    private readonly IApiContentBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _publishedContentCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPublishedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
    /// <param name="publishedContentCache">The published content cache.</param>
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentPublish;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) => notification.PublishedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IContent entity)
        => new
        {
            Id = entity.Key,
            Cultures = entity.PublishCultureInfos?.Values.Select(cultureInfo => new
            {
                cultureInfo.Culture,
                cultureInfo.Date,
            }),
        };
}
