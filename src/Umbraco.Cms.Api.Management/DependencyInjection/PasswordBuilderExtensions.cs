using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class PasswordBuilderExtensions
{
    internal static IUmbracoBuilder AddPasswordConfiguration(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IPasswordConfigurationPresentationFactory, PasswordConfigurationPresentationFactory>();
        return builder;
    }
}
