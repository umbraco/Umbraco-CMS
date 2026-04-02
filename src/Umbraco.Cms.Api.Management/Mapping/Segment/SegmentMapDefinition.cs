using Umbraco.Cms.Api.Management.ViewModels.Segment;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Mapping.Segment;

/// <summary>
/// Provides mapping configuration for segments within the Umbraco CMS Management API.
/// </summary>
public class SegmentMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures object-object mappings between <see cref="Core.Models.Segment"/> and <see cref="SegmentResponseModel"/>.
    /// This method defines how segment data from the core model is transformed into the API response model.
    /// </summary>
    /// <param name="mapper">The mapper instance used to register the mapping definitions.</param>
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
