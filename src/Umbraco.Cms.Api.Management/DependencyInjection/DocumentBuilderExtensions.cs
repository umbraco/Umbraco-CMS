using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Document;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class DocumentBuilderExtensions
{
    internal static IUmbracoBuilder AddDocuments(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IDocumentViewModelFactory, DocumentViewModelFactory>();
        builder.Services.AddTransient<IContentUrlFactory, ContentUrlFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<DocumentMapDefinition>();

        return builder;
    }
}
