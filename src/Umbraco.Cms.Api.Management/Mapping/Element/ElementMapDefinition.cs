using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Api.Management.Mapping.Element;

public class ElementMapDefinition : ContentMapDefinition<IElement, ElementValueResponseModel, ElementVariantResponseModel>, IMapDefinition
{
    public ElementMapDefinition(PropertyEditorCollection propertyEditorCollection)
        : base(propertyEditorCollection)
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IElement, ElementResponseModel>((_, _) => new ElementResponseModel(), Map);
        mapper.Define<ContentScheduleCollection, ElementResponseModel>(Map);
    }

    // Umbraco.Code.MapAll -Flags
    private void Map(IElement source, ElementResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.DocumentType = context.Map<DocumentTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source.Properties);
        target.Variants = MapVariantViewModels(
            source,
            (culture, _, documentVariantViewModel) =>
            {
                documentVariantViewModel.State = DocumentVariantStateHelper.GetState(source, culture);
                documentVariantViewModel.PublishDate = culture == null
                    ? source.PublishDate
                    : source.GetPublishDate(culture);
            });
        target.IsTrashed = source.Trashed;
    }

    private void Map(ContentScheduleCollection source, ElementResponseModel target, MapperContext context)
        => MapContentScheduleCollection<ElementResponseModel, ElementVariantResponseModel>(source, target, context);
}
