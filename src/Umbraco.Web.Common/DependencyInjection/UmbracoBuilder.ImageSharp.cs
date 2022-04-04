using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.ImageProcessors;

namespace Umbraco.Extensions
{
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Image Sharp with Umbraco settings
        /// </summary>
        public static IServiceCollection AddUmbracoImageSharp(this IUmbracoBuilder builder)
        {
            ImagingSettings imagingSettings = builder.Config.GetSection(Cms.Core.Constants.Configuration.ConfigImaging)
                .Get<ImagingSettings>() ?? new ImagingSettings();

            builder.Services.AddImageSharp(options =>
            {
                // options.Configuration is set using ImageSharpConfigurationOptions below
                options.BrowserMaxAge = imagingSettings.Cache.BrowserMaxAge;
                options.CacheMaxAge = imagingSettings.Cache.CacheMaxAge;
                options.CacheHashLength = imagingSettings.Cache.CacheHashLength;

                // Use configurable maximum width and height (overwrite ImageSharps default)
                options.OnParseCommandsAsync = context =>
                {
                    if (context.Commands.Count == 0)
                    {
                        return Task.CompletedTask;
                    }

                    uint width = context.Parser.ParseValue<uint>(context.Commands.GetValueOrDefault(ResizeWebProcessor.Width), context.Culture);
                    uint height = context.Parser.ParseValue<uint>(context.Commands.GetValueOrDefault(ResizeWebProcessor.Height), context.Culture);
                    if (width > imagingSettings.Resize.MaxWidth || height > imagingSettings.Resize.MaxHeight)
                    {
                        context.Commands.Remove(ResizeWebProcessor.Width);
                        context.Commands.Remove(ResizeWebProcessor.Height);
                    }

                    return Task.CompletedTask;
                };
                options.OnBeforeSaveAsync = _ => Task.CompletedTask;
                options.OnProcessedAsync = _ => Task.CompletedTask;
                options.OnPrepareResponseAsync = context =>
                {
                    // Change Cache-Control header when cache buster value is present
                    if (context.Request.Query.ContainsKey("rnd"))
                    {
                        var headers = context.Response.GetTypedHeaders();

                        var cacheControl = headers.CacheControl;
                        cacheControl.MustRevalidate = false;
                        cacheControl.Extensions.Add(new NameValueHeaderValue("immutable"));

                        headers.CacheControl = cacheControl;
                    }

                    return Task.CompletedTask;
                };
            })
                // Replace default image provider
                .ClearProviders().AddProvider<WebRootImageProvider>()
                // Configure cache options
                .Configure<PhysicalFileSystemCacheOptions>(options =>
                {
                    options.CacheFolder = builder.BuilderHostingEnvironment.MapPathContentRoot(imagingSettings.Cache.CacheFolder);
                    options.CacheFolderDepth = imagingSettings.Cache.CacheFolderDepth;
                })
                // Add custom processors
                .AddProcessor<CropWebProcessor>();

            // Configure middleware to use the registered/shared ImageSharp configuration
            builder.Services.AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, ImageSharpConfigurationOptions>();

            return builder.Services;
        }
    }
}
