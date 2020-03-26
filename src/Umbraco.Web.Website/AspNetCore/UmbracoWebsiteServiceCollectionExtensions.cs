using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.Memory;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Website.AspNetCore
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoWebsite(this IServiceCollection services)
        {
            // TODO: We need to avoid this, surely there's a way? See ContainerTests.BuildServiceProvider_Before_Host_Is_Configured
            var serviceProvider = services.BuildServiceProvider();
            var configs = serviceProvider.GetService<Configs>();
            var imagingSettings = configs.Imaging();
            services.AddUmbracoImageSharp(imagingSettings);

            return services;
        }

        public static IServiceCollection AddUmbracoImageSharp(this IServiceCollection services, IImagingSettings imagingSettings)
        {


            services.AddImageSharpCore(
                    options =>
                    {
                        options.Configuration = SixLabors.ImageSharp.Configuration.Default;
                        options.MaxBrowserCacheDays = imagingSettings.MaxBrowserCacheDays;
                        options.MaxCacheDays = imagingSettings.MaxCacheDays;
                        options.CachedNameLength = imagingSettings.CachedNameLength;
                        options.OnParseCommands = context =>
                        {
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Width, imagingSettings.MaxResizeWidth);
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Height, imagingSettings.MaxResizeHeight);
                        };
                        options.OnBeforeSave = _ => { };
                        options.OnProcessed = _ => { };
                        options.OnPrepareResponse = _ => { };
                    })
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetMemoryAllocator(provider => ArrayPoolMemoryAllocator.CreateWithMinimalPooling())
                .Configure<PhysicalFileSystemCacheOptions>(options =>
                {
                    options.CacheFolder = imagingSettings.CacheFolder;
                })
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .AddProvider<PhysicalFileSystemProvider>()
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>();

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
    }

}
