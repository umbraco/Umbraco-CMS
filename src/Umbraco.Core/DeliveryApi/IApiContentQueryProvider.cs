using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Concrete implementation of content querying (e.g. based on Examine)
/// </summary>
public interface IApiContentQueryProvider
{
    [Obsolete($"Use the {nameof(ExecuteQuery)} method that accepts {nameof(ProtectedAccess)}. Will be removed in V14.")]
    PagedModel<Guid> ExecuteQuery(
        SelectorOption selectorOption,
        IList<FilterOption> filterOptions,
        IList<SortOption> sortOptions,
        string culture,
        bool preview,
        int skip,
        int take);

    /// <summary>
    ///     Returns a page of item ids that passed the search criteria.
    /// </summary>
    /// <param name="selectorOption">The selector option of the search criteria.</param>
    /// <param name="filterOptions">The filter options of the search criteria.</param>
    /// <param name="sortOptions">The sorting options of the search criteria.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="preview">Whether or not to search for preview content.</param>
    /// <param name="protectedAccess">Defines the limitations for querying protected content.</param>
    /// <param name="skip">Number of search results to skip (for pagination).</param>
    /// <param name="take">Number of search results to retrieve (for pagination).</param>
    /// <returns>A paged model containing the resulting IDs and the total number of results that matching the search criteria.</returns>
    PagedModel<Guid> ExecuteQuery(
        SelectorOption selectorOption,
        IList<FilterOption> filterOptions,
        IList<SortOption> sortOptions,
        string culture,
        ProtectedAccess protectedAccess,
        bool preview,
        int skip,
        int take) => new();

    /// <summary>
    ///     Returns a selector option that can be applied to fetch "all content" (i.e. if a selector option is not present when performing a search).
    /// </summary>
    SelectorOption AllContentSelectorOption();
}
