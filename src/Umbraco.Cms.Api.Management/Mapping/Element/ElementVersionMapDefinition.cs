using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Element;

public class ElementVersionMapDefinition : ContentMapDefinition<IElement, ElementValueResponseModel, ElementVariantResponseModel>, IMapDefinition
{
    public ElementVersionMapDefinition(PropertyEditorCollection propertyEditorCollection, IDataValueEditorFactory dataValueEditorFactory)
        : base(propertyEditorCollection, dataValueEditorFactory)
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IElement, ElementVersionResponseModel>((_, _) => new ElementVersionResponseModel(), Map);

    // Umbraco.Code.MapAll -Flags
    private void Map(IElement source, ElementVersionResponseModel target, MapperContext context)
    {
        target.Id = source.VersionId.ToGuid(); // this is a magic guid since versions do not have Guids in the DB
        target.Element = new ReferenceByIdModel(source.Key);
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
    }
}
