using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.DocumentType;

/// <summary>
/// Provides mapping configuration for composing document types within the Umbraco CMS management API.
/// </summary>
public class DocumentTypeCompositionMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures mappings for document type compositions using the provided mapper.
    /// </summary>
    /// <param name="mapper">The mapper used to define the mapping between <see cref="IContentType"/> and <see cref="DocumentTypeCompositionResponseModel"/>.</param>
    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IContentType, DocumentTypeCompositionResponseModel>(
            (_, _) => new DocumentTypeCompositionResponseModel(), Map);

    // Umbraco.Code.MapAll
    private static void Map(IContentType source, DocumentTypeCompositionResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Icon = source.Icon ?? string.Empty;
    }
}
