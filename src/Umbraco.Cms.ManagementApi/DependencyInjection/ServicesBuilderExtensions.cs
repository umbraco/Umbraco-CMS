using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Core.Services.Languages;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class ServicesBuilderExtensions
{
    internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<ILanguageService, LanguageService>();
        return builder;
    }

}
