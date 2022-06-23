using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Middleware;

namespace Umbraco.Cms.Infrastructure.Imaging;

public interface IAdditionalImagingOptions
{
    RecyclableMemoryStreamManager MemoryStreamManager(ImageSharpMiddlewareOptions options);
    public Task OnParseCommandsAsync(ImageSharpMiddlewareOptions options, ImageCommandContext context);
    public Task OnPrepareResponseAsync(ImageSharpMiddlewareOptions options, HttpContext context);
    Task<string> OnComputeHMACAsync(ImageSharpMiddlewareOptions options, ImageCommandContext context, byte[] hmac);
    Task OnBeforeSaveAsync(ImageSharpMiddlewareOptions options, FormattedImage image);
    Task OnProcessedAsync(ImageSharpMiddlewareOptions options, ImageProcessingContext context);
}
