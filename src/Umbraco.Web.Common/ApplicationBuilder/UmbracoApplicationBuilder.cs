using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

        public void WithEndpoints(Action<IUmbracoEndpointBuilder> configureUmbraco)
            => AppBuilder.UseEndpoints(endpoints =>
                {
                    var umbAppBuilder = (IUmbracoEndpointBuilder)ActivatorUtilities.CreateInstance<UmbracoEndpointBuilder>(
                        ApplicationServices,
                        new object[] { AppBuilder, endpoints });
                    configureUmbraco(umbAppBuilder);
                });
    }
}
