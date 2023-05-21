using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

public interface IPublishedUrlProvider
{
    /// <summary>
    ///     Gets or sets the provider url mode.
    /// </summary>
    UrlMode Mode { get; set; }

    /// <summary>
    ///     Gets the url of a published content.
    /// </summary>
    /// <param name="id">The published content identifier.</param>
    /// <param name="mode">The url mode.</param>
    /// <param name="culture">A culture.</param>
    /// <param name="current">The current absolute url.</param>
    /// <returns>The url for the published content.</returns>
    string GetUrl(Guid id, UrlMode mode = UrlMode.Default, string? culture = null, Uri? current = null);

    /// <summary>
    ///     Gets the url of a published content.
    /// </summary>
    /// <param name="id">The published content identifier.</param>
    /// <param name="mode">The url mode.</param>
    /// <param name="culture">A culture.</param>
    /// <param name="current">The current absolute url.</param>
    /// <returns>The url for the published content.</returns>
    string GetUrl(int id, UrlMode mode = UrlMode.Default, string? culture = null, Uri? current = null);

    /// <summary>
    ///     Gets the url of a published content.
    /// </summary>
    /// <param name="content">The published content.</param>
    /// <param name="mode">The url mode.</param>
    /// <param name="culture">A culture.</param>
    /// <param name="current">The current absolute url.</param>
    /// <returns>The url for the published content.</returns>
    /// <remarks>
    ///     <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
    ///     <para>
    ///         If the published content is multi-lingual, gets the url for the specified culture or,
    ///         when no culture is specified, the current culture.
    ///     </para>
    ///     <para>If the provider is unable to provide a url, it returns "#".</para>
    /// </remarks>
    string GetUrl(IPublishedContent content, UrlMode mode = UrlMode.Default, string? culture = null, Uri? current = null);

    string GetUrlFromRoute(int id, string? route, string? culture);

    /// <summary>
    ///     Gets the other urls of a published content.
    /// </summary>
    /// <param name="id">The published content id.</param>
    /// <returns>The other urls for the published content.</returns>
    /// <remarks>
    ///     <para>
    ///         Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
    ///         urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).
    ///     </para>
    ///     <para>The results depend on the current url.</para>
    /// </remarks>
    IEnumerable<UrlInfo> GetOtherUrls(int id);

    /// <summary>
    ///     Gets the other urls of a published content.
    /// </summary>
    /// <param name="id">The published content id.</param>
    /// <param name="current">The current absolute url.</param>
    /// <returns>The other urls for the published content.</returns>
    /// <remarks>
    ///     <para>
    ///         Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
    ///         urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).
    ///     </para>
    /// </remarks>
    IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current);

    /// <summary>
    ///     Gets the url of a media item.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="mode"></param>
    /// <param name="culture"></param>
    /// <param name="propertyAlias"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    string GetMediaUrl(Guid id, UrlMode mode = UrlMode.Default, string? culture = null, string propertyAlias = Constants.Conventions.Media.File, Uri? current = null);

    /// <summary>
    ///     Gets the url of a media item.
    /// </summary>
    /// <param name="content">The published content.</param>
    /// <param name="propertyAlias">The property alias to resolve the url from.</param>
    /// <param name="mode">The url mode.</param>
    /// <param name="culture">The variation language.</param>
    /// <param name="current">The current absolute url.</param>
    /// <returns>The url for the media.</returns>
    /// <remarks>
    ///     <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
    ///     <para>
    ///         If the media is multi-lingual, gets the url for the specified culture or,
    ///         when no culture is specified, the current culture.
    ///     </para>
    ///     <para>If the provider is unable to provide a url, it returns <see cref="string.Empty" />.</para>
    /// </remarks>
    string GetMediaUrl(IPublishedContent? content, UrlMode mode = UrlMode.Default, string? culture = null, string propertyAlias = Constants.Conventions.Media.File, Uri? current = null);
}
