using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class MediaTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly INodeCountService _nodeCountService;

        public MediaTelemetryProvider(INodeCountService nodeCountService)
        {
            _nodeCountService = nodeCountService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            var result = new List<UsageInformation>();

            result.Add(new UsageInformation("MediaCount", _nodeCountService.GetMediaCount()));

            return result;
        }
    }
}
