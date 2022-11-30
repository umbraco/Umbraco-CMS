using Microsoft.AspNetCore.Builder;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     Used to modify the <see cref="IApplicationBuilder" /> pipeline before and after Umbraco registers it's core
///     middlewares.
/// </summary>
/// <remarks>
///     Mainly used for package developers.
/// </remarks>
public interface IUmbracoPipelineFilter
{
    /// <summary>
    ///     The name of the filter
    /// </summary>
    /// <remarks>
    ///     This can be used by developers to see what is registered and if anything should be re-ordered, removed, etc...
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Executes before Umbraco middlewares are registered
    /// </summary>
    /// <param name="app"></param>
    void OnPrePipeline(IApplicationBuilder app);

    /// <summary>
    ///     Executes after core Umbraco middlewares are registered and before any Endpoints are declared
    /// </summary>
    /// <param name="app"></param>
    void OnPostPipeline(IApplicationBuilder app);

    /// <summary>
    ///     Executes after <see cref="OnPostPipeline(IApplicationBuilder)" /> just before any Umbraco endpoints are declared.
    /// </summary>
    /// <param name="app"></param>
    void OnEndpoints(IApplicationBuilder app);
}
