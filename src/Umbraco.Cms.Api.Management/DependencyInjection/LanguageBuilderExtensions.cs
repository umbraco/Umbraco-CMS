using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Mapping.Culture;
using Umbraco.Cms.Api.Management.Mapping.Languages;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Core.Services.Languages;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class LanguageBuilderExtensions
{
    internal static IUmbracoBuilder AddLanguages(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<ILanguageService, LanguageService>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<LanguageViewModelsMapDefinition>()
            .Add<CultureViewModelMapDefinition>();

        return builder;
    }
}
