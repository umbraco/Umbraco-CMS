using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Mapping.Culture;
using Umbraco.Cms.ManagementApi.Mapping.Dictionary;
using Umbraco.Cms.ManagementApi.Mapping.Installer;
using Umbraco.Cms.ManagementApi.Mapping.Languages;
using Umbraco.Cms.ManagementApi.Mapping.Relation;
using Umbraco.Cms.ManagementApi.Mapping.TrackedReferences;
using Umbraco.New.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

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
            .Add<CultureViewModelMapDefinition>();

        return builder;
    }
}
