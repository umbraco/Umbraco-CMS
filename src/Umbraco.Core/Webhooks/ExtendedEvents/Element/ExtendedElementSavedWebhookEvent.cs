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
/// Extended webhook event that fires when an element is saved, including full Delivery API element payloads.
/// </summary>
[WebhookEvent("Element Saved", Constants.WebhookEvents.Types.Element)]
public class ExtendedElementSavedWebhookEvent : WebhookEventContentBase<ElementSavedNotification, IElement>
{
    private readonly IApiElementBuilder _apiElementBuilder;
    private readonly IElementCacheService _elementCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedElementSavedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="apiElementBuilder">The API element builder.</param>
    /// <param name="elementCacheService">The element cache service.</param>
    public ExtendedElementSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiElementBuilder apiElementBuilder,
        IElementCacheService elementCacheService)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _apiElementBuilder = apiElementBuilder;
        _elementCacheService = elementCacheService;
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ElementSaved;

    /// <inheritdoc />
    protected override IEnumerable<IElement> GetEntitiesFromNotification(ElementSavedNotification notification) =>
        notification.SavedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IElement entity)
    {
        // Get preview/saved version of element
        IPublishedElement? publishedElement = _elementCacheService.GetByKeyAsync(entity.Key, preview: true).GetAwaiter().GetResult();
        if (publishedElement is null)
        {
            return null;
        }

        IApiElement apiElement = _apiElementBuilder.Build(publishedElement);

        return new
        {
            apiElement.Id,
            apiElement.ContentType,
            apiElement.Properties,
        };
    }
}
