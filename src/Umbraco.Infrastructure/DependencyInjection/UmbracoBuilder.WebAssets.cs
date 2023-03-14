using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.WebAssets;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    internal static IUmbracoBuilder AddWebAssets(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<BackOfficeWebAssets>();
        return builder;
    }
}
