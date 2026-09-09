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
/// Nothing here is specific to a major version of the imaging library, so this file is compiled
/// into the ImageSharp 2.x package as a linked source file rather than copied. The two packages
/// cannot share an assembly - they depend on mutually exclusive majors - so sharing the source is
/// the only way to keep one copy.
/// </remarks>
internal static class ImageProcessingMemory
{
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

        if (memory.RequiresPoolSizeLimit(availableMemoryBytes))
        {
            options.MaximumPoolSizeMegabytes = memory.ResolveMaximumPoolSizeMegabytes(availableMemoryBytes);
        }

        if (memory.RequiresAllocationLimit(availableMemoryBytes))
        {
            options.AllocationLimitMegabytes = memory.ResolveMaximumDecodedImageMegabytes(availableMemoryBytes);
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
}
