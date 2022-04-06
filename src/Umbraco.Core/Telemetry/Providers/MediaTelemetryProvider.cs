using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class MediaTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly INodeCountService _nodeCountService;

        public MediaTelemetryProvider(INodeCountService nodeCountService) => _nodeCountService = nodeCountService;

        public IEnumerable<UsageInformation> GetInformation()
        {
            yield return new UsageInformation("MediaCount", _nodeCountService.GetMediaCount());
        }
    }
}
