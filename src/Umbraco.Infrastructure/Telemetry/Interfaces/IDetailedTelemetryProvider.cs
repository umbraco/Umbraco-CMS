using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

/// <summary>
/// Represents a provider that supplies detailed telemetry information for the application.
/// </summary>
public interface IDetailedTelemetryProvider
{
    /// <summary>
    /// Retrieves detailed telemetry usage information.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="Umbraco.Cms.Infrastructure.Telemetry.UsageInformation"/> objects containing usage data.</returns>
    IEnumerable<UsageInformation> GetInformation();
}
