namespace Umbraco.Cms.Core.ContentApi;

public interface IApiQueryService
{
    /// <summary>
    ///     Gets <see cref="ApiQueryType" /> corresponding to a query string value.
    /// </summary>
    ApiQueryType GetQueryType(string queryOption);

    /// <summary>
    ///     Gets a collection of Guid from querying the Content API using a specific <see cref="ApiQueryType" /> for a given id.
    /// </summary>
    IEnumerable<Guid> GetGuidsFromQuery(Guid id, ApiQueryType queryType);

    IEnumerable<Guid> ExecuteQuery(string query, string fieldValue);
}

/// <summary>
///     ToDo: Remove
///     Defines the possible query types for the Content API.
/// </summary>
public enum ApiQueryType
{
    Ancestors,
    Unknown // Custom
}
