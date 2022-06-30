using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Web.Common.DependencyInjection;

/// <summary>
/// Configures the ImageSharp middleware options.
/// </summary>
/// <seealso cref="IConfigureOptions{ImageSharpMiddlewareOptions}" />
public sealed class ConfigureImageSharpMiddlewareOptions : IConfigureOptions<ImageSharpMiddlewareOptions>
{
    private readonly Configuration _configuration;
    private readonly ImagingSettings _imagingSettings;
    private readonly IImageUrlTokenGenerator _imageUrlTokenGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureImageSharpMiddlewareOptions" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    /// <param name="imageUrlTokenGenerator">The image URL token generator.</param>
    public ConfigureImageSharpMiddlewareOptions(Configuration configuration, IOptions<ImagingSettings> imagingSettings, IImageUrlTokenGenerator imageUrlTokenGenerator)
    {
        _configuration = configuration;
        _imagingSettings = imagingSettings.Value;
        _imageUrlTokenGenerator = imageUrlTokenGenerator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureImageSharpMiddlewareOptions" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    [Obsolete("Use ctor with all params - This will be removed in Umbraco 12.")]
    public ConfigureImageSharpMiddlewareOptions(Configuration configuration, IOptions<ImagingSettings> imagingSettings)
        : this(configuration, imagingSettings, StaticServiceProvider.Instance.GetRequiredService<IImageUrlTokenGenerator>())
    { }

    /// <inheritdoc />
    public void Configure(ImageSharpMiddlewareOptions options)
    {
        options.Configuration = _configuration;

        options.HMACSecretKey = _imagingSettings.HMACSecretKey;
        options.BrowserMaxAge = _imagingSettings.Cache.BrowserMaxAge;
        options.CacheMaxAge = _imagingSettings.Cache.CacheMaxAge;
        options.CacheHashLength = _imagingSettings.Cache.CacheHashLength;

        // Use the image URL token generator to compute the HMAC
        options.OnComputeHMACAsync = (context, _) =>
        {
            string imageUrl = UriHelper.BuildRelative(context.Context.Request.PathBase, context.Context.Request.Path);

            return Task.FromResult(_imageUrlTokenGenerator.GetImageUrlToken(imageUrl, context.Commands));
        };

        // Use configurable maximum width and height
        options.OnParseCommandsAsync = context =>
        {
            if (context.Commands.Count == 0 || _imagingSettings.HMACSecretKey?.Length > 0)
            {
                // Nothing to parse or using HMAC authentication
                return Task.CompletedTask;
            }

            int width = context.Parser.ParseValue<int>(context.Commands.GetValueOrDefault(ResizeWebProcessor.Width), context.Culture);
            if (width <= 0 || width > _imagingSettings.Resize.MaxWidth)
            {
                context.Commands.Remove(ResizeWebProcessor.Width);
            }

            int height = context.Parser.ParseValue<int>(context.Commands.GetValueOrDefault(ResizeWebProcessor.Height), context.Culture);
            if (height <= 0 || height > _imagingSettings.Resize.MaxHeight)
            {
                context.Commands.Remove(ResizeWebProcessor.Height);
            }

            return Task.CompletedTask;
        };

        // Change Cache-Control header when cache buster value is present
        options.OnPrepareResponseAsync = context =>
        {
            if (context.Request.Query.ContainsKey("rnd") || context.Request.Query.ContainsKey("v"))
            {
                ResponseHeaders headers = context.Response.GetTypedHeaders();

                CacheControlHeaderValue cacheControl = headers.CacheControl ?? new CacheControlHeaderValue()
                {
                    Public = true
                };

                // ImageSharp enables cache revalidation by default, so disable and add immutable directive
                cacheControl.MustRevalidate = false;
                cacheControl.Extensions.Add(new NameValueHeaderValue("immutable"));

                // Set updated value
                headers.CacheControl = cacheControl;
            }

            return Task.CompletedTask;
        };
    }
}
