namespace Umbraco.Cms.Core.ContentApi;

/// <summary>
///     Service that handles querying of the Content API.
/// </summary>
public interface IApiQueryService
{
    /// <summary>
    ///     Returns a collection of item ids that passed the search criteria.
    /// </summary>
    /// <param name="fetch">Optional fetch query parameter value.</param>
    /// <param name="filters">Optional filter query parameters values.</param>
    /// <param name="sorts">Optional sort query parameters values.</param>
    /// <returns>A collection of item ids that are returned after applying the search queries.</returns>
    IEnumerable<Guid> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts);
}
