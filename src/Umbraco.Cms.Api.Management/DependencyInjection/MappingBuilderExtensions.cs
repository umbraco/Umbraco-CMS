using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Api.Management.Mapping.Culture;
using Umbraco.Cms.Api.Management.Mapping.Dictionary;
using Umbraco.Cms.Api.Management.Mapping.HealthCheck;
using Umbraco.Cms.Api.Management.Mapping.Installer;
using Umbraco.Cms.Api.Management.Mapping.Languages;
using Umbraco.Cms.Api.Management.Mapping.Relation;
using Umbraco.Cms.Api.Management.Mapping.TrackedReferences;
using Umbraco.New.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class MappingBuilderExtensions
{
    internal static IUmbracoBuilder AddMappers(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<DictionaryViewModelMapDefinition>()
            .Add<TrackedReferenceViewModelsMapDefinition>()
            .Add<RelationModelMapDefinition>()
            .Add<RelationViewModelsMapDefinition>()
            .Add<LanguageViewModelsMapDefinition>()
            .Add<InstallerViewModelsMapDefinition>()
            .Add<CultureViewModelMapDefinition>()
            .Add<HealthCheckViewModelsMapDefinition>();

        return builder;
    }
}
