using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    /// <inheritdoc />
    public class NodeCountTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly INodeCountService _nodeCountService;

        public NodeCountTelemetryProvider(INodeCountService nodeCountService) => _nodeCountService = nodeCountService;

        public IEnumerable<UsageInformation> GetInformation()
        {
            yield return new UsageInformation("MemberCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Member));
            yield return new UsageInformation("TemplateCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Template));
            yield return new UsageInformation("ContentCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Document));
            yield return new UsageInformation("DocumentTypeCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.DocumentType));
        }
    }
}
