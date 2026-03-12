using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <inheritdoc />
public class NodeCountTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly INodeCountService _nodeCountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeCountTelemetryProvider"/> class, which provides telemetry data related to node counts in the Umbraco CMS.
    /// </summary>
    /// <param name="nodeCountService">The service used to retrieve node count information for telemetry purposes.</param>
    public NodeCountTelemetryProvider(INodeCountService nodeCountService) => _nodeCountService = nodeCountService;

    /// <summary>
    /// Retrieves telemetry usage information about the number of nodes for various Umbraco object types.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="Umbraco.Cms.Infrastructure.Telemetry.UsageInformation"/> instances, each representing the count of a specific node type (such as members, templates, content, and document types) in the system.
    /// </returns>
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
