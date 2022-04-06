using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class ContentTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IContentService _contentService;

        public ContentTelemetryProvider(IContentService contentService) => _contentService = contentService;

        public IEnumerable<UsageInformation> GetInformation()
        {
            var rootNodes = _contentService.GetRootContent();

            int nodes = rootNodes.Count();

            var result = new List<UsageInformation>();
            result.Add(new("Root nodes", nodes));
            return result;
        }
    }
}
