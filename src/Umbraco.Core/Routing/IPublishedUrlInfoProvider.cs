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
}
