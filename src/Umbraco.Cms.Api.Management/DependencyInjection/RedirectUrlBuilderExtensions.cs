using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class RedirectUrlBuilderExtensions
{
    internal static IUmbracoBuilder AddRedirectUrl(this IUmbracoBuilder builder)
    {
        builder.Services
            .AddTransient<IRedirectUrlStatusPresentationFactory, RedirectUrlStatusPresentationFactory>()
            .AddTransient<IRedirectUrlPresentationFactory, RedirectUrlPresentationFactory>();

        return builder;
    }
}
