namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
/// The context object used when building the Umbraco application.
/// </summary>
/// <seealso cref="Umbraco.Cms.Web.Common.ApplicationBuilder.IUmbracoApplicationBuilderServices" />
public interface IUmbracoApplicationBuilderContext : IUmbracoApplicationBuilderServices
{
    /// <summary>
    /// Called to include the core Umbraco middlewares.
    /// </summary>
    void UseUmbracoCoreMiddleware();

    /// <summary>
    /// Manually runs the <see cref="IUmbracoPipelineFilter" /> pre pipeline filters.
    /// </summary>
    void RunPrePipeline();

    /// <summary>
    /// Manually runs the <see cref="IUmbracoPipelineFilter" /> pre routing filters.
    /// </summary>
    void RunPreRouting()
    {
        // TODO: Remove default implementation in Umbraco 13
    }

    /// <summary>
    /// Manually runs the <see cref="IUmbracoPipelineFilter" /> post routing filters.
    /// </summary>
    void RunPostRouting()
    {
        // TODO: Remove default implementation in Umbraco 13
    }

    /// <summary>
    /// Manually runs the <see cref="IUmbracoPipelineFilter" /> post pipeline filters.
    /// </summary>
    void RunPostPipeline();

    /// <summary>
    /// Called to include all of the default Umbraco required middleware.
    /// </summary>
    /// <remarks>
    /// If using this method, there is no need to use <see cref="UseUmbracoCoreMiddleware" />.
    /// </remarks>
    void RegisterDefaultRequiredMiddleware();
}
