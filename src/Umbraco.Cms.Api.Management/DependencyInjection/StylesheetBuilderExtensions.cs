using Umbraco.Cms.Api.Management.Mapping.Stylesheet;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class StylesheetBuilderExtensions
{
    internal static IUmbracoBuilder AddStylesheets(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<StylesheetViewModelsMapDefinition>();

        return builder;
    }
}
