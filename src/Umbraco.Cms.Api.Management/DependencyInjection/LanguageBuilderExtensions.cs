using Umbraco.Cms.Api.Management.Mapping.Culture;
using Umbraco.Cms.Api.Management.Mapping.Language;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class LanguageBuilderExtensions
{
    internal static IUmbracoBuilder AddLanguages(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<LanguageViewModelsMapDefinition>()
            .Add<CultureViewModelMapDefinition>();

        return builder;
    }
}
