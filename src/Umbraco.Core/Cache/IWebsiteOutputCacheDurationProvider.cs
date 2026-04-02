using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides custom cache durations per content item for website output caching.
/// </summary>
/// <remarks>
///     <para>
///         Single registration — customers replace the default implementation to vary duration
///         by content type, path, or content properties.
///     </para>
///     <para>
///         Return <c>null</c> to use the configured default duration.
///         Return <see cref="TimeSpan.Zero"/> to disable caching for the content item.
///     </para>
/// </remarks>
public interface IWebsiteOutputCacheDurationProvider
{
    /// <summary>
    ///     Returns a custom cache duration for the given content, or <c>null</c> to use the configured default.
    /// </summary>
    /// <param name="content">The published content being cached.</param>
    /// <returns>
    ///     A custom cache duration, <see cref="TimeSpan.Zero"/> to disable caching for this content,
    ///     or <c>null</c> to use the configured default duration.
    /// </returns>
    TimeSpan? GetDuration(IPublishedContent content);
}
