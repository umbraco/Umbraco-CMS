using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

public interface IDetailedTelemetryProvider
{
    IEnumerable<UsageInformation> GetInformation();
}
