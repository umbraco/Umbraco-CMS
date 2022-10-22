using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.ImageProcessors;
using Umbraco.Cms.Web.Common.Media;

namespace Umbraco.Extensions;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Image Sharp with Umbraco settings
    /// </summary>
    public static IServiceCollection AddUmbracoImageSharp(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

        builder.Services.AddImageSharp()

            // Replace default image provider
            .ClearProviders()
            .AddProvider<WebRootImageProvider>()

            // Add custom processors
            .AddProcessor<CropWebProcessor>();

        // Configure middleware
        builder.Services
            .AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, ConfigureImageSharpMiddlewareOptions>();

        // Configure cache options
        builder.Services
            .AddTransient<IConfigureOptions<PhysicalFileSystemCacheOptions>, ConfigurePhysicalFileSystemCacheOptions>();

        return builder.Services;
    }
}
