using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class ContentTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IContentService _contentService;
        private readonly INodeCountService _nodeCountService;

        public ContentTelemetryProvider(IContentService contentService, INodeCountService nodeCountService)
        {
            _contentService = contentService;
            _nodeCountService = nodeCountService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            var rootNodes = _contentService.GetRootContent();

            int nodes = rootNodes.Count();

            var result = new List<UsageInformation>();
            result.Add(new("RootCount", nodes));
            return result;
        }
    }
}
