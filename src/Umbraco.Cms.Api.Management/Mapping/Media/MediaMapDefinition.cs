using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Api.Management.Mapping.Media;

public class MediaMapDefinition : ContentMapDefinition<IMedia, MediaValueModel, MediaVariantResponseModel>, IMapDefinition
{
    public MediaMapDefinition(PropertyEditorCollection propertyEditorCollection)
        : base(propertyEditorCollection)
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IMedia, MediaResponseModel>((_, _) => new MediaResponseModel(), Map);

    // Umbraco.Code.MapAll -Urls
    private void Map(IMedia source, MediaResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.MediaType = context.Map<MediaTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source);
        target.Variants = MapVariantViewModels(source);
        target.IsTrashed = source.Trashed;
    }
}
