using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Imaging.ImageSharp.ImageProcessors;
using Umbraco.Cms.Imaging.ImageSharp.Media;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> to add ImageSharp image processing.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds ImageSharp image processing with Umbraco settings.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder" />.</param>
    /// <returns>The <see cref="IServiceCollection" />.</returns>
    public static IServiceCollection AddUmbracoImageSharp(this IUmbracoBuilder builder)
    {
        ImagingSettings imagingSettings = builder.Config
            .GetSection(Constants.Configuration.ConfigImaging)
            .Get<ImagingSettings>() ?? new ImagingSettings();

        ILogger logger = builder.BuilderLoggerFactory.CreateLogger("Umbraco.Cms.Imaging.ImageSharp");
        var availableMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        var availableMemoryMegabytes = availableMemoryBytes / 1024 / 1024;

        // ImageSharp pools unmanaged memory sized against the available memory and releases it only
        // on a gen2 collection, so on a memory constrained host it sits at rest well above what the
        // site needs, and it will decode a source of any size into that memory. Both are left to
        // the library on a host with room to spare. Applied before the configuration is shared so
        // nothing allocates from the default pool first.
        MemoryAllocatorOptions options = default;

        if (imagingSettings.Memory.RequiresPoolSizeLimit(availableMemoryBytes))
        {
            options.MaximumPoolSizeMegabytes = imagingSettings.Memory.ResolveMaximumPoolSizeMegabytes(availableMemoryBytes);
        }

        if (imagingSettings.Memory.RequiresAllocationLimit(availableMemoryBytes))
        {
            options.AllocationLimitMegabytes = imagingSettings.Memory.ResolveMaximumDecodedImageMegabytes(availableMemoryBytes);
        }

        if (options.MaximumPoolSizeMegabytes.HasValue || options.AllocationLimitMegabytes.HasValue)
        {
            // One allocator, shared process-wide, as the imaging library advises. Its documented
            // sample clones the configuration instead, but a clone would leave Configuration.Default
            // on its own allocator, so anything using that directly would pool separately. Assigned
            // once per host build, so a process building several - the test harness - replaces it
            // rather than accumulating them.
            // https://docs.sixlabors.com/articles/imagesharp/memorymanagement.html#customize-the-allocator
            Configuration.Default.MemoryAllocator = MemoryAllocator.Create(options);

            logger.LogInformation(
                "Bounded image processing memory with a {MaximumPoolSizeMegabytes} MB pool and a {MaximumDecodedImageMegabytes} MB ceiling per image, with {AvailableMemoryMegabytes} MB available to the process. A null bound is left to the imaging library.",
                options.MaximumPoolSizeMegabytes,
                options.AllocationLimitMegabytes,
                availableMemoryMegabytes);
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Left image processing memory to the imaging library, with {AvailableMemoryMegabytes} MB available to the process.",
                    availableMemoryMegabytes);
            }
        }

        // Add default ImageSharp configuration and service implementations
        builder.Services.AddSingleton(Configuration.Default);
        builder.Services.AddUnique<IImageDimensionExtractor, ImageSharpDimensionExtractor>();

        builder.Services.AddSingleton<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

        // Replaces the no-op IImageUrlTokenGenerator registered in Core; allows rich text render
        // paths to re-sign image URLs against the current HMACSecretKey after a key rotation.
        builder.Services.AddSingleton<IImageUrlTokenGenerator, ImageSharpImageUrlTokenGenerator>();

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
                PrePipeline = prePipeline =>
                {
                    prePipeline.UseMiddleware<ImageProcessingThrottleMiddleware>();
                    prePipeline.UseImageSharp();
                }
            });
        });

        return builder.Services;
    }
}
