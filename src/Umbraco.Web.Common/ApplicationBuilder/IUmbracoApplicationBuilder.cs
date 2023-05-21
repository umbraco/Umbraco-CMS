namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

public interface IUmbracoApplicationBuilder
{
    /// <summary>
    ///     EXPERT call to replace the middlewares that Umbraco installs by default with a completely custom pipeline.
    /// </summary>
    /// <param name="configureUmbracoMiddleware"></param>
    /// <returns></returns>
    IUmbracoEndpointBuilder WithCustomMiddleware(Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware);

    /// <summary>
    ///     Called to include default middleware to run umbraco.
    /// </summary>
    /// <param name="configureUmbracoMiddleware"></param>
    /// <returns></returns>
    IUmbracoEndpointBuilder WithMiddleware(Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware);
}
