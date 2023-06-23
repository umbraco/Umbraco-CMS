using Umbraco.Cms.Api.Management.Mapping.DocumentType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class DocumentTypeBuilderExtensions
{
    internal static IUmbracoBuilder AddDocumentTypes(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<DocumentTypeMapDefinition>();

        return builder;
    }
}
