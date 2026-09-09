using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Bounds the memory the imaging library uses against the memory available to the process.
/// </summary>
/// <remarks>
/// The policy lives here rather than on <see cref="ImagingMemorySettings" /> because it only means
/// anything against the imaging library's own defaults, which Umbraco.Core knows nothing about.
/// Those settings carry the configured values; this decides what they amount to.
/// <para>
/// Nothing here is specific to a major version of the imaging library, so this file is compiled
/// into the ImageSharp 2.x package as a linked source file rather than copied. The two packages
/// cannot share an assembly - they depend on mutually exclusive majors - so sharing the source is
/// the only way to keep one copy.
/// </para>
/// </remarks>
internal static class ImageProcessingMemory
{
    /// <summary>
    /// The share of available memory image processing is allowed to occupy when deriving the
    /// concurrency bound.
    /// </summary>
    /// <remarks>
    /// The available memory reported for a container is already a fraction of its limit, so this
    /// only has to leave room for the rest of the site rather than for the whole overhead again.
    /// </remarks>
    private const int ConcurrencyMemoryShareDivisor = 2;

    /// <summary>
    /// The assumed peak cost of processing a single image, in megabytes, when deriving the
    /// concurrency bound. Measured against a 12 megapixel JPEG source.
    /// </summary>
    private const int EstimatedMegabytesPerImage = 64;

    /// <summary>
    /// The share of available memory used when deriving the pool size.
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
    /// The share of available memory a single decoded image may occupy when deriving its ceiling.
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
    /// The memory available to the process, in megabytes, below which the pool size and the
    /// single-image ceiling are applied.
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
    /// Applies the configured bounds to the imaging library's memory allocator.
    /// </summary>
    /// <param name="services">The application services.</param>
    /// <param name="availableMemoryBytes">
    /// The memory available to the process, honouring any container limit. Read by the caller so
    /// that every input to the decision is explicit.
    /// </param>
    /// <remarks>
    /// Applied while the pipeline is built rather than while services are registered, so the
    /// settings come from <c>IOptions</c> - the same source the rest of the imaging code reads, and
    /// the only one a composer can contribute to. That is late enough: nothing is allocated until an
    /// image is decoded, which cannot happen before the site takes a request. It also puts both
    /// bounds in one place, so the pool and the concurrency gate are engaged as a unit.
    /// </remarks>
    internal static void Configure(IServiceProvider services, long availableMemoryBytes)
    {
        ImagingMemorySettings memory = services.GetRequiredService<IOptions<ImagingSettings>>().Value.Memory;
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ImageProcessingMemory));

        var availableMemoryMegabytes = availableMemoryBytes / 1024 / 1024;

        // ImageSharp pools unmanaged memory sized against the available memory and releases it only
        // on a gen2 collection, so on a memory constrained host it sits at rest well above what the
        // site needs, and it will decode a source of any size into that memory. Both are left to
        // the library on a host with room to spare.
        MemoryAllocatorOptions options = default;

        if (RequiresPoolSizeLimit(memory, availableMemoryBytes))
        {
            options.MaximumPoolSizeMegabytes = ResolveMaximumPoolSizeMegabytes(memory, availableMemoryBytes);
        }

        if (RequiresAllocationLimit(memory, availableMemoryBytes))
        {
            options.AllocationLimitMegabytes = ResolveMaximumDecodedImageMegabytes(memory, availableMemoryBytes);
        }

        if (options.MaximumPoolSizeMegabytes.HasValue is false && options.AllocationLimitMegabytes.HasValue is false)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    "Left image processing memory to the imaging library, with {AvailableMemoryMegabytes} MB available to the process.",
                    availableMemoryMegabytes);
            }

            return;
        }

        // One allocator, shared process-wide. The library's own remarks on
        // Configuration.MemoryAllocator say to ensure that "by altering the allocator of
        // Configuration.Default", which is why this does not follow the documented sample's
        // Configuration.Default.Clone(): a clone would leave the default on its own allocator, so
        // anything using that directly - a package, or a plain Image.Load - would pool separately.
        // https://docs.sixlabors.com/articles/imagesharp/memorymanagement.html#customize-the-allocator
        MemoryAllocator dropped = Configuration.Default.MemoryAllocator;
        Configuration.Default.MemoryAllocator = MemoryAllocator.Create(options);

        // Required of an allocator that is dropped, by the same remarks. Nothing is retained on a
        // first boot, but a process that builds several hosts - the test harness - would otherwise
        // leave every replaced pool holding its returned buffers. Only idle buffers are freed, so
        // this cannot disturb an image still in use.
        dropped.ReleaseRetainedResources();

        logger.LogInformation(
            "Bounded image processing memory with a {MaximumPoolSizeMegabytes} MB pool and a {MaximumDecodedImageMegabytes} MB ceiling per image, with {AvailableMemoryMegabytes} MB available to the process. A null bound is left to the imaging library.",
            options.MaximumPoolSizeMegabytes,
            options.AllocationLimitMegabytes,
            availableMemoryMegabytes);
    }

    /// <summary>
    /// Gets a value indicating whether the number of images processed concurrently needs to be
    /// bounded on this host.
    /// </summary>
    /// <param name="memory">The imaging memory settings.</param>
    /// <param name="availableMemoryBytes">The memory available to the process.</param>
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
    internal static bool RequiresConcurrencyLimit(
        ImagingMemorySettings memory,
        long availableMemoryBytes,
        int processorCount)
        => memory.Enabled
           && (memory.MaximumConcurrentProcessing > 0
               || DeriveConcurrentProcessing(availableMemoryBytes) < processorCount);

    /// <summary>
    /// Resolves the number of images that may be processed at the same time, deriving a value when
    /// it is not configured.
    /// </summary>
    /// <param name="memory">The imaging memory settings.</param>
    /// <param name="availableMemoryBytes">The memory available to the process.</param>
    /// <param name="processorCount">The number of processors available to the process.</param>
    /// <returns>The maximum number of images to process concurrently.</returns>
    internal static int ResolveMaximumConcurrentProcessing(
        ImagingMemorySettings memory,
        long availableMemoryBytes,
        int processorCount)
    {
        if (memory.MaximumConcurrentProcessing > 0)
        {
            return memory.MaximumConcurrentProcessing;
        }

        // Decoding is CPU bound, so more concurrency than processors buys nothing but memory.
        return (int)Math.Clamp(DeriveConcurrentProcessing(availableMemoryBytes), 1, Math.Max(processorCount, 1));
    }

    /// <summary>
    /// Gets a value indicating whether the pool the imaging library retains between requests needs
    /// to be capped on this host.
    /// </summary>
    /// <param name="memory">The imaging memory settings.</param>
    /// <param name="availableMemoryBytes">The memory available to the process.</param>
    /// <returns>
    /// <c>true</c> when a size is configured explicitly, or when the memory available to the
    /// process is low enough that what the library retains at rest competes with the rest of the
    /// site; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Left alone on a host with memory to spare, so upgrading a site that was never at risk does
    /// not change how the imaging library allocates.
    /// </remarks>
    internal static bool RequiresPoolSizeLimit(ImagingMemorySettings memory, long availableMemoryBytes)
        => memory.Enabled
           && (memory.MaximumPoolSizeMegabytes > 0
               || availableMemoryBytes < MemoryManagementThresholdMegabytes * (long)OneMegabyte);

    /// <summary>
    /// Resolves the size of the pool the imaging library retains between requests, deriving a value
    /// when it is not configured.
    /// </summary>
    /// <param name="memory">The imaging memory settings.</param>
    /// <param name="availableMemoryBytes">The memory available to the process.</param>
    /// <returns>The maximum pool size, in megabytes.</returns>
    internal static int ResolveMaximumPoolSizeMegabytes(ImagingMemorySettings memory, long availableMemoryBytes)
    {
        if (memory.MaximumPoolSizeMegabytes > 0)
        {
            return memory.MaximumPoolSizeMegabytes;
        }

        long derived = availableMemoryBytes / PoolMemoryShareDivisor / OneMegabyte;

        return (int)Math.Clamp(derived, MinimumPoolSizeMegabytes, MaximumDerivedPoolSizeMegabytes);
    }

    /// <summary>
    /// Gets a value indicating whether the size of a single decoded image needs to be capped on
    /// this host.
    /// </summary>
    /// <param name="memory">The imaging memory settings.</param>
    /// <param name="availableMemoryBytes">The memory available to the process.</param>
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
    internal static bool RequiresAllocationLimit(ImagingMemorySettings memory, long availableMemoryBytes)
        => memory.Enabled
           && (memory.MaximumDecodedImageMegabytes > 0
               || availableMemoryBytes < MemoryManagementThresholdMegabytes * (long)OneMegabyte);

    /// <summary>
    /// Resolves the size of the buffers a single image may be decoded into, deriving a value when
    /// it is not configured.
    /// </summary>
    /// <param name="memory">The imaging memory settings.</param>
    /// <param name="availableMemoryBytes">The memory available to the process.</param>
    /// <returns>The maximum size of a single decoded image, in megabytes.</returns>
    internal static int ResolveMaximumDecodedImageMegabytes(ImagingMemorySettings memory, long availableMemoryBytes)
    {
        if (memory.MaximumDecodedImageMegabytes > 0)
        {
            return memory.MaximumDecodedImageMegabytes;
        }

        long derived = availableMemoryBytes / DecodedImageMemoryShareDivisor / OneMegabyte;

        return (int)Math.Clamp(derived, MinimumDecodedImageMegabytes, MaximumDerivedDecodedImageMegabytes);
    }

    private static long DeriveConcurrentProcessing(long availableMemoryBytes)
        => availableMemoryBytes / ConcurrencyMemoryShareDivisor / (EstimatedMegabytesPerImage * OneMegabyte);
}
