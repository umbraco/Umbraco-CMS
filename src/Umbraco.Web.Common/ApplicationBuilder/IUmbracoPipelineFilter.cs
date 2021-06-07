using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    /// <summary>
    /// Used to modify the <see cref="IApplicationBuilder"/> pipeline before and after Umbraco registers it's core middlewares.
    /// </summary>
    /// <remarks>
    /// Mainly used for package developers.
    /// </remarks>
    public interface IUmbracoPipelineFilter
    {
        /// <summary>
        /// The name of the filter
        /// </summary>
        /// <remarks>
        /// This can be used by developers to see what is registered and if anything should be re-ordered, removed, etc...
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Executes before Umbraco middlewares are registered
        /// </summary>
        void OnPrePipeline(IApplicationBuilder app);

        /// <summary>
        /// Executes after core Umbraco middlewares are registered and before any Endpoints are declared
        /// </summary>
        void OnPostPipeline(IApplicationBuilder app);

        /// <summary>
        /// Executes after <see cref="OnPostPipeline(IApplicationBuilder)"/> just before any Umbraco endpoints are declared.
        /// </summary>
        void OnPreEndpoints(IApplicationBuilder app);

        /// <summary>
        /// Appends registered endpoints to default Umbraco endpoint configuration.
        /// </summary>
        /// <returns></returns>
        Action<IUmbracoEndpointBuilder> AppendEndpoints(Action<IUmbracoEndpointBuilder> configure);
    }
}
