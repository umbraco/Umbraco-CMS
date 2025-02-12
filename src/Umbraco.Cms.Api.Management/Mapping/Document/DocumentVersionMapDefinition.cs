using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Document;

public class DocumentVersionMapDefinition : ContentMapDefinition<IContent, DocumentValueResponseModel, DocumentVariantResponseModel>, IMapDefinition
{
    public DocumentVersionMapDefinition(PropertyEditorCollection propertyEditorCollection)
        : base(propertyEditorCollection)
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IContent, DocumentVersionResponseModel>((_, _) => new DocumentVersionResponseModel(), Map);
    }

    private void Map(IContent source, DocumentVersionResponseModel target, MapperContext context)
    {
        target.Id = source.VersionId.ToGuid(); // this is a magic guid since versions do not have Guids in the DB
        target.Document = new ReferenceByIdModel(source.Key);
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
