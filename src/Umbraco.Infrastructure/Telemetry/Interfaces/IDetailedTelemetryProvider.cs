using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

internal interface IDetailedTelemetryProvider
{
    IEnumerable<UsageInformation> GetInformation();
}
