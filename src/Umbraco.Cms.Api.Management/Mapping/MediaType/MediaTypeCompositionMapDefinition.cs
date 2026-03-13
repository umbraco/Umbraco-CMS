using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.MediaType;

/// <summary>
/// Provides mapping definitions for media type compositions in the Umbraco CMS API.
/// </summary>
public class MediaTypeCompositionMapDefinition : IMapDefinition
{
    /// <summary>
    /// Defines the mapping configuration for media type compositions.
    /// </summary>
    /// <param name="mapper">The Umbraco mapper to configure mappings on.</param>
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IMediaType, MediaTypeCompositionResponseModel>(
            (_, _) => new MediaTypeCompositionResponseModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(IMediaType source, MediaTypeCompositionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Icon = source.Icon ?? string.Empty;
    }
}
