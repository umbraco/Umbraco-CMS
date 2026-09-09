// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Typed configuration options for the memory used while processing images.
/// </summary>
/// <remarks>
/// Image processing decodes the full-resolution source into memory before it is resized, so peak
/// memory scales with the number of images being processed at the same time rather than with the
/// size of the response. On a host with a hard memory limit - a container, most commonly - an
/// unbounded number of concurrent decodes will exhaust the limit and the process will be killed.
/// </remarks>
public class ImagingMemorySettings
{
    /// <summary>
    /// Whether image processing memory is managed by default.
    /// </summary>
    internal const bool StaticEnabled = true;

    /// <summary>
    /// The default maximum pool size, in megabytes. Zero means it is derived from the available memory.
    /// </summary>
    internal const int StaticMaximumPoolSizeMegabytes = 0;

    /// <summary>
    /// The default maximum number of images processed concurrently. Zero means it is derived from
    /// the available memory.
    /// </summary>
    internal const int StaticMaximumConcurrentProcessing = 0;

    /// <summary>
    /// The default maximum size of a single decoded image, in megabytes. Zero means it is derived
    /// from the available memory.
    /// </summary>
    internal const int StaticMaximumDecodedImageMegabytes = 0;

    /// <summary>
    /// The share of available memory image processing is allowed to occupy when deriving
    /// <see cref="MaximumConcurrentProcessing" />.
    /// </summary>
    /// <remarks>
    /// The available memory reported for a container is already a fraction of its limit, so this
    /// only has to leave room for the rest of the site rather than for the whole overhead again.
    /// </remarks>
    private const int ConcurrencyMemoryShareDivisor = 2;

    /// <summary>
    /// The assumed peak cost of processing a single image, in megabytes, when deriving
    /// <see cref="MaximumConcurrentProcessing" />. Measured against a 12 megapixel JPEG source.
    /// </summary>
    private const int EstimatedMegabytesPerImage = 64;

    /// <summary>
    /// The share of available memory used when deriving <see cref="MaximumPoolSizeMegabytes" />.
    /// </summary>
    /// <remarks>
    /// ImageSharp itself defaults to an eighth of available memory on a 64-bit process, which it
    /// releases only on a gen2 collection and then only in halves, at most once a minute. A tighter
    /// pool trades a little throughput for markedly lower memory at rest.
    /// </remarks>
    private const int PoolMemoryShareDivisor = 32;

    private const int MinimumPoolSizeMegabytes = 16;

    private const int MaximumDerivedPoolSizeMegabytes = 64;

    /// <summary>
    /// The share of available memory a single decoded image may occupy when deriving
    /// <see cref="MaximumDecodedImageMegabytes" />.
    /// </summary>
    /// <remarks>
    /// Deliberately generous. This is a ceiling on the absurd, not a target: a source needing a
    /// quarter of the memory available to the whole site cannot be processed usefully whatever the
    /// concurrency, so failing that one request beats exhausting the host.
    /// </remarks>
    private const int DecodedImageMemoryShareDivisor = 4;

    private const int MinimumDecodedImageMegabytes = 256;

    private const int MaximumDerivedDecodedImageMegabytes = 1024;

    /// <summary>
    /// The memory available to the process, in megabytes, below which
    /// <see cref="MaximumPoolSizeMegabytes" /> and <see cref="MaximumDecodedImageMegabytes" /> are
    /// applied.
    /// </summary>
    /// <remarks>
    /// On a 64-bit process the imaging library's own pool default is an eighth of available memory
    /// whatever the host size, so it is never disproportionate - it is a problem only in absolute
    /// terms, where that eighth competes with the memory the rest of the site needs. On a host with
    /// room to spare it costs nothing worth reclaiming, so the pool is left alone above this point.
    /// A 32-bit process gets a flat 128 MB instead, which is already conservative, so whether this
    /// threshold is reached there matters little either way.
    /// <para>
    /// That default is not documented, only implemented, so it is worth re-checking whenever the
    /// imaging library is upgraded:
    /// https://github.com/SixLabors/ImageSharp/blob/v3.1.12/src/ImageSharp/Memory/Allocators/UniformUnmanagedMemoryPoolMemoryAllocator.cs#L156
    /// </para>
    /// </remarks>
    private const int MemoryManagementThresholdMegabytes = 4096;

    private const int OneMegabyte = 1024 * 1024;

    /// <summary>
    /// Gets or sets a value indicating whether image processing memory is managed.
    /// </summary>
    /// <remarks>
    /// When enabled (the default), the pool the imaging library retains between requests is capped and
    /// the number of images decoded at the same time is bounded on hosts where memory is the binding
    /// constraint. Set to <c>false</c> to leave the imaging library's own memory behaviour untouched -
    /// none of the pool cap, the concurrency bound or the single-image ceiling is applied.
    /// </remarks>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;

    /// <summary>
    /// Gets or sets the maximum size, in megabytes, of the pool the imaging library retains for
    /// reuse between requests.
    /// </summary>
    /// <remarks>
    /// This memory is unmanaged, so it is not governed by any of the <c>DOTNET_GC*</c> settings and
    /// does not appear in the managed heap. Set to zero to derive a value from the available memory.
    /// </remarks>
    [DefaultValue(StaticMaximumPoolSizeMegabytes)]
    public int MaximumPoolSizeMegabytes { get; set; } = StaticMaximumPoolSizeMegabytes;

    /// <summary>
    /// Gets or sets the maximum number of images that may be processed at the same time.
    /// </summary>
    /// <remarks>
    /// Requests beyond this limit wait rather than being rejected. Set to zero to derive a value
    /// from the available memory and processor count.
    /// </remarks>
    [DefaultValue(StaticMaximumConcurrentProcessing)]
    public int MaximumConcurrentProcessing { get; set; } = StaticMaximumConcurrentProcessing;

    /// <summary>
    /// Gets or sets the maximum size, in megabytes, of the buffers a single image may be decoded
    /// into.
    /// </summary>
    /// <remarks>
    /// A request for an image needing more than this fails rather than being served, which on a
    /// memory-limited host is preferable to exhausting the limit and taking the process with it.
    /// <see cref="MaximumConcurrentProcessing" /> bounds how many images are decoded at once
    /// against an assumed cost each; this bounds that cost, so an unusually large source cannot
    /// exceed the budget the two are meant to keep. Set to zero to derive a value from the
    /// available memory.
    /// </remarks>
    [DefaultValue(StaticMaximumDecodedImageMegabytes)]
    public int MaximumDecodedImageMegabytes { get; set; } = StaticMaximumDecodedImageMegabytes;

    /// <summary>
    /// Resolves <see cref="MaximumPoolSizeMegabytes" />, deriving a value when it is not configured.
    /// </summary>
    /// <param name="availableMemoryBytes">
    /// The memory available to the process, honouring any container limit. Typically
    /// <see cref="GCMemoryInfo.TotalAvailableMemoryBytes" />.
    /// </param>
    /// <returns>The maximum pool size, in megabytes.</returns>
    public int ResolveMaximumPoolSizeMegabytes(long availableMemoryBytes)
    {
        if (MaximumPoolSizeMegabytes > 0)
        {
            return MaximumPoolSizeMegabytes;
        }

        long derived = availableMemoryBytes / PoolMemoryShareDivisor / OneMegabyte;

        return (int)Math.Clamp(derived, MinimumPoolSizeMegabytes, MaximumDerivedPoolSizeMegabytes);
    }

    /// <summary>
    /// Gets a value indicating whether the pool the imaging library retains between requests needs
    /// to be capped on this host.
    /// </summary>
    /// <param name="availableMemoryBytes">
    /// The memory available to the process, honouring any container limit. Typically
    /// <see cref="GCMemoryInfo.TotalAvailableMemoryBytes" />.
    /// </param>
    /// <returns>
    /// <c>true</c> when a size is configured explicitly, or when the memory available to the
    /// process is low enough that what the library retains at rest competes with the rest of the
    /// site; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Left alone on a host with memory to spare, so upgrading a site that was never at risk does
    /// not change how the imaging library allocates.
    /// </remarks>
    public bool RequiresPoolSizeLimit(long availableMemoryBytes)
        => Enabled
           && (MaximumPoolSizeMegabytes > 0
               || availableMemoryBytes < MemoryManagementThresholdMegabytes * (long)OneMegabyte);

    /// <summary>
    /// Resolves <see cref="MaximumDecodedImageMegabytes" />, deriving a value when it is not
    /// configured.
    /// </summary>
    /// <param name="availableMemoryBytes">
    /// The memory available to the process, honouring any container limit. Typically
    /// <see cref="GCMemoryInfo.TotalAvailableMemoryBytes" />.
    /// </param>
    /// <returns>The maximum size of a single decoded image, in megabytes.</returns>
    public int ResolveMaximumDecodedImageMegabytes(long availableMemoryBytes)
    {
        if (MaximumDecodedImageMegabytes > 0)
        {
            return MaximumDecodedImageMegabytes;
        }

        long derived = availableMemoryBytes / DecodedImageMemoryShareDivisor / OneMegabyte;

        return (int)Math.Clamp(derived, MinimumDecodedImageMegabytes, MaximumDerivedDecodedImageMegabytes);
    }

    /// <summary>
    /// Gets a value indicating whether the size of a single decoded image needs to be capped on
    /// this host.
    /// </summary>
    /// <param name="availableMemoryBytes">
    /// The memory available to the process, honouring any container limit. Typically
    /// <see cref="GCMemoryInfo.TotalAvailableMemoryBytes" />.
    /// </param>
    /// <returns>
    /// <c>true</c> when a size is configured explicitly, or when the memory available to the
    /// process is low enough that one outsized source could exhaust it; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The imaging library's own ceiling here is a flat 1 GB on a 32-bit process and 4 GB on a
    /// 64-bit one, so on a host with memory to spare a derived value would only ever loosen it.
    /// Left alone above the same threshold as the pool cap, so one figure governs whether imaging
    /// memory is managed at all.
    /// </remarks>
    public bool RequiresAllocationLimit(long availableMemoryBytes)
        => Enabled
           && (MaximumDecodedImageMegabytes > 0
               || availableMemoryBytes < MemoryManagementThresholdMegabytes * (long)OneMegabyte);

    /// <summary>
    /// Resolves <see cref="MaximumConcurrentProcessing" />, deriving a value when it is not configured.
    /// </summary>
    /// <param name="availableMemoryBytes">
    /// The memory available to the process, honouring any container limit. Typically
    /// <see cref="GCMemoryInfo.TotalAvailableMemoryBytes" />.
    /// </param>
    /// <param name="processorCount">The number of processors available to the process.</param>
    /// <returns>The maximum number of images to process concurrently.</returns>
    public int ResolveMaximumConcurrentProcessing(long availableMemoryBytes, int processorCount)
    {
        if (MaximumConcurrentProcessing > 0)
        {
            return MaximumConcurrentProcessing;
        }

        // Decoding is CPU bound, so more concurrency than processors buys nothing but memory.
        return (int)Math.Clamp(DeriveConcurrentProcessing(availableMemoryBytes), 1, Math.Max(processorCount, 1));
    }

    /// <summary>
    /// Gets a value indicating whether the number of images processed concurrently needs to be
    /// bounded on this host.
    /// </summary>
    /// <param name="availableMemoryBytes">
    /// The memory available to the process, honouring any container limit. Typically
    /// <see cref="GCMemoryInfo.TotalAvailableMemoryBytes" />.
    /// </param>
    /// <param name="processorCount">The number of processors available to the process.</param>
    /// <returns>
    /// <c>true</c> when a limit is configured explicitly, or when the memory budget cannot cover as
    /// many concurrent decodes as the processors would otherwise run; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Decoding is CPU bound, so the processor count already caps how many images decode at once.
    /// A concurrency limit only earns its keep when memory is the tighter constraint - a container
    /// with a low limit relative to its core count. Everywhere else - an uncapped host, or a host
    /// with few cores relative to its memory - bounding concurrency would only add latency to
    /// requests the cache can serve without protecting against anything.
    /// </remarks>
    public bool RequiresConcurrencyLimit(long availableMemoryBytes, int processorCount)
        => Enabled && (MaximumConcurrentProcessing > 0 || DeriveConcurrentProcessing(availableMemoryBytes) < processorCount);

    private static long DeriveConcurrentProcessing(long availableMemoryBytes)
        => availableMemoryBytes / ConcurrencyMemoryShareDivisor / (EstimatedMegabytesPerImage * OneMegabyte);
}
