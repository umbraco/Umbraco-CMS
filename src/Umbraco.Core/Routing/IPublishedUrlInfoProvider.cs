using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides URL information for published content items.
/// </summary>
public interface IPublishedUrlInfoProvider
{
    /// <summary>
    /// Gets all published urls for a content item.
    /// </summary>
    /// <param name="content">The content to get urls for.</param>
    /// <returns>Set of all published url infos.</returns>
    Task<ISet<UrlInfo>> GetAllAsync(IContent content);

    /// <summary>
    /// Gets the published urls for a content item, optionally restricted to a single culture.
    /// </summary>
    /// <param name="content">The content to get urls for.</param>
    /// <param name="culture">
    /// The culture to restrict variant content urls to, or <c>null</c> to return urls for all cultures.
    /// Ignored for invariant content, which always returns all of its domain urls.
    /// </param>
    /// <returns>Set of published url infos.</returns>
    // TODO (V19): Remove the default implementation.
    Task<ISet<UrlInfo>> GetAllAsync(IContent content, string? culture)
        => GetAllAsync(content);
}
