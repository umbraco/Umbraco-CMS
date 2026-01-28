using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Document;

public class DocumentMapDefinition : ContentMapDefinition<IContent, DocumentValueResponseModel, DocumentVariantResponseModel>, IMapDefinition
{
    private readonly CommonMapper _commonMapper;

    public DocumentMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        CommonMapper commonMapper,
        IDataValueEditorFactory dataValueEditorFactory)
        : base(propertyEditorCollection, dataValueEditorFactory)
        => _commonMapper = commonMapper;

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 18.")]
    public DocumentMapDefinition(
        PropertyEditorCollection propertyEditorCollection,
        CommonMapper commonMapper)
        : this(
            propertyEditorCollection,
            commonMapper,
            StaticServiceProvider.Instance.GetRequiredService<IDataValueEditorFactory>())
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IContent, DocumentResponseModel>((_, _) => new DocumentResponseModel(), Map);
        mapper.Define<IContent, PublishedDocumentResponseModel>((_, _) => new PublishedDocumentResponseModel(), Map);
        mapper.Define<IContent, DocumentCollectionResponseModel>((_, _) => new DocumentCollectionResponseModel(), Map);
        mapper.Define<IContent, DocumentBlueprintResponseModel>((_, _) => new DocumentBlueprintResponseModel(), Map);
        mapper.Define<ContentScheduleCollection, DocumentResponseModel>(Map);
    }

    // Umbraco.Code.MapAll -Urls -Template -Flags
    private void Map(IContent source, DocumentResponseModel target, MapperContext context)
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

    // Umbraco.Code.MapAll -Urls -Template -Flags
    private void Map(IContent source, PublishedDocumentResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.DocumentType = context.Map<DocumentTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source.Properties, published: true);
        target.Variants = MapVariantViewModels(
            source,
            (culture, _, documentVariantViewModel) =>
            {
                documentVariantViewModel.Name = source.GetPublishName(culture) ?? documentVariantViewModel.Name;
                DocumentVariantState variantState = DocumentVariantStateHelper.GetState(source, culture);
                documentVariantViewModel.State = variantState == DocumentVariantState.PublishedPendingChanges
                        ? DocumentVariantState.Published
                        : variantState;
                documentVariantViewModel.PublishDate = culture == null
                    ? source.PublishDate
                    : source.GetPublishDate(culture);
            });
        target.IsTrashed = source.Trashed;
    }

    // Umbraco.Code.MapAll -IsProtected -Ancestors -Flags
    private void Map(IContent source, DocumentCollectionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.DocumentType = context.Map<DocumentTypeCollectionReferenceResponseModel>(source.ContentType)!;
        target.SortOrder = source.SortOrder;
        target.Creator = _commonMapper.GetOwnerName(source, context);
        target.Updater = _commonMapper.GetCreatorName(source, context);
        target.IsTrashed = source.Trashed;

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


    // Umbraco.Code.MapAll -Flags
    private void Map(IContent source, DocumentBlueprintResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.DocumentType = context.Map<DocumentTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source.Properties);
        target.Variants = MapVariantViewModels(
            source,
            (culture, _, documentVariantViewModel) =>
            {
                documentVariantViewModel.State = DocumentVariantState.Draft;
            });
    }

    private void Map(ContentScheduleCollection source, DocumentResponseModel target, MapperContext context)
        => MapContentScheduleCollection<DocumentResponseModel, DocumentVariantResponseModel>(source, target, context);
}
