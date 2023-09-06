using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.TrackedReferences;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class TrackedReferencesBuilderExtensions
{
    internal static IUmbracoBuilder AddTrackedReferences(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IRelationPresentationFactory, RelationPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<TrackedReferenceViewModelsMapDefinition>()
            .Add<RelationModelMapDefinition>();

        return builder;
    }
}
