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
/// <para>
/// Zero means "derive a value from the memory available to the process". What each is derived as,
/// and whether it is applied at all, belongs to the imaging package that reads these - the policy
/// depends on the imaging library's own defaults, which this assembly knows nothing about.
/// </para>
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
    /// The largest pool size, in megabytes, that may be configured for
    /// <see cref="MaximumPoolSizeMegabytes" />.
    /// </summary>
    /// <remarks>
    /// Not a limit on anything the imaging library can do - it is well past any pool a site could
    /// use, and past what its own default reaches on all but an extraordinary host. It exists to
    /// catch a value given in bytes rather than megabytes, which would otherwise have the library
    /// size an internal array against it and fail the boot with an unattributable out of memory
    /// error.
    /// </remarks>
    internal const int MaximumConfigurablePoolSizeMegabytes = 65536;

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
}
