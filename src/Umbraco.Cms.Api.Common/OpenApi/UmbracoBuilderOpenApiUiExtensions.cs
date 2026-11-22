using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> to choose the UI that presents the OpenAPI documents.
/// </summary>
public static class UmbracoBuilderOpenApiUiExtensions
{
    /// <summary>
    /// Uses <typeparamref name="TProvider"/> to serve the OpenAPI UI, replacing any provider registered before.
    /// </summary>
    /// <typeparam name="TProvider">The UI provider.</typeparam>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> for chaining.</returns>
    public static IUmbracoBuilder AddOpenApiUi<TProvider>(this IUmbracoBuilder builder)
        where TProvider : class, IOpenApiUiProvider
    {
        builder.Services.AddUnique<IOpenApiUiProvider, TProvider>();
        return builder;
    }

    /// <summary>
    /// Uses <typeparamref name="TProvider"/> to serve the OpenAPI UI, unless a provider has been registered already.
    /// </summary>
    /// <remarks>
    /// Intended for a UI that is installed by default: it applies regardless of registration order, so a provider
    /// registered with <see cref="AddOpenApiUi{TProvider}"/>, before or after this call, takes precedence.
    /// </remarks>
    /// <typeparam name="TProvider">The UI provider.</typeparam>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> for chaining.</returns>
    public static IUmbracoBuilder TryAddOpenApiUi<TProvider>(this IUmbracoBuilder builder)
        where TProvider : class, IOpenApiUiProvider
    {
        builder.Services.TryAddSingleton<IOpenApiUiProvider, TProvider>();
        return builder;
    }
}
