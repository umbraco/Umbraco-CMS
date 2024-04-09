using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.DocumentType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class DocumentTypeBuilderExtensions
{
    internal static IUmbracoBuilder AddDocumentTypes(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IDocumentTypeEditingPresentationFactory, DocumentTypeEditingPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<DocumentTypeMapDefinition>();
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<DocumentTypeCompositionMapDefinition>();

        return builder;
    }
}
