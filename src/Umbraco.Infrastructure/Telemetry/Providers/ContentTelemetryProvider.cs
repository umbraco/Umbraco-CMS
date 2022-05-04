using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class ContentTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IContentService _contentService;

    public ContentTelemetryProvider(IContentService contentService) => _contentService = contentService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<IContent> rootNodes = _contentService.GetRootContent();
        var nodes = rootNodes.Count();
        yield return new UsageInformation(Constants.Telemetry.RootCount, nodes);
    }
}
