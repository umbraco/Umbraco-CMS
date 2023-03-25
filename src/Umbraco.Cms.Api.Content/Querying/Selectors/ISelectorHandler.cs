using Examine.Search;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Querying.Selectors;

public interface ISelectorHandler : IQueryHandler
{
    /// <summary>
    ///     Builds an Examine query for the given selector query string.
    /// </summary>
    /// <param name="query">The base query to build upon.</param>
    /// <param name="queryString">The selector query string.</param>
    /// <returns>An <see cref="IBooleanOperation"/> representing the Examine query to execute, or null if the query cannot be built.</returns>
    IBooleanOperation? BuildApiIndexQuery(IQuery query, string queryString);
}
