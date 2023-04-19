namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A handler that handles sort query parameters.
/// </summary>
public interface ISortHandler : IQueryHandler
{
    /// <summary>
    ///     Builds a <see cref="SortOption"/> for the sort query.
    /// </summary>
    /// <param name="sort">The sort query (i.e. "name:asc").</param>
    /// <returns>A <see cref="SortOption"/> that can be used when building specific sorting queries.</returns>
    SortOption BuildSortOption(string sort);
}
