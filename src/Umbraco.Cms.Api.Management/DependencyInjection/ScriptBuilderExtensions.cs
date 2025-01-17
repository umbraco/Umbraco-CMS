using Umbraco.Cms.Api.Management.Mapping.Script;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ScriptBuilderExtensions
{
    internal static IUmbracoBuilder AddScripts(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ScriptViewModelsMapDefinition>();

        return builder;
    }
}
