using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents a service for handling audit.
/// </summary>
public interface IAuditService : IService
{
    void Add(AuditType type, int userId, int objectId, string? entityType, string comment, string? parameters = null);

    IEnumerable<IAuditItem> GetLogs(int objectId);

    IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null);

    IEnumerable<IAuditItem> GetLogs(AuditType type, DateTime? sinceDate = null);

    void CleanLogs(int maximumAgeOfLogsInMinutes);

    /// <summary>
    ///     Returns paged items in the audit trail for a given entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderDirection">
    ///     By default this will always be ordered descending (newest first)
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter
    ///     so we need to do that here
    /// </param>
    /// <param name="customFilter">
    ///     Optional filter to be applied
    /// </param>
    /// <returns></returns>
    IEnumerable<IAuditItem> GetPagedItemsByEntity(
        int entityId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null);

    /// <summary>
    ///     Returns paged items in the audit trail for a given user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderDirection">
    ///     By default this will always be ordered descending (newest first)
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter
    ///     so we need to do that here
    /// </param>
    /// <param name="customFilter">
    ///     Optional filter to be applied
    /// </param>
    /// <returns></returns>
    IEnumerable<IAuditItem> GetPagedItemsByUser(
        int userId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null);

    /// <summary>
    ///     Returns paged items in the audit trail for a given entity
    /// </summary>
    /// <param name="entityKey">The key of the entity</param>
    /// <param name="entityType">The entity type</param>
    /// <param name="skip">The amount of audit trail entries to skip</param>
    /// <param name="take">The amount of audit trail entries to take</param>
    /// <param name="orderDirection">
    ///     By default this will always be ordered descending (newest first)
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter
    ///     so we need to do that here
    /// </param>
    /// <param name="sinceDate">
    ///     If populated, will only return entries after this time.
    /// </param>
    /// <returns></returns>
    Task<PagedModel<IAuditItem>> GetItemsByKeyAsync(
        Guid entityKey,
        UmbracoObjectTypes entityType,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        DateTimeOffset? sinceDate = null,
        AuditType[]? auditTypeFilter = null) => throw new NotImplementedException();

    /// <summary>
    ///     Returns paged items in the audit trail for a given user
    /// </summary>
    /// <param name="userKey"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="orderDirection">
    ///     By default this will always be ordered descending (newest first)
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter
    ///     so we need to do that here
    /// </param>
    /// <param name="sinceDate"></param>
    /// <returns></returns>
    Task<PagedModel<IAuditItem>> GetPagedItemsByUserAsync(
        Guid userKey,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        DateTime? sinceDate = null) => throw new NotImplementedException();

    /// <summary>
    ///     Writes an audit entry for an audited event.
    /// </summary>
    /// <param name="performingUserId">The identifier of the user triggering the audited event.</param>
    /// <param name="perfomingDetails">Free-form details about the user triggering the audited event.</param>
    /// <param name="performingIp">The IP address or the request triggering the audited event.</param>
    /// <param name="eventDateUtc">The date and time of the audited event.</param>
    /// <param name="affectedUserId">The identifier of the user affected by the audited event.</param>
    /// <param name="affectedDetails">Free-form details about the entity affected by the audited event.</param>
    /// <param name="eventType">
    ///     The type of the audited event - must contain only alphanumeric chars and hyphens with forward slashes separating
    ///     categories.
    ///     <example>
    ///         The eventType will generally be formatted like: {application}/{entity-type}/{category}/{sub-category}
    ///         Example: umbraco/user/sign-in/failed
    ///     </example>
    /// </param>
    /// <param name="eventDetails">Free-form details about the audited event.</param>
    [Obsolete("Will be moved to a new service in V17. Scheduled for removal in V18.")]
    IAuditEntry Write(
        int performingUserId,
        string perfomingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string affectedDetails,
        string eventType,
        string eventDetails);
}
