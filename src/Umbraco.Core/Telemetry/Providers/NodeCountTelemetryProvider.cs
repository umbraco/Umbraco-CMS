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
            var result = new List<UsageInformation>();

            result.Add(new UsageInformation("MemberCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Member)));
            result.Add(new UsageInformation("ContentCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Document)));
            result.Add(new UsageInformation("MediaCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Media)));
            result.Add(new UsageInformation("TemplateCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Template)));
            result.Add(new UsageInformation("DocumentTypeCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.DocumentType)));
            result.Add(new UsageInformation("LanguageCount", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Language)));

            return result;
        }
    }
}
