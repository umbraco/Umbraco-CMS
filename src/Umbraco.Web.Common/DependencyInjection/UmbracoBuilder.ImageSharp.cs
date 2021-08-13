using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Web.Common.ImageProcessors;
using Umbraco.Cms.Web.Common.Media;

namespace Umbraco.Extensions
{
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Image Sharp with Umbraco settings
        /// </summary>
        public static IServiceCollection AddUmbracoImageSharp(this IUmbracoBuilder builder)
        {
            IConfiguration configuration = builder.Config;
            IServiceCollection services = builder.Services;

            ImagingSettings imagingSettings = configuration.GetSection(Cms.Core.Constants.Configuration.ConfigImaging)
                .Get<ImagingSettings>() ?? new ImagingSettings();

            services.AddImageSharp(options =>
            {
                options.BrowserMaxAge = imagingSettings.Cache.BrowserMaxAge;
                options.CacheMaxAge = imagingSettings.Cache.CacheMaxAge;
                options.CachedNameLength = imagingSettings.Cache.CachedNameLength;

                // Use configurable maximum width and height (overwrite ImageSharps default)
                options.OnParseCommandsAsync = context =>
                {
                    if (context.Commands.Count == 0)
                    {
                        return Task.CompletedTask;
                    }

                    uint width = context.Parser.ParseValue<uint>(context.Commands.GetValueOrDefault(ResizeWebProcessor.Width), context.Culture);
                    uint height = context.Parser.ParseValue<uint>(context.Commands.GetValueOrDefault(ResizeWebProcessor.Height), context.Culture);
                    if (width > imagingSettings.Resize.MaxWidth && height > imagingSettings.Resize.MaxHeight)
                    {
                        context.Commands.Remove(ResizeWebProcessor.Width);
                        context.Commands.Remove(ResizeWebProcessor.Height);
                    }

                    return Task.CompletedTask;
                };
            })
                .Configure<PhysicalFileSystemCacheOptions>(options =>
                {
                    options.CacheFolder = imagingSettings.Cache.CacheFolder;
                })
                // We need to add CropWebProcessor before ResizeWebProcessor (until https://github.com/SixLabors/ImageSharp.Web/issues/182 is fixed)
                .RemoveProcessor<ResizeWebProcessor>()
                .AddProcessor<CropWebProcessor>()
                .AddProcessor<ResizeWebProcessor>();

            builder.Services.AddUnique<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

            return services;
        }
    }
}
