using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Imaging;

public class AdditionalImagingOptions : IAdditionalImagingOptions
{
    private readonly ImagingSettings _imagingSettings;

    public AdditionalImagingOptions(IOptions<ImagingSettings> imagingSettings) => _imagingSettings = imagingSettings.Value;

    public RecyclableMemoryStreamManager MemoryStreamManager(ImageSharpMiddlewareOptions options) => options.MemoryStreamManager;

    public Task OnParseCommandsAsync(ImageSharpMiddlewareOptions options, ImageCommandContext context)
    {
        if (context.Commands.Count == 0)
        {
            return Task.CompletedTask;
        }

        // Use configurable maximum width and height
        var width = context.Parser.ParseValue<int>(
            context.Commands.GetValueOrDefault(ResizeWebProcessor.Width),
            context.Culture);
        if (width <= 0 || width > _imagingSettings.Resize.MaxWidth)
        {
            context.Commands.Remove(ResizeWebProcessor.Width);
        }

        var height = context.Parser.ParseValue<int>(
            context.Commands.GetValueOrDefault(ResizeWebProcessor.Height),
            context.Culture);
        if (height <= 0 || height > _imagingSettings.Resize.MaxHeight)
        {
            context.Commands.Remove(ResizeWebProcessor.Height);
        }

        return Task.CompletedTask;
    }

    public Task OnPrepareResponseAsync(ImageSharpMiddlewareOptions options, HttpContext context)
    {
        // Change Cache-Control header when cache buster value is present
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
    }


    public Task<string> OnComputeHMACAsync(ImageSharpMiddlewareOptions options, ImageCommandContext context, byte[] hmac) => options.OnComputeHMACAsync(context, hmac);

    public Task OnBeforeSaveAsync(ImageSharpMiddlewareOptions options, FormattedImage image) => options.OnBeforeSaveAsync(image);

    public Task OnProcessedAsync(ImageSharpMiddlewareOptions options, ImageProcessingContext context) => options.OnProcessedAsync(context);
}
