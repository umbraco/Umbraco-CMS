using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IAuditItem" /> entities.
/// </summary>
public interface IAuditRepository : IAsyncReadWriteRepository<int, IAuditItem>
{
    /// <summary>
    ///     Cleans audit logs older than the specified maximum age.
    /// </summary>
    /// <param name="maximumAgeOfLogsInMinutes">The maximum age of logs in minutes.</param>
    Task CleanLogsAsync(int maximumAgeOfLogsInMinutes);

    Task<PagedModel<IAuditItem>> GetPagedAsync(
        int skip, int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null,
        CancellationToken cancellationToken = default);

    Task<PagedModel<IAuditItem>> GetPagedForEntityAsync(
        int entityId,
        int skip, int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null,
        CancellationToken cancellationToken = default);

    Task<PagedModel<IAuditItem>> GetPagedForUserAsync(
        int userId,
        int skip, int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null,
        CancellationToken cancellationToken = default);
}
