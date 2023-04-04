using Umbraco.Cms.Api.Management.Mapping.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.DocumentType;

public class DocumentTypeMapDefinition : ContentTypeMapDefinition<IContentType, DocumentTypePropertyTypeResponseModel, DocumentTypePropertyTypeContainerResponseModel>, IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IContentType, DocumentTypeResponseModel>((_, _) => new DocumentTypeResponseModel(), Map);

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
        target.Containers = MapPropertyTypeContainers(source);
        target.Properties = MapPropertyTypes(source);
        target.AllowedContentTypes = MapAllowedContentTypes(source);
        target.Compositions = MapCompositions(source, source.ContentTypeComposition);

        if (source.AllowedTemplates != null)
        {
            target.AllowedTemplateIds = source.AllowedTemplates.Select(template => template.Key);
        }

        target.DefaultTemplateId = source.DefaultTemplate?.Key;

        if (source.HistoryCleanup != null)
        {
            target.Cleanup = new ContentTypeCleanup
            {
                PreventCleanup = source.HistoryCleanup.PreventCleanup,
                KeepAllVersionsNewerThanDays = source.HistoryCleanup.KeepAllVersionsNewerThanDays,
                KeepLatestVersionPerDayForDays = source.HistoryCleanup.KeepLatestVersionPerDayForDays
            };
        }
    }
}
