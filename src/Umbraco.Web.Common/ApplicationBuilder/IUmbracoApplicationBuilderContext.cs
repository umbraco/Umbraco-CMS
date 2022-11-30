namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     The context object used during
/// </summary>
public interface IUmbracoApplicationBuilderContext : IUmbracoApplicationBuilderServices
{
    /// <summary>
    ///     Called to include the core umbraco middleware.
    /// </summary>
    void UseUmbracoCoreMiddleware();

    /// <summary>
    ///     Manually runs the <see cref="IUmbracoPipelineFilter" /> pre pipeline filters
    /// </summary>
    void RunPrePipeline();

    /// <summary>
    ///     Manually runs the <see cref="IUmbracoPipelineFilter " /> post pipeline filters
    /// </summary>
    void RunPostPipeline();

    /// <summary>
    ///     Called to include all of the default umbraco required middleware.
    /// </summary>
    /// <remarks>
    ///     If using this method, there is no need to use <see cref="UseUmbracoCoreMiddleware" />
    /// </remarks>
    void RegisterDefaultRequiredMiddleware();
}
