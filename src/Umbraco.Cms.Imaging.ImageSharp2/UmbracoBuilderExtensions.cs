using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Imaging.ImageSharp.ImageProcessors;
using Umbraco.Cms.Imaging.ImageSharp.Media;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Imaging.ImageSharp;

public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Image Sharp with Umbraco settings
    /// </summary>
    public static IServiceCollection AddUmbracoImageSharp(this IUmbracoBuilder builder)
    {
        // Add default ImageSharp configuration and service implementations
        builder.Services.AddSingleton(Configuration.Default);
        builder.Services.AddUnique<IImageDimensionExtractor, ImageSharpDimensionExtractor>();

        builder.Services.AddSingleton<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

        builder.Services.AddImageSharp()
            // Replace default image provider
            .ClearProviders()
            .AddProvider<WebRootImageProvider>()
            // Add custom processors
            .AddProcessor<CropWebProcessor>();

        // Configure middleware
        builder.Services.AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, ConfigureImageSharpMiddlewareOptions>();

        // Configure cache options
        builder.Services.AddTransient<IConfigureOptions<PhysicalFileSystemCacheOptions>, ConfigurePhysicalFileSystemCacheOptions>();

        // Important we handle image manipulations before the static files, otherwise the querystring is just ignored
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(nameof(ImageSharpComposer))
            {
                PrePipeline = prePipeline => prePipeline.UseImageSharp()
            });
        });

        return builder.Services;
    }
}
