using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class PropertyEditorTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IContentTypeService _contentTypeService;

    public PropertyEditorTelemetryProvider(IContentTypeService contentTypeService) =>
        _contentTypeService = contentTypeService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        IEnumerable<IContentType> contentTypes = _contentTypeService.GetAll();
        var propertyTypes = new HashSet<string>();
        var propertyTypeCounts = new List<int>();
        var totalCompositions = 0;

        foreach (IContentType contentType in contentTypes)
        {
            propertyTypes.UnionWith(contentType.PropertyTypes.Select(x => x.PropertyEditorAlias));
            propertyTypeCounts.Add(contentType.CompositionPropertyTypes.Count());
            totalCompositions += contentType.CompositionAliases().Count();
        }

        yield return new UsageInformation(Constants.Telemetry.Properties, propertyTypes);
        yield return new UsageInformation("TotalPropertyCount", propertyTypeCounts.Sum());
        yield return new UsageInformation("HighestPropertyCount", propertyTypeCounts.Max());
        yield return new UsageInformation("TotalCompositions", totalCompositions);
    }
}
