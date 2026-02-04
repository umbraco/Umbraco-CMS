using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IAuditItem" /> entities.
/// </summary>
public interface IAuditRepository : IReadRepository<int, IAuditItem>, IWriteRepository<IAuditItem>,
    IQueryRepository<IAuditItem>
{
    /// <summary>
    ///     Cleans audit logs older than the specified maximum age.
    /// </summary>
    /// <param name="maximumAgeOfLogsInMinutes">The maximum age of logs in minutes.</param>
    void CleanLogs(int maximumAgeOfLogsInMinutes);

    /// <summary>
    ///     Return the audit items as paged result
    /// </summary>
    /// <param name="query">
    ///     The query coming from the service
    /// </param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderDirection"></param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter
    ///     so we need to do that here
    /// </param>
    /// <param name="customFilter">
    ///     A user supplied custom filter
    /// </param>
    /// <returns></returns>
    IEnumerable<IAuditItem> GetPagedResultsByQuery(
        IQuery<IAuditItem> query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection,
        AuditType[]? auditTypeFilter,
        IQuery<IAuditItem>? customFilter);

    /// <summary>
    ///     Gets audit items by type and query.
    /// </summary>
    /// <param name="type">The audit type to filter by.</param>
    /// <param name="query">The query to apply.</param>
    /// <returns>A collection of <see cref="IAuditItem" /> objects.</returns>
    IEnumerable<IAuditItem> Get(AuditType type, IQuery<IAuditItem> query);
}
