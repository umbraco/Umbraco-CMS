using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IUmbracoBuilder"/> to register search-related services.
/// </summary>
public static class SearchBuilderExtensions
{
    /// <summary>
    /// Registers the services used by the search Management API endpoints.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The Umbraco builder.</returns>
    internal static IUmbracoBuilder AddSearch(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<ISearchPresentationFactory, SearchPresentationFactory>();
        return builder;
    }
}
