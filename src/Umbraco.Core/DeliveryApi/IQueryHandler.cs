using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Base interface for query handlers in the Delivery API.
/// </summary>
public interface IQueryHandler : IDiscoverable
{
    /// <summary>
    ///     Determines whether this query handler can handle the given query.
    /// </summary>
    /// <param name="query">The query string to check (i.e. "children:articles", "contentType:article", "name:asc", ...).</param>
    /// <returns>True if this query handler can handle the given query; otherwise, false.</returns>
    bool CanHandle(string query);
}
