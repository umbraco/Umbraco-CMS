using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
///     Configures the ImageSharp middleware options.
/// </summary>
/// <seealso cref="IConfigureOptions{ImageSharpMiddlewareOptions}" />
public sealed class ConfigureImageSharpMiddlewareOptions : IConfigureOptions<ImageSharpMiddlewareOptions>
{
    private readonly Configuration _configuration;
    private readonly ImagingSettings _imagingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureImageSharpMiddlewareOptions" /> class.
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

        options.BrowserMaxAge = _imagingSettings.Cache.BrowserMaxAge;
        options.CacheMaxAge = _imagingSettings.Cache.CacheMaxAge;
        options.CacheHashLength = _imagingSettings.Cache.CacheHashLength;

        // Use configurable maximum width and height
        options.OnParseCommandsAsync = context =>
        {
            if (context.Commands.Count == 0)
            {
                return Task.CompletedTask;
            }

            if (context.Commands.Contains(ResizeWebProcessor.Width))
            {
                if (!int.TryParse(
                        context.Commands.GetValueOrDefault(ResizeWebProcessor.Width),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var width)
                    || width < 0
                    || width >= _imagingSettings.Resize.MaxWidth)
                {
                    context.Commands.Remove(ResizeWebProcessor.Width);
                }
            }

            if (context.Commands.Contains(ResizeWebProcessor.Height))
            {
                if (!int.TryParse(
                        context.Commands.GetValueOrDefault(ResizeWebProcessor.Height),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var height)
                    || height < 0
                    || height >= _imagingSettings.Resize.MaxHeight)
                {
                    context.Commands.Remove(ResizeWebProcessor.Height);
                }
            }

            return Task.CompletedTask;
        };

        // The decoded image has been disposed by the time this runs, and the cache write and the
        // response that follow hold only the encoded result, so the place a gated request took in
        // ImageProcessingThrottleMiddleware is given back here rather than when the request ends. The
        // middleware releases again at the end of the request, which is a no-op after this, and
        // covers a request that faulted before getting here.
        Func<ImageProcessingContext, Task> onProcessedAsync = options.OnProcessedAsync;
        options.OnProcessedAsync = async context =>
        {
            await onProcessedAsync(context);

            if (context.Context.Items.TryGetValue(ImageProcessingSlot.HttpContextItemKey, out var value)
                && value is ImageProcessingSlot slot)
            {
                slot.Release();
            }
        };

        // Change Cache-Control header when cache buster value is present
        options.OnPrepareResponseAsync = context =>
        {
            if (context.Request.Query.ContainsKey("rnd") || context.Request.Query.ContainsKey("v"))
            {
                ResponseHeaders headers = context.Response.GetTypedHeaders();

                CacheControlHeaderValue cacheControl =
                    headers.CacheControl ?? new CacheControlHeaderValue { Public = true };
                cacheControl.MustRevalidate = false; // ImageSharp enables this by default
                cacheControl.Extensions.Add(new NameValueHeaderValue("immutable"));

                headers.CacheControl = cacheControl;
            }

            return Task.CompletedTask;
        };
    }
}
