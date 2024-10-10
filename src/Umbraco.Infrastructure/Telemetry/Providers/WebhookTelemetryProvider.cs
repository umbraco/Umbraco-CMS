using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class WebhookTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IWebhookService _webhookService;

    public WebhookTelemetryProvider(IWebhookService webhookService) => _webhookService = webhookService;

    private readonly string[] _defaultEventTypes =
        [
            "Umbraco.ContentDelete",
            "Umbraco.ContentPublish",
            "Umbraco.ContentUnpublish",
            "Umbraco.MediaDelete",
            "Umbraco.MediaSave"
        ];

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
