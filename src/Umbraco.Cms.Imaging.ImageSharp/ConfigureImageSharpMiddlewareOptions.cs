using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Configures the ImageSharp middleware options.
/// </summary>
/// <seealso cref="IConfigureOptions{ImageSharpMiddlewareOptions}" />
public sealed class ConfigureImageSharpMiddlewareOptions : IConfigureOptions<ImageSharpMiddlewareOptions>
{
    private readonly Configuration _configuration;
    private readonly ImagingSettings _imagingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureImageSharpMiddlewareOptions" /> class.
    /// </summary>
    /// <param name="configuration">The ImageSharp configuration.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    public ConfigureImageSharpMiddlewareOptions(Configuration configuration, IOptions<ImagingSettings> imagingSettings)
    {
        _configuration = configuration;
        _imagingSettings = imagingSettings.Value;
    }

    /// <inheritdoc />
    public void Configure(ImageSharpMiddlewareOptions options)
    {
        options.Configuration = _configuration;

        options.HMACSecretKey = _imagingSettings.HMACSecretKey;
        options.BrowserMaxAge = _imagingSettings.Cache.BrowserMaxAge;
        options.CacheMaxAge = _imagingSettings.Cache.CacheMaxAge;
        options.CacheHashLength = _imagingSettings.Cache.CacheHashLength;

        // Use configurable maximum width and height
        options.OnParseCommandsAsync = context =>
        {
            if (context.Commands.Count == 0 || _imagingSettings.HMACSecretKey.Length > 0)
            {
                // Nothing to parse or using HMAC authentication
                return Task.CompletedTask;
            }

            if (context.Commands.Contains(ResizeWebProcessor.Width))
            {
                if (!int.TryParse(context.Commands.GetValueOrDefault(ResizeWebProcessor.Width), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var width)
                || width < 0
                || width >= _imagingSettings.Resize.MaxWidth)
                {
                    context.Commands.Remove(ResizeWebProcessor.Width);
                }
            }

            if (context.Commands.Contains(ResizeWebProcessor.Height))
            {
                if (!int.TryParse(context.Commands.GetValueOrDefault(ResizeWebProcessor.Height), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var height)
                || height < 0
                || height >= _imagingSettings.Resize.MaxHeight)
                {
                    context.Commands.Remove(ResizeWebProcessor.Height);
                }
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
