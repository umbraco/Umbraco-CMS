using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IPublishedContentCache : IPublishedCache
{
    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="id">The content unique identifier.</param>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="key">The content unique identifier.</param>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByIdAsync(Guid key, bool? preview = null);

    // FIXME: All these routing methods needs to be removed, as they are no longer part of the content cache
    /// <summary>
    ///     Gets content identified by a route.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="route">The route</param>
    /// <param name="hideTopLevelNode">A value forcing the HideTopLevelNode setting.</param>
    /// <param name="culture">the culture</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>
    ///     <para>
    ///         A valid route is either a simple path eg <c>/foo/bar/nil</c> or a root node id and a path, eg
    ///         <c>123/foo/bar/nil</c>.
    ///     </para>
    ///     <para>
    ///         If
    ///         <paramref name="hideTopLevelNode" />
    ///         is <c>null</c> then the settings value is used.
    ///     </para>
    ///     <para>The value of <paramref name="preview" /> overrides defaults.</para>
    /// </remarks>
    [Obsolete]
    IPublishedContent? GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string? culture = null);

    /// <summary>
    ///     Gets content identified by a route.
    /// </summary>
    /// <param name="route">The route</param>
    /// <param name="hideTopLevelNode">A value forcing the HideTopLevelNode setting.</param>
    /// <param name="culture">The culture</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>
    ///     <para>
    ///         A valid route is either a simple path eg <c>/foo/bar/nil</c> or a root node id and a path, eg
    ///         <c>123/foo/bar/nil</c>.
    ///     </para>
    ///     <para>
    ///         If
    ///         <paramref name="hideTopLevelNode" />
    ///         is <c>null</c> then the settings value is used.
    ///     </para>
    ///     <para>Considers published or unpublished content depending on defaults.</para>
    /// </remarks>
    [Obsolete]
    IPublishedContent? GetByRoute(string route, bool? hideTopLevelNode = null, string? culture = null);

    /// <summary>
    ///     Gets the route for a content identified by its unique identifier.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="contentId">The content unique identifier.</param>
    /// <param name="culture">The culture</param>
    /// <returns>A special string formatted route path.</returns>
    /// <remarks>
    ///     <para>
    ///         The resulting string is a special encoded route string that may contain the domain ID
    ///         for the current route. If a domain is present the string will be prefixed with the domain ID integer, example:
    ///         {domainId}/route-path-of-item
    ///     </para>
    ///     <para>The value of <paramref name="preview" /> overrides defaults.</para>
    /// </remarks>
    [Obsolete]
    string? GetRouteById(bool preview, int contentId, string? culture = null);

    /// <summary>
    ///     Gets the route for a content identified by its unique identifier.
    /// </summary>
    /// <param name="contentId">The content unique identifier.</param>
    /// <param name="culture">The culture</param>
    /// <returns>A special string formatted route path.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    /// <para>
    ///     The resulting string is a special encoded route string that may contain the domain ID
    ///     for the current route. If a domain is present the string will be prefixed with the domain ID integer, example:
    ///     {domainId}/route-path-of-item
    /// </para>
    [Obsolete]
    string? GetRouteById(int contentId, string? culture = null);
}
