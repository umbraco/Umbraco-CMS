using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
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
/// here instead. The gate engages only where imaging memory management is on and the memory
/// available to the process is limited (see
/// <see cref="ImageProcessingMemory.RequiresConcurrencyLimit" />); on any other host no semaphore
/// is created and every request passes straight through.
/// </para>
/// <para>
/// Running ahead of <c>UseImageSharp()</c> means a cache hit cannot be told from a decode, so a
/// gated request takes its place before the downstream pipeline runs - for a cache hit, or a
/// missing source falling through to the 404 content, as much as for a decode. Narrowing that
/// needs the gate inside the imaging middleware, at <c>OnBeforeLoadAsync</c>, which only
/// ImageSharp.Web 3.x offers. The release is the same in both packages: the imaging middleware's
/// processed hook gives the place back as soon as the decoded image is disposed (see
/// <see cref="ConfigureImageSharpMiddlewareOptions" />), and the end of the request is the backstop.
/// </para>
/// </remarks>
public sealed class ImageProcessingThrottleMiddleware
{
    private readonly RequestDelegate _next;
    private readonly FormatUtilities _formatUtilities;
    private readonly ILogger _logger;
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
        _logger = logger;

        var availableMemoryMegabytes = availableMemoryBytes / 1024 / 1024;

        ImagingMemorySettings memorySettings = imagingSettings.Value.Memory;
        if (ImageProcessingMemory.RequiresConcurrencyLimit(memorySettings, availableMemoryBytes))
        {
            var maximumConcurrentProcessing = ImageProcessingMemory.ResolveMaximumConcurrentProcessing(memorySettings, availableMemoryBytes, processorCount);
            _semaphore = new SemaphoreSlim(maximumConcurrentProcessing, maximumConcurrentProcessing);

            logger.LogInformation(
                "Bounded concurrent image processing to {MaximumConcurrentProcessing}, with {AvailableMemoryMegabytes} MB and {ProcessorCount} processors available to the process.",
                maximumConcurrentProcessing,
                availableMemoryMegabytes,
                processorCount);
        }
        else if (memorySettings.Enabled is false)
        {
            // The host's characteristics played no part, so they are not reported as if they had.
            logger.LogDebug("Left concurrent image processing unbounded, because imaging memory management is disabled.");
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
            // The single-image ceiling is in force on hosts where the gate is not, so its failure
            // still needs attributing.
            await InvokeNextAsync(context);
            return;
        }

        var slot = new ImageProcessingSlot(_semaphore, ImageProcessingThrottle.WaitTimeout);
        try
        {
            // Waiting without a bound is hanging, not degrading, so demand beyond what the host can
            // serve is turned away instead of queued indefinitely.
            await slot.AcquireAsync(context.RequestAborted);
        }
        catch (ImageProcessingUnavailableException)
        {
            ImageProcessingThrottle.Reject(context, _logger);
            return;
        }

        // Published so the imaging middleware can give the place back as soon as the decoded image
        // is disposed; released again here for a request that never gets that far.
        context.Items[ImageProcessingSlot.HttpContextItemKey] = slot;
        try
        {
            await InvokeNextAsync(context);
        }
        finally
        {
            slot.Release();
        }
    }

    private async Task InvokeNextAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidImageContentException ex) when (ex.InnerException is InvalidMemoryOperationException inner)
        {
            // Rethrown: the image genuinely cannot be decoded within the ceiling, so a failure is the
            // honest answer. Only the reason for it is added.
            ImageProcessingMemory.LogDecodeOverLimit(context, _logger, inner);
            throw;
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
