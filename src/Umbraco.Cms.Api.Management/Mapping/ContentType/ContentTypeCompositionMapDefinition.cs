using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.ContentType;

public class ContentTypeCompositionMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IContentType, DocumentTypeCompositionResponseModel>(
            (_, _) => new DocumentTypeCompositionResponseModel(), Map);

    // TODO: Consider abstracting this mapping
    // Umbraco.Code.MapAll
    private static void Map(IContentType source, DocumentTypeCompositionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Icon = source.Icon ?? string.Empty;
    }
}
