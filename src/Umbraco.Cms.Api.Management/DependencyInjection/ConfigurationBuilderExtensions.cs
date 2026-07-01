using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

/// <summary>
/// Provides extension methods to configure dependency injection for the Umbraco CMS API Management.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    internal static IUmbracoBuilder AddConfigurationFactories(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IConfigurationPresentationFactory, ConfigurationPresentationFactory>();

        return builder;
    }
}
