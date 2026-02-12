using Umbraco.Cms.Api.Management.ViewModels.Segment;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Segment;

public class SegmentMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<Core.Models.Segment, SegmentResponseModel>(
        (_, _) => new SegmentResponseModel { Name = string.Empty, Alias = string.Empty, Cultures = null },
        Map);

    // Umbraco.Code.MapAll
    private static void Map(Core.Models.Segment source, SegmentResponseModel target, MapperContext context)
    {
        target.Name = source.Name;
        target.Alias = source.Alias;
        target.Cultures = source.Cultures;
    }
}
