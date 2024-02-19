using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Mapping.ContentType;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.MediaType;

public class MediaTypeMapDefinition : ContentTypeMapDefinition<IMediaType, MediaTypePropertyTypeResponseModel, MediaTypePropertyTypeContainerResponseModel>, IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMediaType, MediaTypeResponseModel>((_, _) => new MediaTypeResponseModel(), Map);
        mapper.Define<IMediaType, MediaTypeReferenceResponseModel>((_, _) => new MediaTypeReferenceResponseModel(), Map);
        mapper.Define<ISimpleContentType, MediaTypeReferenceResponseModel>((_, _) => new MediaTypeReferenceResponseModel(), Map);
        mapper.Define<IMediaType, AllowedMediaType>((_, _) => new AllowedMediaType(), Map);
        mapper.Define<ISimpleContentType, MediaTypeCollectionReferenceResponseModel>((_, _) => new MediaTypeCollectionReferenceResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
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
        target.Collection = source.ListView.ToReferenceByIdModel();

        target.Containers = MapPropertyTypeContainers(source);
        target.Properties = MapPropertyTypes(source);
        target.AllowedMediaTypes = source.AllowedContentTypes?.Select(ct =>
                new MediaTypeSort { MediaType = new ReferenceByIdModel(ct.Key), SortOrder = ct.SortOrder })
            .ToArray() ?? Enumerable.Empty<MediaTypeSort>();
        target.Compositions = source.ContentTypeComposition.Select(contentTypeComposition => new MediaTypeComposition
        {
            MediaType = new ReferenceByIdModel(contentTypeComposition.Key),
            CompositionType = CalculateCompositionType(source, contentTypeComposition)
        }).ToArray();
    }

    // Umbraco.Code.MapAll
    private void Map(IMediaType source, MediaTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Icon = source.Icon ?? string.Empty;
        target.Collection = source.ListView.ToReferenceByIdModel();
    }

    // Umbraco.Code.MapAll
    private void Map(ISimpleContentType source, MediaTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Icon = source.Icon ?? string.Empty;
        target.Collection = source.ListView.ToReferenceByIdModel();
    }

    // Umbraco.Code.MapAll
    private void Map(IMediaType source, AllowedMediaType target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Description = source.Description;
        target.Icon = source.Icon ?? string.Empty;
    }

    // Umbraco.Code.MapAll
    private void Map(ISimpleContentType source, MediaTypeCollectionReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Alias = source.Alias;
        target.Icon = source.Icon ?? string.Empty;
    }
}
