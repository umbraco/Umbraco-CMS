using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Mapping.TrackedReferences;
using Umbraco.New.Cms.Core.Persistence.Repositories;
using Umbraco.New.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.New.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class RepositoryBuilderExtensions
{
    internal static IUmbracoBuilder AddRepositories(this IUmbracoBuilder builder)
    {
        builder.Services.AddScoped<ITrackedReferencesSkipTakeRepository, TrackedReferencesSkipTakeRepository>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<TrackedReferenceViewModelsMapDefinition>()
            .Add<RelationModelMapDefinition>();

        return builder;
    }

}
