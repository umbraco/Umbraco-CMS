using Umbraco.Cms.Api.Management.Mapping.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.MediaType;

public class MediaTypeMapDefinition : ContentTypeMapDefinition<IMediaType, MediaTypePropertyTypeViewModel, MediaTypePropertyTypeContainerViewModel>, IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IMediaType, MediaTypeViewModel>((_, _) => new MediaTypeViewModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(IMediaType source, MediaTypeViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Alias = source.Alias;
        target.Name = source.Name ?? string.Empty;
        target.Description = source.Description;
        target.Icon = source.Icon ?? string.Empty;
        target.AllowedAsRoot = source.AllowedAsRoot;
        target.VariesByCulture = source.VariesByCulture();
        target.VariesBySegment = source.VariesBySegment();
        target.IsElement = source.IsElement;
        target.Containers = MapPropertyTypeContainers(source);
        target.Properties = MapPropertyTypes(source);
        target.AllowedContentTypes = MapAllowedContentTypes(source);
        target.Compositions = MapCompositions(source, source.ContentTypeComposition);
    }
}
