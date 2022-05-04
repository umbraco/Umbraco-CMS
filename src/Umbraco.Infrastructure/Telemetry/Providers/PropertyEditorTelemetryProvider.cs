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
        foreach (IContentType contentType in contentTypes)
        {
            propertyTypes.UnionWith(contentType.PropertyTypes.Select(x => x.PropertyEditorAlias));
        }

        yield return new UsageInformation(Constants.Telemetry.Properties, propertyTypes);
    }
}
