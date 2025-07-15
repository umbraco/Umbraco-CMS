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
        builder.Services.AddTransient<IDocumentPresentationFactory, DocumentPresentationFactory>();
        builder.Services.AddTransient<IDocumentNotificationPresentationFactory, DocumentNotificationPresentationFactory>();
        builder.Services.AddTransient<IDocumentUrlFactory, DocumentUrlFactory>();
        builder.Services.AddTransient<IDocumentEditingPresentationFactory, DocumentEditingPresentationFactory>();
        builder.Services.AddTransient<IDocumentBlueprintEditingPresentationFactory, DocumentBlueprintEditingPresentationFactory>();
        builder.Services.AddTransient<IPublicAccessPresentationFactory, PublicAccessPresentationFactory>();
        builder.Services.AddTransient<IDomainPresentationFactory, DomainPresentationFactory>();
        builder.Services.AddTransient<IDocumentVersionPresentationFactory, DocumentVersionPresentationFactory>();
        builder.Services.AddTransient<IDocumentCollectionPresentationFactory, DocumentCollectionPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<DocumentMapDefinition>()
            .Add<DomainMapDefinition>()
            .Add<DocumentVersionMapDefinition>();

        return builder;
    }
}
