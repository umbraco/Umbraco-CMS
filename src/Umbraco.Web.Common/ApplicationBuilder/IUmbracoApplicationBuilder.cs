using System;
using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    public interface IUmbracoApplicationBuilder : IUmbracoMiddlewareBuilder
    {
        /// <summary>
        /// Called to include umbraco middleware
        /// </summary>
        /// <param name="configureUmbraco"></param>
        /// <returns></returns>
        IUmbracoApplicationBuilder WithMiddleware(Action<IUmbracoMiddlewareBuilder> configureUmbraco);

        /// <summary>
        /// Final call during app building to configure endpoints
        /// </summary>
        /// <param name="configureUmbraco"></param>
        void WithEndpoints(Action<IUmbracoEndpointBuilder> configureUmbraco);
    }
}
