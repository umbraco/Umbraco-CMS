using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <inheritdoc />
public class NodeCountTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly INodeCountService _nodeCountService;

    public NodeCountTelemetryProvider(INodeCountService nodeCountService) => _nodeCountService = nodeCountService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        yield return new UsageInformation(
            Constants.Telemetry.MemberCount,
            _nodeCountService.GetNodeCount(Constants.ObjectTypes.Member));
        yield return new UsageInformation(
            Constants.Telemetry.TemplateCount,
            _nodeCountService.GetNodeCount(Constants.ObjectTypes.Template));
        yield return new UsageInformation(
            Constants.Telemetry.ContentCount,
            _nodeCountService.GetNodeCount(Constants.ObjectTypes.Document));
        yield return new UsageInformation(
            Constants.Telemetry.DocumentTypeCount,
            _nodeCountService.GetNodeCount(Constants.ObjectTypes.DocumentType));
    }
}
