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

namespace Umbraco.Web.Website.AspNetCore
{
    public static class UmbracoBackOfficeServiceCollectionExtensions
    {
        public static IServiceCollection AddUmbracoWebsite(this IServiceCollection services)
        {
            services.AddUmbracoImageSharp();

            return services;
        }

        public static IServiceCollection AddUmbracoImageSharp(this IServiceCollection services)
        {
            services.AddImageSharpCore(
                    options =>
                    {
                        options.Configuration = SixLabors.ImageSharp.Configuration.Default;
                        options.MaxBrowserCacheDays = 7;
                        options.MaxCacheDays = 365;
                        options.CachedNameLength = 8;
                        options.OnParseCommands = context =>
                        {
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Width, 5000);
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Height, 5000);
                        };
                        options.OnBeforeSave = _ => { };
                        options.OnProcessed = _ => { };
                        options.OnPrepareResponse = _ => { };
                    })
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetMemoryAllocator(provider => ArrayPoolMemoryAllocator.CreateWithMinimalPooling())
                .Configure<PhysicalFileSystemCacheOptions>(options =>
                {
                    options.CacheFolder = "../app_data/cache";
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
