using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder
{
    /// <summary>
    /// A builder to allow encapsulating the enabled endpoints in Umbraco
    /// </summary>
    public class UmbracoApplicationBuilder : IUmbracoApplicationBuilder, IUmbracoEndpointBuilder, IUmbracoApplicationBuilderContext
    {
        private Action<IUmbracoApplicationBuilderContext> _customMiddlewareRegistration;

        public UmbracoApplicationBuilder(IApplicationBuilder appBuilder)
        {
            ApplicationServices = appBuilder.ApplicationServices;
            RuntimeState = appBuilder.ApplicationServices.GetRequiredService<IRuntimeState>();
            AppBuilder = appBuilder ?? throw new ArgumentNullException(nameof(appBuilder));
            RegisterDefaultRequiredMiddleware = RegisterRequiredMiddleware;
        }

        public IServiceProvider ApplicationServices { get; }
        public IRuntimeState RuntimeState { get; }
        public IApplicationBuilder AppBuilder { get; }
        public Action RegisterDefaultRequiredMiddleware { get; set; }

        /// <inheritdoc />
        public IUmbracoApplicationBuilder WithCustomDefaultMiddleware(Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware)
        {
            _customMiddlewareRegistration = configureUmbracoMiddleware;
            return this;
        }

        /// <inheritdoc />
        public IUmbracoEndpointBuilder WithMiddleware(Action<IUmbracoApplicationBuilderContext> configureUmbracoMiddleware)
        {
            if (configureUmbracoMiddleware is null)
            {
                throw new ArgumentNullException(nameof(configureUmbracoMiddleware));
            }

            IOptions<UmbracoPipelineOptions> startupOptions = ApplicationServices.GetRequiredService<IOptions<UmbracoPipelineOptions>>();

            RunPrePipeline(startupOptions.Value);

            if (_customMiddlewareRegistration != null)
            {
                _customMiddlewareRegistration(this);
            }
            else
            {
                RegisterRequiredMiddleware();
            }

            RunPostPipeline(startupOptions.Value);

            configureUmbracoMiddleware(this);

            return this;
        }

        /// <inheritdoc />
        public void WithEndpoints(Action<IUmbracoEndpointBuilderContext> configureUmbraco)
        {
            IOptions<UmbracoPipelineOptions> startupOptions = ApplicationServices.GetRequiredService<IOptions<UmbracoPipelineOptions>>();
            RunPreEndpointsPipeline(startupOptions.Value);

            AppBuilder.UseEndpoints(endpoints =>
            {
                var umbAppBuilder = (IUmbracoEndpointBuilderContext)ActivatorUtilities.CreateInstance<UmbracoEndpointBuilder>(
                    ApplicationServices,
                    new object[] { AppBuilder, endpoints });
                configureUmbraco(umbAppBuilder);
            });
        }

        /// <summary>
        /// Registers the default required middleware to run Umbraco
        /// </summary>
        /// <param name="umbracoApplicationBuilderContext"></param>
        private void RegisterRequiredMiddleware()
        {
            AppBuilder.UseUmbracoCore();
            AppBuilder.UseUmbracoRequestLogging();

            // We need to add this before UseRouting so that the UmbracoContext and other middlewares are executed
            // before endpoint routing middleware.
            AppBuilder.UseUmbracoRouting();

            AppBuilder.UseStatusCodePages();

            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            // TODO: Since we are dependent on these we need to register them but what happens when we call this multiple times since we are dependent on this for UseUmbracoBackOffice too?
            AppBuilder.UseImageSharp();
            AppBuilder.UseStaticFiles();
            AppBuilder.UseUmbracoPlugins();

            // UseRouting adds endpoint routing middleware, this means that middlewares registered after this one
            // will execute after endpoint routing. The ordering of everything is quite important here, see
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0
            // where we need to have UseAuthentication and UseAuthorization proceeding this call but before
            // endpoints are defined.
            AppBuilder.UseRouting();
            AppBuilder.UseAuthentication();
            AppBuilder.UseAuthorization();

            // This must come after auth because the culture is based on the auth'd user
            AppBuilder.UseRequestLocalization();

            // Must be called after UseRouting and before UseEndpoints
            AppBuilder.UseSession();

            // DO NOT PUT ANY UseEndpoints declarations here!! Those must all come very last in the pipeline,
            // endpoints are terminating middleware. All of our endpoints are declared in ext of IUmbracoApplicationBuilder
        }

        private void RunPrePipeline(UmbracoPipelineOptions startupOptions)
        {
            foreach (IUmbracoPipelineFilter filter in startupOptions.PipelineFilters)
            {
                filter.OnPrePipeline(AppBuilder);
            }
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
