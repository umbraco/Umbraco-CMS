using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class MediaTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly INodeCountService _nodeCountService;

    public MediaTelemetryProvider(INodeCountService nodeCountService) => _nodeCountService = nodeCountService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        yield return new UsageInformation(Constants.Telemetry.MediaCount, _nodeCountService.GetMediaCount());
    }
}
