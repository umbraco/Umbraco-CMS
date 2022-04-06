using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class MemberTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly INodeCountService _nodeCountService;
        public MemberTelemetryProvider(INodeCountService nodeCountService) => _nodeCountService = nodeCountService;

        public IEnumerable<UsageInformation> GetInformation()
        {
            var result = new List<UsageInformation>();

            result.Add(new UsageInformation("MemberNodes", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Member)));
            result.Add(new UsageInformation("ContentNodes", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Document)));
            result.Add(new UsageInformation("MediaNodes", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Media)));
            result.Add(new UsageInformation("TemplateNodes", _nodeCountService.GetNodeCount(Constants.ObjectTypes.Template)));
            result.Add(new UsageInformation("DocumentTypeNodes", _nodeCountService.GetNodeCount(Constants.ObjectTypes.DocumentType)));


            return null;
        }
    }
}
