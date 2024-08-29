using Umbraco.Cms.Api.Management.Mapping.Import;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ImportBuilderExtensions
{
    internal static IUmbracoBuilder AddImport(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<EntityImportAnalysisViewModelsMapDefinition>();

        return builder;
    }
}
