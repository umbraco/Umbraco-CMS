using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Abstract base class for extended content webhook events that include full Delivery API content payloads.
/// </summary>
/// <typeparam name="TNotification">The type of notification this webhook event handles.</typeparam>
public abstract class ExtendedContentWebhookEventBase<TNotification> : WebhookEventContentBase<TNotification, IContent>
    where TNotification : INotification
{
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedContentWebhookEventBase{TNotification}"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="outputExpansionStrategyAccessor">The output expansion strategy accessor.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    public ExtendedContentWebhookEventBase(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor)
    {
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
        _variationContextAccessor = variationContextAccessor;
    }

    /// <summary>
    /// Builds a dictionary of culture-specific properties for the given content.
    /// </summary>
    /// <param name="publishedContent">The published content.</param>
    /// <param name="deliveryContent">The Delivery API content response.</param>
    /// <returns>A dictionary containing culture codes as keys and culture-specific data as values.</returns>
    public Dictionary<string, object> BuildCultureProperties(
        IPublishedContent publishedContent,
        IApiContentResponse deliveryContent)
    {
        var cultures = new Dictionary<string, object>();

        // just to be safe that messing with the variationContext doesn't screw things up
        VariationContext? originalVariationContext = _variationContextAccessor.VariationContext;

        try
        {
            foreach (KeyValuePair<string, IApiContentRoute> culture in deliveryContent.Cultures)
            {
                _variationContextAccessor.VariationContext = new VariationContext(culture.Key);

                IDictionary<string, object?> properties =
                    _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
                        ? outputExpansionStrategy.MapContentProperties(publishedContent!)
                        : new Dictionary<string, object?>();

                cultures.Add(culture.Key, new { culture.Value.Path, culture.Value.StartItem, properties, });
            }
        }
        finally
        {
            _variationContextAccessor.VariationContext = originalVariationContext;
        }

        return cultures;
    }
}
