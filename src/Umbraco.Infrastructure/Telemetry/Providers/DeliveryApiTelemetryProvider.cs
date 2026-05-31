using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry services by collecting and reporting data related to the Delivery API.
/// </summary>
public class DeliveryApiTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly DeliveryApiSettings _deliveryApiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryApiTelemetryProvider"/> class.
    /// </summary>
    /// <param name="deliveryApiSettings">The <see cref="IOptions{DeliveryApiSettings}"/> instance containing the delivery API settings, typically provided via dependency injection.</param>
    public DeliveryApiTelemetryProvider(IOptions<DeliveryApiSettings> deliveryApiSettings)
    {
        _deliveryApiSettings = deliveryApiSettings.Value;
    }

    /// <summary>
    /// Retrieves telemetry usage information about the current Delivery API configuration.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="UsageInformation"/> objects, each representing a specific Delivery API setting and its current value.
    /// </returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        yield return new UsageInformation(Constants.Telemetry.DeliverApiEnabled, _deliveryApiSettings.Enabled);
        yield return new UsageInformation(Constants.Telemetry.DeliveryApiPublicAccess, _deliveryApiSettings.PublicAccess);
    }
}
