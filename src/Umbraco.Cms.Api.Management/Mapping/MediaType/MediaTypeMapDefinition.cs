using Umbraco.Cms.Api.Management.Mapping.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.MediaType;

public class MediaTypeMapDefinition : ContentTypeMapDefinition<IMediaType, MediaTypePropertyTypeResponseModel, MediaTypePropertyTypeContainerResponseModel>, IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IMediaType, MediaTypeResponseModel>((_, _) => new MediaTypeResponseModel(), Map);

    // Todo: ParentId
    // Umbraco.Code.MapAll -ParentId
    private void Map(IMediaType source, MediaTypeResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
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
