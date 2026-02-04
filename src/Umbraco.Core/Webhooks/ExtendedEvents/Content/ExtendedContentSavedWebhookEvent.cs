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
/// Extended webhook event that fires when content is saved, including full Delivery API content payloads.
/// </summary>
[WebhookEvent("Content Saved", Constants.WebhookEvents.Types.Content)]
public class ExtendedContentSavedWebhookEvent : ExtendedContentWebhookEventBase<ContentSavedNotification>
{
    private readonly IApiContentResponseBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _contentCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedContentSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="apiContentBuilder">The API content response builder.</param>
    /// <param name="contentCache">The published content cache.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="outputExpansionStrategyAccessor">The output expansion strategy accessor.</param>
    public ExtendedContentSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiContentResponseBuilder apiContentBuilder,
        IPublishedContentCache contentCache,
        IVariationContextAccessor variationContextAccessor,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor,
            outputExpansionStrategyAccessor,
            variationContextAccessor)
    {
        _apiContentBuilder = apiContentBuilder;
        _contentCache = contentCache;
        _variationContextAccessor = variationContextAccessor;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentSaved;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentSavedNotification notification) =>
        notification.SavedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IContent entity)
    {
        // Get preview/saved version of content
        IPublishedContent? publishedContent = _contentCache.GetById(true, entity.Key);
        IApiContentResponse? deliveryContent = publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);

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
