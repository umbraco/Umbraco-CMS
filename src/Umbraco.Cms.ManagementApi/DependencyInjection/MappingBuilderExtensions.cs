using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Mapping.Culture;
using Umbraco.Cms.ManagementApi.Mapping.Installer;
using Umbraco.Cms.ManagementApi.Mapping.Languages;
using Umbraco.Cms.ManagementApi.Mapping.Relation;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class MappingBuilderExtensions
{
    internal static IUmbracoBuilder AddMappers(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<RelationViewModelsMapDefinition>()
            .Add<LanguageViewModelsMapDefinition>()
            .Add<InstallerViewModelsMapDefinition>()
            .Add<CultureViewModelMapDefinition>();

        return builder;
    }
}
