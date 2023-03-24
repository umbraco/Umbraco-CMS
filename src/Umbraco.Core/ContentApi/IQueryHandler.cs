using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public interface IQueryHandler : IDiscoverable
{
    /// <summary>
    ///     Determines whether this query handler can handle the given query string.
    /// </summary>
    /// <param name="queryString">The query string to check.</param>
    /// <returns>True if this query handler can handle the given query string; otherwise, false.</returns>
    bool CanHandle(string queryString);
}
