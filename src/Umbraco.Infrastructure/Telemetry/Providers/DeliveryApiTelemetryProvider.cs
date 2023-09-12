using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class DeliveryApiTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly DeliveryApiSettings _deliveryApiSettings;

    public DeliveryApiTelemetryProvider(IOptions<DeliveryApiSettings> deliveryApiSettings)
    {
        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    public IEnumerable<UsageInformation> GetInformation()
    {
        yield return new UsageInformation(Constants.Telemetry.DeliverApiEnabled, _deliveryApiSettings.Enabled);
        yield return new UsageInformation(Constants.Telemetry.DeliveryApiPublicAccess, _deliveryApiSettings.PublicAccess);
    }
}
