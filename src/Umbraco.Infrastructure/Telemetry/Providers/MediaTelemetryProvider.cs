using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data about media items, including their usage and performance metrics, within the Umbraco CMS.
/// </summary>
public class MediaTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly INodeCountService _nodeCountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTelemetryProvider"/> class.
    /// </summary>
    /// <param name="nodeCountService">The service used to count media nodes.</param>
    public MediaTelemetryProvider(INodeCountService nodeCountService) => _nodeCountService = nodeCountService;

    /// <summary>
    /// Retrieves telemetry information about media usage, specifically the total count of media items.
    /// </summary>
    /// <returns>
    /// An enumerable collection containing a <see cref="UsageInformation"/> instance representing the total number of media items.
    /// </returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        yield return new UsageInformation(Constants.Telemetry.MediaCount, _nodeCountService.GetMediaCount());
    }
}
