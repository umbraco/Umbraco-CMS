namespace Umbraco.Cms.Core.ContentApi;

/// <summary>
///     A handler that handles filter query parameters.
/// </summary>
public interface IFilterHandler : IQueryHandler
{
    /// <summary>
    ///     Builds a <see cref="FilterOption"/> for the filter query parameter.
    /// </summary>
    /// <param name="filterValueString">The filter value from the query string.</param>
    /// <returns>A <see cref="FilterOption"/> that can be used when building specific filter queries.</returns>
    FilterOption BuildFilterOption(string filterValueString);
}
