using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers
{
    public class ContentTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IContentService _contentService;

        public ContentTelemetryProvider(IContentService contentService) => _contentService = contentService;

        public IEnumerable<UsageInformation> GetInformation()
        {
            var rootNodes = _contentService.GetRootContent();
            int nodes = rootNodes.Count();
            yield return new UsageInformation(Constants.Telemetry.RootCount, nodes);
        }
    }
}
