using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Element;

/// <summary>
/// Provides mapping configuration for converting element version entities between different models or representations within the API.
/// </summary>
public class ElementVersionMapDefinition : ContentMapDefinition<IElement, ElementValueResponseModel, ElementVariantResponseModel>, IMapDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementVersionMapDefinition"/> class, used for mapping element versions with the specified property editors and data value editor factory.
    /// </summary>
    /// <param name="propertyEditorCollection">A collection containing the available property editors used for mapping element properties.</param>
    /// <param name="dataValueEditorFactory">A factory responsible for creating data value editors for property values.</param>
    public ElementVersionMapDefinition(PropertyEditorCollection propertyEditorCollection, IDataValueEditorFactory dataValueEditorFactory)
        : base(propertyEditorCollection, dataValueEditorFactory)
    {
    }

    /// <summary>
    /// Configures object-object mappings for converting element content entities to their version response models.
    /// </summary>
    /// <param name="mapper">The mapper instance used to define the mapping rules.</param>
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
