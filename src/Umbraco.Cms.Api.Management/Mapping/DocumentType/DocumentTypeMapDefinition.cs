using Umbraco.Cms.Api.Management.Mapping.ContentType;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.DocumentType;

public class DocumentTypeMapDefinition : ContentTypeMapDefinition<IContentType, DocumentTypePropertyTypeResponseModel, DocumentTypePropertyTypeContainerResponseModel>, IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IContentType, DocumentTypeResponseModel>((_, _) => new DocumentTypeResponseModel(), Map);
        mapper.Define<IContentType, DocumentTypeReferenceResponseModel>((_, _) => new DocumentTypeReferenceResponseModel(), Map);
        mapper.Define<ISimpleContentType, DocumentTypeReferenceResponseModel>((_, _) => new DocumentTypeReferenceResponseModel(), Map);
        mapper.Define<IContentType, AllowedDocumentType>((_, _) => new AllowedDocumentType(), Map);
        mapper.Define<ISimpleContentType, DocumentTypeCollectionReferenceResponseModel>((_, _) => new DocumentTypeCollectionReferenceResponseModel(), Map);
        mapper.Define<IContentEntitySlim, DocumentTypeReferenceResponseModel>((_, _) => new DocumentTypeReferenceResponseModel(), Map);
        mapper.Define<IDocumentEntitySlim, DocumentTypeReferenceResponseModel>((_, _) => new DocumentTypeReferenceResponseModel(), Map);
        mapper.Define<IContent, DocumentTypeBlueprintItemResponseModel>((_, _) => new DocumentTypeBlueprintItemResponseModel(), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IContentType source, DocumentTypeResponseModel target, MapperContext context)
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
        target.Collection = ReferenceByIdModel.ReferenceOrNull(source.ListView);
        target.Containers = MapPropertyTypeContainers(source);
        target.Properties = MapPropertyTypes(source);
        target.AllowedDocumentTypes = source.AllowedContentTypes?.Select(ct =>
                new DocumentTypeSort { DocumentType = new ReferenceByIdModel(ct.Key), SortOrder = ct.SortOrder })
            .OrderBy(ct => ct.SortOrder)
            .ToArray() ?? Enumerable.Empty<DocumentTypeSort>();
        target.Compositions = MapNestedCompositions(
            source.ContentTypeComposition,
            source.ParentId,
            (referenceByIdModel, compositionType) => new DocumentTypeComposition
            {
                DocumentType = referenceByIdModel,
                CompositionType = compositionType,
            });

        if (source.AllowedTemplates != null)
        {
            target.AllowedTemplates = source.AllowedTemplates.Select(template => new ReferenceByIdModel(template.Key));
        }

        target.DefaultTemplate = ReferenceByIdModel.ReferenceOrNull(source.DefaultTemplate?.Key);

        if (source.HistoryCleanup != null)
        {
            target.Cleanup = new DocumentTypeCleanup
            {
                PreventCleanup = source.HistoryCleanup.PreventCleanup,
                KeepAllVersionsNewerThanDays = source.HistoryCleanup.KeepAllVersionsNewerThanDays,
                KeepLatestVersionPerDayForDays = source.HistoryCleanup.KeepLatestVersionPerDayForDays
            };
        }
    }

    // Umbraco.Code.MapAll
    private void Map(IContentType source, DocumentTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Icon = source.Icon ?? string.Empty;
        target.Collection = ReferenceByIdModel.ReferenceOrNull(source.ListView);
    }

    // Umbraco.Code.MapAll
    private void Map(IContentEntitySlim source, DocumentTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.ContentTypeKey;
        target.Icon = source.ContentTypeIcon ?? string.Empty;
        target.Collection = ReferenceByIdModel.ReferenceOrNull(source.ListViewKey);
    }

    // Umbraco.Code.MapAll
    private void Map(IDocumentEntitySlim source, DocumentTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.ContentTypeKey;
        target.Icon = source.ContentTypeIcon ?? string.Empty;
        target.Collection = ReferenceByIdModel.ReferenceOrNull(source.ListViewKey);
    }

    // Umbraco.Code.MapAll
    private void Map(ISimpleContentType source, DocumentTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Icon = source.Icon ?? string.Empty;
        target.Collection = ReferenceByIdModel.ReferenceOrNull(source.ListView);
    }

    // Umbraco.Code.MapAll
    private void Map(IContentType source, AllowedDocumentType target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Description = source.Description;
        target.Icon = source.Icon ?? string.Empty;
    }

    // Umbraco.Code.MapAll
    private void Map(ISimpleContentType source, DocumentTypeCollectionReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Alias = source.Alias;
        target.Icon = source.Icon ?? string.Empty;
    }

    // Umbraco.Code.MapAll
    private void Map(IContent source, DocumentTypeBlueprintItemResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
    }
}
