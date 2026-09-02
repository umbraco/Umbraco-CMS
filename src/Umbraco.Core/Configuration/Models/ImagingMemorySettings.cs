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
    /// ImageSharp itself defaults to an eighth of available memory, which it releases only on a
    /// gen2 collection and then only in halves, at most once a minute. A tighter pool trades a
    /// little throughput for markedly lower memory at rest.
    /// </remarks>
    private const int PoolMemoryShareDivisor = 32;

    private const int MinimumPoolSizeMegabytes = 16;

    private const int MaximumDerivedPoolSizeMegabytes = 64;

    private const int OneMegabyte = 1024 * 1024;

    /// <summary>
    /// Gets or sets a value indicating whether image processing memory is managed.
    /// </summary>
    /// <remarks>
    /// When enabled (the default), the pool the imaging library retains between requests is capped and
    /// the number of images decoded at the same time is bounded on hosts where memory is the binding
    /// constraint. Set to <c>false</c> to leave the imaging library's own memory behaviour untouched -
    /// neither the pool cap nor the concurrency bound is applied.
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
