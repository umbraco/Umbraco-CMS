namespace Umbraco.Cms.Core.ContentApi;

public interface IApiQueryService
{
    /// <summary>
    ///     Gets <see cref="ApiQueryType" /> corresponding to a query string value.
    /// </summary>
    ApiQueryType GetQueryType(string queryOption);

    /// <summary>
    ///     Extracts Guid from query.
    /// </summary>
    Guid? GetGuidFromFetch(string fetchQuery);

    /// <summary>
    ///     Gets a collection of Guid from querying the Content API using a specific <see cref="ApiQueryType" /> for a given id.
    /// </summary>
    IEnumerable<Guid> GetGuidsFromQuery(Guid id, ApiQueryType queryType);
}

/// <summary>
///     Defines the possible query types for the Content API.
/// </summary>
public enum ApiQueryType
{
    Ancestors,
    Children,
    Descendants,
    Unknown // Custom
}
