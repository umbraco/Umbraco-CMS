using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data collection and reporting functionality for property editors within Umbraco.
/// </summary>
public class PropertyEditorTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyEditorTelemetryProvider"/> class, which provides telemetry data related to property editors.
    /// </summary>
    /// <param name="contentTypeService">The service used to access and manage content types, which is required for collecting property editor telemetry.</param>
    public PropertyEditorTelemetryProvider(IContentTypeService contentTypeService) =>
        _contentTypeService = contentTypeService;

    /// <summary>
    /// Gathers and returns telemetry usage statistics about property editors used across all content types.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{UsageInformation}"/> containing statistics such as the set of property editor aliases in use, the total number of properties, the highest property count on a single content type, and the total number of compositions.
    /// </returns>
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
        yield return new UsageInformation(Constants.Telemetry.TotalPropertyCount, propertyTypeCounts.Sum());
        yield return new UsageInformation(Constants.Telemetry.HighestPropertyCount, propertyTypeCounts.Count > 0 ? propertyTypeCounts.Max() : 0);
        yield return new UsageInformation(Constants.Telemetry.TotalCompositions, totalCompositions);
    }
}
