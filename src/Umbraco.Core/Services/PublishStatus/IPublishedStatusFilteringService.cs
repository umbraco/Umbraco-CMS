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
}
