using Umbraco.Cms.Api.Management.Mapping.ContentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.DocumentType;

public class DocumentTypeMapDefinition : ContentTypeMapDefinition<IContentType, DocumentTypePropertyTypeViewModel, DocumentTypePropertyTypeContainerViewModel>, IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IContentType, DocumentTypeViewModel>((_, _) => new DocumentTypeViewModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(IContentType source, DocumentTypeViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Alias = source.Alias;
        target.Name = source.Name ?? string.Empty;
        target.Description = source.Description;
        target.AllowedAsRoot = source.AllowedAsRoot;
        target.VariesByCulture = source.VariesByCulture();
        target.IsElement = source.IsElement;
        target.Containers = MapPropertyTypeContainers(source);
        target.Properties = MapPropertyTypes(source);
    }
}
