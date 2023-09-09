using Microsoft.AspNetCore.Builder;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
/// Used to modify the <see cref="IApplicationBuilder" /> pipeline before and after Umbraco registers its middlewares.
/// </summary>
/// <remarks>
/// Mainly used for package developers.
/// </remarks>
public interface IUmbracoPipelineFilter
{
    /// <summary>
    /// The name of the filter.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    /// <remarks>
    /// This can be used by developers to see what is registered and if anything should be re-ordered, removed, etc...
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Executes before any default Umbraco middlewares are registered.
    /// </summary>
    /// <param name="app">The application.</param>
    void OnPrePipeline(IApplicationBuilder app);

    /// <summary>
    /// Executes after static files middlewares are registered and just before the routing middleware is registered.
    /// </summary>
    /// <param name="app">The application.</param>
    void OnPreRouting(IApplicationBuilder app)
    {
        // TODO: Remove default implementation in Umbraco 13
    }

    /// <summary>
    /// Executes after the routing middleware is registered and just before the authentication and authorization middlewares are registered. This can be used to add CORS policies.
    /// </summary>
    /// <param name="app">The application.</param>
    void OnPostRouting(IApplicationBuilder app)
    {
        // TODO: Remove default implementation in Umbraco 13
    }

    /// <summary>
    /// Executes after core Umbraco middlewares are registered and before any endpoints are declared.
    /// </summary>
    /// <param name="app">The application.</param>
    void OnPostPipeline(IApplicationBuilder app);

    /// <summary>
    /// Executes after the middlewares are registered and just before any Umbraco endpoints are declared.
    /// </summary>
    /// <param name="app">The application.</param>
    void OnEndpoints(IApplicationBuilder app);
}
