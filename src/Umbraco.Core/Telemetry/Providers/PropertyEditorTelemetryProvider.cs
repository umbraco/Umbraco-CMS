using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class PropertyEditorTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IContentTypeService _contentTypeService;
        public PropertyEditorTelemetryProvider(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            var result = new List<UsageInformation>();
            var contentTypes = _contentTypeService.GetAll();
            var propertyTypes = new HashSet<IPropertyType>();
            foreach (IContentType contentType in contentTypes)
            {
                propertyTypes.UnionWith(contentType.PropertyTypes);
            }

            result.Add(new UsageInformation("Properties", propertyTypes.Select(x => x.Name)));
            return result;
        }
    }
}
