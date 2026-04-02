using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data about webhook usage and events within the system, enabling monitoring and diagnostics of webhook activity.
/// </summary>
public class WebhookTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IWebhookService _webhookService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Telemetry.Providers.WebhookTelemetryProvider"/> class, using the specified webhook service.
    /// </summary>
    /// <param name="webhookService">The <see cref="IWebhookService"/> instance used to manage webhooks.</param>
    public WebhookTelemetryProvider(IWebhookService webhookService) => _webhookService = webhookService;

    private readonly string[] _defaultEventTypes =
        [
            "Umbraco.ContentDelete",
            "Umbraco.ContentPublish",
            "Umbraco.ContentUnpublish",
            "Umbraco.MediaDelete",
            "Umbraco.MediaSave"
        ];

    /// <summary>
    /// Returns a collection of usage metrics about webhooks, including:
    /// <list type="bullet">
    /// <item>Total number of webhooks</item>
    /// <item>Number of webhooks per default event type</item>
    /// <item>Number of webhooks with custom events</item>
    /// <item>Number of webhooks with custom headers</item>
    /// </list>
    /// </summary>
    /// <returns>An <see cref="IEnumerable{UsageInformation}"/> containing webhook usage metrics.</returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        IWebhook[] allWebhooks = _webhookService.GetAllAsync(0, int.MaxValue).GetAwaiter().GetResult().Items.ToArray();

        yield return new UsageInformation(Constants.Telemetry.WebhookTotal, allWebhooks.Length);

        foreach (var eventType in _defaultEventTypes)
        {
            IWebhook[] webhooks = allWebhooks.Where(x => x.Events.Contains(eventType)).ToArray();
            yield return new UsageInformation($"{Constants.Telemetry.WebhookPrefix}{eventType}", webhooks.Length);
        }

        IEnumerable<IWebhook> customWebhooks = allWebhooks.Where(x => x.Events.Except(_defaultEventTypes).Any());
        yield return new UsageInformation(Constants.Telemetry.WebhookCustomEvent, customWebhooks.Count());

        IEnumerable<IWebhook> customHeaderWebhooks = allWebhooks.Where(x => x.Headers.Any());
        yield return new UsageInformation(Constants.Telemetry.WebhookCustomHeaders, customHeaderWebhooks.Count());
    }
}
