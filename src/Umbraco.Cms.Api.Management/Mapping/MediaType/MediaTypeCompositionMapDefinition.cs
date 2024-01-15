using Umbraco.Cms.Api.Management.ViewModels.MediaType.Composition;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.MediaType;

public class MediaTypeCompositionMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IContentType, MediaTypeCompositionResponseModel>(
            (_, _) => new MediaTypeCompositionResponseModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(IContentType source, MediaTypeCompositionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Icon = source.Icon ?? string.Empty;
    }
}
