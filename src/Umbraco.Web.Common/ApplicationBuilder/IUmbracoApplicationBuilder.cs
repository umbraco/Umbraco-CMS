using System;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    public interface IUmbracoApplicationBuilder
    {
        /// <summary>
        /// EXPERT/OPTIONAL call to replace the default middlewares that Umbraco installs by default.
        /// </summary>
        /// <param name="configureUmbracoMiddleware"></param>
        /// <returns></returns>
        IUmbracoApplicationBuilder WithCustomDefaultMiddleware(Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware);

        /// <summary>
        /// Called to include umbraco middleware
        /// </summary>
        /// <param name="configureUmbracoMiddleware"></param>
        /// <returns></returns>
        IUmbracoEndpointBuilder WithMiddleware(Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware);
    }
}
