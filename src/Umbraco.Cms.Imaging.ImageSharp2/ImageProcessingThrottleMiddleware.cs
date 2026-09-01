using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Bounds the number of images processed concurrently.
/// </summary>
/// <remarks>
/// The imaging middleware only de-duplicates concurrent requests for the same URL, so a page of
/// distinct thumbnails decodes every source at full resolution in parallel. Peak memory is then
/// the number of concurrent requests multiplied by the size of a decoded source, which on a host
/// with a hard memory limit is enough to have the process killed. Requests over the limit wait
/// here instead. The gate engages only when memory is the binding constraint (see
/// <see cref="ImagingMemorySettings.RequiresConcurrencyLimit" />); on any other host it steps
/// aside so cache hits and other cheap requests are never made to wait.
/// </remarks>
public sealed class ImageProcessingThrottleMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SemaphoreSlim? _semaphore;
    private readonly HashSet<string> _commands;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingThrottleMiddleware" /> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    /// <param name="processors">The registered image processors, used to recognise processing requests.</param>
    public ImageProcessingThrottleMiddleware(
        RequestDelegate next,
        IOptions<ImagingSettings> imagingSettings,
        IEnumerable<IImageWebProcessor> processors)
        : this(
            next,
            imagingSettings,
            processors,
            GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            Environment.ProcessorCount)
    {
    }

    internal ImageProcessingThrottleMiddleware(
        RequestDelegate next,
        IOptions<ImagingSettings> imagingSettings,
        IEnumerable<IImageWebProcessor> processors,
        long availableMemoryBytes,
        int processorCount)
    {
        _next = next;

        ImagingMemorySettings memory = imagingSettings.Value.Memory;
        if (memory.RequiresConcurrencyLimit(availableMemoryBytes, processorCount))
        {
            var maximumConcurrentProcessing = memory.ResolveMaximumConcurrentProcessing(availableMemoryBytes, processorCount);
            _semaphore = new SemaphoreSlim(maximumConcurrentProcessing, maximumConcurrentProcessing);
        }

        _commands = new HashSet<string>(processors.SelectMany(x => x.Commands), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (_semaphore is null || !IsProcessingRequest(context.Request))
        {
            await _next(context);
            return;
        }

        await _semaphore.WaitAsync(context.RequestAborted);
        try
        {
            await _next(context);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool IsProcessingRequest(HttpRequest request)
    {
        // The image provider resolves a file, so anything without an extension cannot reach it.
        // Without this an unrelated request that happens to carry a "width" would queue here too.
        if (!Path.HasExtension(request.Path.Value))
        {
            return false;
        }

        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> query in request.Query)
        {
            if (_commands.Contains(query.Key))
            {
                return true;
            }
        }

        return false;
    }
}
