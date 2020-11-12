using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using Smidge;
using Smidge.Nuglify;
using Umbraco.Core.Configuration.Models;
using Umbraco.Web.Common.ApplicationModels;

namespace Umbraco.Extensions
{
    public static class UmbracoWebServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the web components needed for Umbraco
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoWebComponents(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureOptions<UmbracoMvcConfigureOptions>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, UmbracoApiBehaviorApplicationModelProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, BackOfficeApplicationModelProvider>());
            services.AddUmbracoImageSharp(configuration);

            return services;
        }

        /// <summary>
        /// Adds Image Sharp with Umbraco settings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoImageSharp(this IServiceCollection services, IConfiguration configuration)
        {
            var imagingSettings = configuration.GetSection(Core.Constants.Configuration.ConfigImaging)
                .Get<ImagingSettings>() ?? new ImagingSettings();

            services.AddImageSharp(options =>
                    {
                        options.Configuration = SixLabors.ImageSharp.Configuration.Default;
                        options.BrowserMaxAge = imagingSettings.Cache.BrowserMaxAge;
                        options.CacheMaxAge = imagingSettings.Cache.CacheMaxAge;
                        options.CachedNameLength = imagingSettings.Cache.CachedNameLength;
                        options.OnParseCommandsAsync = context =>
                        {
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Width, imagingSettings.Resize.MaxWidth);
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Height, imagingSettings.Resize.MaxHeight);

                            return Task.CompletedTask;
                        };
                        options.OnBeforeSaveAsync = _ =>  Task.CompletedTask;
                        options.OnProcessedAsync = _ => Task.CompletedTask;
                        options.OnPrepareResponseAsync = _ => Task.CompletedTask;
                    })
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetMemoryAllocator(provider => ArrayPoolMemoryAllocator.CreateWithMinimalPooling())
                .Configure<PhysicalFileSystemCacheOptions>(options =>
                {
                    options.CacheFolder = imagingSettings.Cache.CacheFolder;
                })
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .AddProvider<PhysicalFileSystemProvider>()
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>();

            return services;
        }

        /// <summary>
        /// Adds the Umbraco runtime minifier
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoRuntimeMinifier(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSmidge(configuration.GetSection(Core.Constants.Configuration.ConfigRuntimeMinification));
            services.AddSmidgeNuglify();

            return services;
        }

        private static void RemoveIntParamenterIfValueGreatherThen(IDictionary<string, string> commands, string parameter, int maxValue)
        {
            if (commands.TryGetValue(parameter, out var command))
            {
                if (int.TryParse(command, out var i))
                {
                    if (i > maxValue)
                    {
                        commands.Remove(parameter);
                    }
                }
            }
        }

        /// <summary>
        /// Options for globally configuring MVC for Umbraco
        /// </summary>
        /// <remarks>
        /// We generally don't want to change the global MVC settings since we want to be unobtrusive as possible but some
        /// global mods are needed - so long as they don't interfere with normal user usages of MVC.
        /// </remarks>
        private class UmbracoMvcConfigureOptions : IConfigureOptions<MvcOptions>
        {

            // TODO: we can inject params with DI here
            public UmbracoMvcConfigureOptions()
            {
            }

            // TODO: we can configure global mvc options here if we need to
            public void Configure(MvcOptions options)
            {

            }
        }


    }

}
