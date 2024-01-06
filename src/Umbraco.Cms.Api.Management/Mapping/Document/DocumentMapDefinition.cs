using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Api.Management.Mapping.Document;

public class DocumentMapDefinition : ContentMapDefinition<IContent, DocumentValueModel, DocumentVariantResponseModel>, IMapDefinition
{
    public DocumentMapDefinition(PropertyEditorCollection propertyEditorCollection)
        : base(propertyEditorCollection)
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IContent, DocumentResponseModel>((_, _) => new DocumentResponseModel(), Map);

    // Umbraco.Code.MapAll -Urls -Template
    private void Map(IContent source, DocumentResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.DocumentType = context.Map<DocumentTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source);
        target.Variants = MapVariantViewModels(
            source,
            (culture, _, documentVariantViewModel) =>
            {
                documentVariantViewModel.State = ContentStateHelper.GetContentState(source, culture);
                documentVariantViewModel.PublishDate = culture == null
                    ? source.PublishDate
                    : source.GetPublishDate(culture);
            });
        target.IsTrashed = source.Trashed;
    }
}
