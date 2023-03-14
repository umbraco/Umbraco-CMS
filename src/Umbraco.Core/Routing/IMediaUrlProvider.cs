using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides media URL.
/// </summary>
public interface IMediaUrlProvider
{
    /// <summary>
    ///     Gets the URL of a media item.
    /// </summary>
    /// <param name="content">The published content.</param>
    /// <param name="propertyAlias">The property alias to resolve the URL from.</param>
    /// <param name="mode">The URL mode.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="current">The current absolute URL.</param>
    /// <returns>The URL for the media.</returns>
    /// <remarks>
    ///     <para>The URL is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
    ///     <para>
    ///         If the media is multi-lingual, gets the URL for the specified culture or,
    ///         when no culture is specified, the current culture.
    ///     </para>
    ///     <para>
    ///         The URL provider can ignore the mode and always return an absolute URL,
    ///         e.g. a cdn URL provider will most likely always return an absolute URL.
    ///     </para>
    ///     <para>If the provider is unable to provide a URL, it returns <c>null</c>.</para>
    /// </remarks>
    UrlInfo? GetMediaUrl(IPublishedContent content, string propertyAlias, UrlMode mode, string? culture, Uri current);
}
