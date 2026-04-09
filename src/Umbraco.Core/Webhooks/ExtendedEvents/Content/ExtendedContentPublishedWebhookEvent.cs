using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Extended webhook event that fires when content is published, including full Delivery API content payloads.
/// </summary>
[WebhookEvent("Content Published", Constants.WebhookEvents.Types.Content)]
public class ExtendedContentPublishedWebhookEvent : ExtendedContentWebhookEventBase<ContentPublishedNotification>
{
    private readonly IApiContentResponseBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _publishedContentCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedContentPublishedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="apiContentBuilder">The API content response builder.</param>
    /// <param name="publishedContentCache">The published content cache.</param>
    /// <param name="outputExpansionStrategyAccessor">The output expansion strategy accessor.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    public ExtendedContentPublishedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiContentResponseBuilder apiContentBuilder,
        IPublishedContentCache publishedContentCache,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor,
            outputExpansionStrategyAccessor,
            variationContextAccessor)
    {
        _apiContentBuilder = apiContentBuilder;
        _publishedContentCache = publishedContentCache;
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentPublish;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) =>
        notification.PublishedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IContent entity)
    {
        IPublishedContent? publishedContent = _publishedContentCache.GetById(entity.Key);
        IApiContentResponse? deliveryContent =
            publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);

        if (deliveryContent == null)
        {
            return null;
        }

        Dictionary<string, object> cultures = BuildCultureProperties(publishedContent!, deliveryContent);

        return new
        {
            deliveryContent.Id,
            deliveryContent.ContentType,
            deliveryContent.Name,
            deliveryContent.CreateDate,
            deliveryContent.UpdateDate,
            Cultures = cultures,
        };
    }
}
