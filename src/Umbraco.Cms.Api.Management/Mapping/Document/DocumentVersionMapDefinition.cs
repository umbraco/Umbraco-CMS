using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Document;

/// <summary>
/// Provides mapping configuration for converting document version entities between different models or representations within the API.
/// </summary>
public class DocumentVersionMapDefinition : ContentMapDefinition<IContent, DocumentValueResponseModel, DocumentVariantResponseModel>, IMapDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentVersionMapDefinition"/> class, used for mapping document versions with the specified property editors and data value editor factory.
    /// </summary>
    /// <param name="propertyEditorCollection">A collection containing the available property editors used for mapping document properties.</param>
    /// <param name="dataValueEditorFactory">A factory responsible for creating data value editors for property values.</param>
    public DocumentVersionMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        IDataValueEditorFactory dataValueEditorFactory)
        : base(propertyEditorCollection, dataValueEditorFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentVersionMapDefinition"/> class.
    /// </summary>
    /// <param name="propertyEditorCollection">The collection of property editors used for mapping document versions.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 18.")]
    public DocumentVersionMapDefinition(
        PropertyEditorCollection propertyEditorCollection)
        : this(
            propertyEditorCollection,
            StaticServiceProvider.Instance.GetRequiredService<IDataValueEditorFactory>())
    {
    }

    /// <summary>
    /// Configures object-object mappings for converting document content entities to their version response models.
    /// </summary>
    /// <param name="mapper">The mapper instance used to define the mapping rules.</param>
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
