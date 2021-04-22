using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    /// <summary>
    /// A builder to allow encapsulating the enabled endpoints in Umbraco
    /// </summary>
    internal class UmbracoApplicationBuilder : IUmbracoApplicationBuilder
    {
        public UmbracoApplicationBuilder(IServiceProvider services, IRuntimeState runtimeState, IApplicationBuilder appBuilder)
        {
            ApplicationServices = services;
            RuntimeState = runtimeState;
            AppBuilder = appBuilder;
        }

        public IServiceProvider ApplicationServices { get; }
        public IRuntimeState RuntimeState { get; }
        public IApplicationBuilder AppBuilder { get; }

        public IUmbracoApplicationBuilder WithMiddleware(Action<IUmbracoMiddlewareBuilder> configureUmbraco)
        {
            IOptions<UmbracoPipelineOptions> startupOptions = ApplicationServices.GetRequiredService<IOptions<UmbracoPipelineOptions>>();
            RunPostPipeline(startupOptions.Value);

            configureUmbraco(this);

            return this;
        }

        public void WithEndpoints(Action<IUmbracoEndpointBuilder> configureUmbraco)
        {
            IOptions<UmbracoPipelineOptions> startupOptions = ApplicationServices.GetRequiredService<IOptions<UmbracoPipelineOptions>>();
            RunPreEndpointsPipeline(startupOptions.Value);

            AppBuilder.UseEndpoints(endpoints =>
            {
                var umbAppBuilder = (IUmbracoEndpointBuilder)ActivatorUtilities.CreateInstance<UmbracoEndpointBuilder>(
                    ApplicationServices,
                    new object[] { AppBuilder, endpoints });
                configureUmbraco(umbAppBuilder);
            });
        }

        private void RunPostPipeline(UmbracoPipelineOptions startupOptions)
        {
            foreach (IUmbracoPipelineFilter filter in startupOptions.PipelineFilters)
            {
                filter.OnPostPipeline(AppBuilder);
            }
        }

        private void RunPreEndpointsPipeline(UmbracoPipelineOptions startupOptions)
        {
            foreach (IUmbracoPipelineFilter filter in startupOptions.PipelineFilters)
            {
                filter.OnEndpoints(AppBuilder);
            }
        }
    }
}
