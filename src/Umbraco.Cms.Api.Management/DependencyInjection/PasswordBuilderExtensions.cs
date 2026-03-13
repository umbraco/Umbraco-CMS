using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

/// <summary>
/// Provides extension methods to configure password-related services in the dependency injection container.
/// </summary>
public static class PasswordBuilderExtensions
{
    internal static IUmbracoBuilder AddPasswordConfiguration(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IPasswordConfigurationPresentationFactory, PasswordConfigurationPresentationFactory>();
        return builder;
    }
}
