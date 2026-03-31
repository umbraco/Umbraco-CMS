using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data and metrics related to content entities within the Umbraco CMS.
/// </summary>
public class ContentTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Telemetry.Providers.ContentTelemetryProvider"/> class.
    /// </summary>
    /// <param name="contentService">The <see cref="IContentService"/> instance used to collect content-related telemetry data.</param>
    public ContentTelemetryProvider(IContentService contentService) => _contentService = contentService;

    /// <summary>
    /// Returns telemetry information about content, specifically the number of root content nodes in the system.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{UsageInformation}"/> containing the root content node count.
    /// </returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<IContent> rootNodes = _contentService.GetRootContent();
        var nodes = rootNodes.Count();
        yield return new UsageInformation(Constants.Telemetry.RootCount, nodes);
    }
}
