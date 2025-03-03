using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Media;

public class MediaMapDefinition : ContentMapDefinition<IMedia, MediaValueResponseModel, MediaVariantResponseModel>, IMapDefinition
{
    private readonly CommonMapper _commonMapper;

    public MediaMapDefinition(PropertyEditorCollection propertyEditorCollection, CommonMapper commonMapper)
        : base(propertyEditorCollection)
        => _commonMapper = commonMapper;

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMedia, MediaResponseModel>((_, _) => new MediaResponseModel(), Map);
        mapper.Define<IMedia, MediaCollectionResponseModel>((_, _) => new MediaCollectionResponseModel(), Map);
    }

    // Umbraco.Code.MapAll -Urls
    private void Map(IMedia source, MediaResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.MediaType = context.Map<MediaTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source.Properties);
        target.Variants = MapVariantViewModels(source);
        target.IsTrashed = source.Trashed;
    }

    // Umbraco.Code.MapAll
    private void Map(IMedia source, MediaCollectionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.MediaType = context.Map<MediaTypeCollectionReferenceResponseModel>(source.ContentType)!;
        target.SortOrder = source.SortOrder;
        target.Creator = _commonMapper.GetOwnerName(source, context);

        // If there's a set of property aliases specified in the collection configuration, we will check if the current property's
        // value should be mapped. If it isn't one of the ones specified in 'includeProperties', we will just return the result
        // without mapping the value.
        var includedProperties = context.GetIncludedProperties();

        IEnumerable<IProperty> properties = source.Properties;
        if (includedProperties is not null)
        {
            properties = properties.Where(property => includedProperties.Contains(property.Alias));
        }

        target.Values = MapValueViewModels(properties);
        target.Variants = MapVariantViewModels(source);
    }
}
