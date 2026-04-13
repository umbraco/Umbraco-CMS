using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides custom cache durations per content or media item for Delivery API output caching.
/// </summary>
/// <remarks>
///     A single registration of this interface is expected.
///     The default implementation can be replaced to vary duration by content type, path, or content properties.
/// </remarks>
public interface IDeliveryApiOutputCacheDurationProvider
{
    /// <summary>
    ///     Returns a custom cache duration for the given content or media item, or <c>null</c> to use the configured default.
    /// </summary>
    /// <param name="content">The published content or media item being cached.</param>
    /// <returns>
    ///     A custom cache duration, <see cref="TimeSpan.Zero"/> to disable caching for this item,
    ///     or <c>null</c> to use the configured default duration.
    /// </returns>
    TimeSpan? GetDuration(IPublishedContent content);
}
