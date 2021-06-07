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
    public class UmbracoPipelineFilter : IUmbracoPipelineFilter
    {
        public UmbracoPipelineFilter(string name) : this(name, null, null, null, null) { }

        public UmbracoPipelineFilter(
            string name,
            Action<IApplicationBuilder> prePipeline,
            Action<IApplicationBuilder> postPipeline,
            Action<IApplicationBuilder> preEndpoints,
            Action<IUmbracoEndpointBuilder> endpoints)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PrePipeline = prePipeline;
            PostPipeline = postPipeline;
            PreEndpoints = preEndpoints;
            Endpoints = endpoints;
        }

        public Action<IApplicationBuilder> PrePipeline { get; set; }

        public Action<IApplicationBuilder> PostPipeline { get; set; }

        public Action<IApplicationBuilder> PreEndpoints { get; set; }

        public Action<IUmbracoEndpointBuilder> Endpoints { get; set; }

        public string Name { get; }

        public void OnPrePipeline(IApplicationBuilder app) => PrePipeline?.Invoke(app);

        public void OnPostPipeline(IApplicationBuilder app) => PostPipeline?.Invoke(app);

        public void OnPreEndpoints(IApplicationBuilder app) => PreEndpoints?.Invoke(app);

        public Action<IUmbracoEndpointBuilder> AppendEndpoints(Action<IUmbracoEndpointBuilder> configure)
        {
            if (Endpoints == null)
            {
                return configure;
            }

            return configure += Endpoints;
        }
    }
}
