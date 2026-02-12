using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides URLs.
/// </summary>
public interface IUrlProvider
{
    /// <summary>
    ///     Gets the URL of a published content.
    /// </summary>
    /// <param name="content">The published content.</param>
    /// <param name="mode">The URL mode.</param>
    /// <param name="culture">A culture.</param>
    /// <param name="current">The current absolute URL.</param>
    /// <returns>The URL for the published content.</returns>
    /// <remarks>
    ///     <para>The URL is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
    ///     <para>
    ///         If the published content is multi-lingual, gets the URL for the specified culture or,
    ///         when no culture is specified, the current culture.
    ///     </para>
    ///     <para>If the provider is unable to provide a URL, it should return <c>null</c>.</para>
    /// </remarks>
    UrlInfo? GetUrl(IPublishedContent content, UrlMode mode, string? culture, Uri current);

    /// <summary>
    ///     Gets the other URLs of a published content.
    /// </summary>
    /// <param name="id">The published content id.</param>
    /// <param name="current">The current absolute URL.</param>
    /// <returns>The other URLs for the published content.</returns>
    /// <remarks>
    ///     <para>
    ///         Other URLs are those that <c>GetUrl</c> would not return in the current context, but would be valid
    ///         URLs for the node in other contexts (different domain for current request, umbracoUrlAlias...).
    ///     </para>
    /// </remarks>
    IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current);

    /// <summary>
    ///     Gets the preview URL of a content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="culture">The culture to preview (null means invariant).</param>
    /// <param name="segment">The segment to preview (null means no specific segment).</param>
    /// <returns>The preview URLs of the content item.</returns>
    Task<UrlInfo?> GetPreviewUrlAsync(IContent content, string? culture, string? segment);

    /// <summary>
    ///     Gets the alias for the URL provider.
    /// </summary>
    public string Alias { get; }
}
