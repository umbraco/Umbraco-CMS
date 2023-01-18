using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Relation;
using Umbraco.Cms.Api.Management.Mapping.TrackedReferences;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.New.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class TrackedReferencesBuilderExtensions
{
    internal static IUmbracoBuilder AddTrackedReferences(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IRelationViewModelFactory, RelationViewModelFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<TrackedReferenceViewModelsMapDefinition>()
            .Add<RelationModelMapDefinition>()
            .Add<RelationViewModelsMapDefinition>();

        return builder;
    }
}
