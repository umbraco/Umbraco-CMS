namespace Umbraco.Cms.Core.ContentApi;

/// <summary>
///     A handler that handles sort query parameters.
/// </summary>
public interface ISortHandler : IQueryHandler
{
    /// <summary>
    ///     Builds a <see cref="SortOption"/> for the sort query parameter.
    /// </summary>
    /// <param name="sortValueString">The sort value from the query string.</param>
    /// <returns>A <see cref="SortOption"/> that can be used when building specific sorting queries.</returns>
    SortOption BuildSortOption(string sortValueString);
}
