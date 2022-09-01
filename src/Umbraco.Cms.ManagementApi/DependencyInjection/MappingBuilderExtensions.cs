using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Mapping.Installer;
using Umbraco.Cms.ManagementApi.Mapping.Languages;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class MappingBuilderExtensions
{
    internal static IUmbracoBuilder AddMappers(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<LanguageViewModelsMapDefinition>()
            .Add<InstallerViewModelsMapDefinition>();

        return builder;
    }
}
