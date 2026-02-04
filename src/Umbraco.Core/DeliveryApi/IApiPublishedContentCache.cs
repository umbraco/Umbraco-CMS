using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a cache for accessing published content through the Delivery API.
/// </summary>
public interface IApiPublishedContentCache
{
    /// <summary>
    ///     Gets published content by its route.
    /// </summary>
    /// <param name="route">The content route.</param>
    /// <returns>The published content, or <c>null</c> if not found.</returns>
    IPublishedContent? GetByRoute(string route);

    /// <summary>
    ///     Gets published content by its unique identifier.
    /// </summary>
    /// <param name="contentId">The content's unique identifier.</param>
    /// <returns>The published content, or <c>null</c> if not found.</returns>
    IPublishedContent? GetById(Guid contentId);

    /// <summary>
    ///     Gets multiple published content items by their unique identifiers.
    /// </summary>
    /// <param name="contentIds">The content unique identifiers.</param>
    /// <returns>The published content items that were found.</returns>
    IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds);

    /// <summary>
    ///     Asynchronously gets published content by its unique identifier.
    /// </summary>
    /// <param name="contentId">The content's unique identifier.</param>
    /// <returns>A task containing the published content, or <c>null</c> if not found.</returns>
    Task<IPublishedContent?> GetByIdAsync(Guid contentId) => Task.FromResult(GetById(contentId));

    /// <summary>
    ///     Asynchronously gets published content by its route.
    /// </summary>
    /// <param name="route">The content route.</param>
    /// <returns>A task containing the published content, or <c>null</c> if not found.</returns>
    Task<IPublishedContent?> GetByRouteAsync(string route) => Task.FromResult(GetByRoute(route));

    /// <summary>
    ///     Asynchronously gets multiple published content items by their unique identifiers.
    /// </summary>
    /// <param name="contentIds">The content unique identifiers.</param>
    /// <returns>A task containing the published content items that were found.</returns>
    Task<IEnumerable<IPublishedContent>> GetByIdsAsync(IEnumerable<Guid> contentIds) => Task.FromResult(GetByIds(contentIds));
}
