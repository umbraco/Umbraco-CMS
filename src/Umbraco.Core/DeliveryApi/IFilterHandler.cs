namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A handler that handles filter query parameters.
/// </summary>
public interface IFilterHandler : IQueryHandler
{
    /// <summary>
    ///     Builds a <see cref="FilterOption"/> for the filter query.
    /// </summary>
    /// <param name="filter">The filter query (i.e. "contentType:article").</param>
    /// <returns>A <see cref="FilterOption"/> that can be used when building specific filter queries.</returns>
    FilterOption BuildFilterOption(string filter);
}
