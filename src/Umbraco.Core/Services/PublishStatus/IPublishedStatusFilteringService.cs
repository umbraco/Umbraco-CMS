using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Provides filtering operations to determine which published content items are available for display.
/// </summary>
public interface IPublishedStatusFilteringService
{
    /// <summary>
    /// Filters a collection of candidate content keys to return only those that are available for display.
    /// </summary>
    /// <param name="candidateKeys">The collection of content keys to filter.</param>
    /// <param name="culture">The culture to filter by, or <c>null</c> to use the current culture context.</param>
    /// <returns>A collection of <see cref="IPublishedContent"/> items that are available for display.</returns>
    IEnumerable<IPublishedContent> FilterAvailable(IEnumerable<Guid> candidateKeys, string? culture);

    /// <summary>
    /// Returns content for a collection of candidate content keys.
    /// </summary>
    /// <param name="candidateKeys">The collection of content keys return.</param>
    /// <returns>A collection of <see cref="IPublishedContent"/> items that are available for display.</returns>
    [Obsolete("This is an intermediate solution to avoid breaking changes. Use the IPublishedContentCache to get published content by key. Scheduled for removal in V19.")]
    IEnumerable<IPublishedContent> Unfiltered(IEnumerable<Guid> candidateKeys) => throw new NotImplementedException();
}
