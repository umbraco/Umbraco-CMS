using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Bounds the number of images processed concurrently.
/// </summary>
/// <remarks>
/// <para>
/// The imaging middleware only de-duplicates concurrent requests for the same URL, so a page of
/// distinct thumbnails decodes every source at full resolution in parallel. Peak memory is then
/// the number of concurrent requests multiplied by the size of a decoded source, which on a host
/// with a hard memory limit is enough to have the process killed. Requests over the limit wait
/// here instead. The gate engages only when memory is the binding constraint (see
/// <see cref="ImagingMemorySettings.RequiresConcurrencyLimit" />); on any other host no semaphore
/// is created and every request passes straight through.
/// </para>
/// <para>
/// Running ahead of <c>UseImageSharp()</c> means a cache hit cannot be told from a decode, so a
/// gated request holds its slot for the whole of the downstream pipeline - a cache hit, or a
/// missing source falling through to the 404 content, included. Narrowing that further needs the
/// gate inside the imaging middleware, at <c>OnBeforeLoadAsync</c>, which only ImageSharp.Web 3.x
/// offers - hence the placement here, which both packages share.
/// </para>
/// </remarks>
public sealed class ImageProcessingThrottleMiddleware
{
    private readonly RequestDelegate _next;
    private readonly FormatUtilities _formatUtilities;
    private readonly SemaphoreSlim? _semaphore;
    private readonly HashSet<string> _commands;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingThrottleMiddleware" /> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    /// <param name="processors">The registered image processors, used to recognise processing requests.</param>
    /// <param name="formatUtilities">The image format utilities, used to recognise image sources.</param>
    /// <param name="logger">The logger.</param>
    public ImageProcessingThrottleMiddleware(
        RequestDelegate next,
        IOptions<ImagingSettings> imagingSettings,
        IEnumerable<IImageWebProcessor> processors,
        FormatUtilities formatUtilities,
        ILogger<ImageProcessingThrottleMiddleware> logger)
        : this(
            next,
            imagingSettings,
            processors,
            formatUtilities,
            logger,
            GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            Environment.ProcessorCount)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageProcessingThrottleMiddleware" /> class,
    /// with the host's characteristics supplied rather than measured.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="imagingSettings">The Umbraco imaging settings.</param>
    /// <param name="processors">The registered image processors, used to recognise processing requests.</param>
    /// <param name="formatUtilities">The image format utilities, used to recognise image sources.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="availableMemoryBytes">The memory available to the process.</param>
    /// <param name="processorCount">The number of processors available to the process.</param>
    /// <remarks>
    /// Whether the limit applies at all, and what it works out to, are derived from the memory and
    /// processor count of the host. Tests supply both so they assert the derivation instead of
    /// whatever the machine running them happens to report.
    /// </remarks>
    internal ImageProcessingThrottleMiddleware(
        RequestDelegate next,
        IOptions<ImagingSettings> imagingSettings,
        IEnumerable<IImageWebProcessor> processors,
        FormatUtilities formatUtilities,
        ILogger<ImageProcessingThrottleMiddleware> logger,
        long availableMemoryBytes,
        int processorCount)
    {
        _next = next;
        _formatUtilities = formatUtilities;

        var availableMemoryMegabytes = availableMemoryBytes / 1024 / 1024;

        ImagingMemorySettings memory = imagingSettings.Value.Memory;
        if (memory.RequiresConcurrencyLimit(availableMemoryBytes, processorCount))
        {
            var maximumConcurrentProcessing = memory.ResolveMaximumConcurrentProcessing(availableMemoryBytes, processorCount);
            _semaphore = new SemaphoreSlim(maximumConcurrentProcessing, maximumConcurrentProcessing);

            logger.LogInformation(
                "Bounded concurrent image processing to {MaximumConcurrentProcessing}, with {AvailableMemoryMegabytes} MB and {ProcessorCount} processors available to the process.",
                maximumConcurrentProcessing,
                availableMemoryMegabytes,
                processorCount);
        }
        else
        {
            logger.LogDebug(
                "Left concurrent image processing unbounded, with {AvailableMemoryMegabytes} MB and {ProcessorCount} processors available to the process.",
                availableMemoryMegabytes,
                processorCount);
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
        if (_semaphore is null || IsProcessingRequest(context.Request) is false)
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
        if (HasProcessorCommand(request.Query) is false)
        {
            return false;
        }

        // The same test the image providers use, so a slot is never held for the whole of a request
        // the imaging middleware declines.
        return _formatUtilities.TryGetExtensionFromUri(request.GetDisplayUrl(), out _);
    }

    private bool HasProcessorCommand(IQueryCollection query)
    {
        foreach (KeyValuePair<string, StringValues> command in query)
        {
            if (_commands.Contains(command.Key))
            {
                return true;
            }
        }

        return false;
    }
}
