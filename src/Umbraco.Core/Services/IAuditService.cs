using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents a service for handling audit.
/// </summary>
public interface IAuditService : IService
{
    /// <summary>
    ///    Adds an audit entry.
    /// </summary>
    /// <param name="type">The type of the audit.</param>
    /// <param name="userKey">The key of the user triggering the event.</param>
    /// <param name="objectId">The identifier of the affected object.</param>
    /// <param name="entityType">The entity type of the affected object.</param>
    /// <param name="comment">The comment associated with the audit entry.</param>
    /// <param name="parameters">The parameters associated with the audit entry.</param>
    /// <returns>Result of the add audit log operation.</returns>
    public Task<Attempt<AuditLogOperationStatus>> AddAsync(
        AuditType type,
        Guid userKey,
        int objectId,
        string? entityType,
        string? comment = null,
        string? parameters = null) => throw new NotImplementedException();

    /// <summary>
    /// Adds an audit log entry.
    /// </summary>
    /// <param name="type">The type of audit entry.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <param name="objectId">The identifier of the object being audited.</param>
    /// <param name="entityType">The type of entity being audited.</param>
    /// <param name="comment">A comment describing the action.</param>
    /// <param name="parameters">Optional parameters associated with the audit entry.</param>
    [Obsolete("Use AddAsync() instead. Scheduled for removal in Umbraco 19.")]
    void Add(AuditType type, int userId, int objectId, string? entityType, string comment, string? parameters = null);

    /// <summary>
    ///    Returns paged items in the audit trail.
    /// </summary>
    /// <param name="skip">The number of audit trail entries to skip.</param>
    /// <param name="take">The number of audit trail entries to take.</param>
    /// <param name="orderDirection">
    ///     By default, this will always be ordered descending (newest first).
    /// </param>
    /// <param name="sinceDate">
    ///     If populated, will only return entries after this time.
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter, so we need to do that here.
    /// </param>
    /// <returns>The paged audit logs.</returns>
    public Task<PagedModel<IAuditItem>> GetItemsAsync(
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        DateTimeOffset? sinceDate = null,
        AuditType[]? auditTypeFilter = null) => throw new NotImplementedException();

    /// <summary>
    /// Gets audit logs filtered by audit type.
    /// </summary>
    /// <param name="type">The audit type to filter by.</param>
    /// <param name="sinceDate">Optional date to filter logs since.</param>
    /// <returns>A collection of audit items matching the criteria.</returns>
    [Obsolete("Use GetItemsAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IAuditItem> GetLogs(AuditType type, DateTime? sinceDate = null);

    /// <summary>
    ///     Returns paged items in the audit trail for a given entity.
    /// </summary>
    /// <param name="entityKey">The key of the entity.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="skip">The number of audit trail entries to skip.</param>
    /// <param name="take">The number of audit trail entries to take.</param>
    /// <param name="orderDirection">
    ///     By default, this will always be ordered descending (newest first).
    /// </param>
    /// <param name="sinceDate">
    ///     If populated, will only return entries after this time.
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter, so we need to do that here.
    /// </param>
    /// <returns>The paged items in the audit trail for the specified entity.</returns>
    Task<PagedModel<IAuditItem>> GetItemsByKeyAsync(
        Guid entityKey,
        UmbracoObjectTypes entityType,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        DateTimeOffset? sinceDate = null,
        AuditType[]? auditTypeFilter = null) => throw new NotImplementedException();

    /// <summary>
    ///     Returns paged items in the audit trail for a given entity.
    /// </summary>
    /// <param name="entityId">The identifier of the entity.</param>
    /// <param name="skip">The number of audit trail entries to skip.</param>
    /// <param name="take">The number of audit trail entries to take.</param>
    /// <param name="orderDirection">
    ///     By default, this will always be ordered descending (newest first).
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter, so we need to do that here.
    /// </param>
    /// <param name="customFilter">
    ///     Optional filter to be applied.
    /// </param>
    /// <returns>The paged items in the audit trail for the specified entity.</returns>
    public Task<PagedModel<IAuditItem>> GetItemsByEntityAsync(
        int entityId,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null) => throw new NotImplementedException();

    /// <summary>
    ///     Returns paged items in the audit trail for a given entity.
    /// </summary>
    /// <param name="entityId">The identifier of the entity.</param>
    /// <param name="pageIndex">The index of tha page (pagination).</param>
    /// <param name="pageSize">The number of results to return.</param>
    /// <param name="totalRecords">The total number of records.</param>
    /// <param name="orderDirection">
    ///     By default, this will always be ordered descending (newest first).
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter, so we need to do that here.
    /// </param>
    /// <param name="customFilter">
    ///     Optional filter to be applied.
    /// </param>
    /// <returns>The paged items in the audit trail for the specified entity.</returns>
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IAuditItem> GetPagedItemsByEntity(
        int entityId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null);

    /// <summary>
    /// Gets audit logs for a specific object.
    /// </summary>
    /// <param name="objectId">The identifier of the object to get logs for.</param>
    /// <returns>A collection of audit items for the specified object.</returns>
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IAuditItem> GetLogs(int objectId);

    /// <summary>
    ///     Returns paged items in the audit trail for a given user.
    /// </summary>
    /// <param name="userKey">The key of the user.</param>
    /// <param name="skip">The number of audit trail entries to skip.</param>
    /// <param name="take">The number of audit trail entries to take.</param>
    /// <param name="orderDirection">
    ///     By default, this will always be ordered descending (newest first).
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter, so we need to do that here.
    /// </param>
    /// <param name="sinceDate">The date to filter the audit entries.</param>
    /// <returns>The paged items in the audit trail for the specified user.</returns>
    Task<PagedModel<IAuditItem>> GetPagedItemsByUserAsync(
        Guid userKey,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        DateTime? sinceDate = null) => throw new NotImplementedException();

    /// <summary>
    ///     Returns paged items in the audit trail for a given user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderDirection">
    ///     By default this will always be ordered descending (newest first).
    /// </param>
    /// <param name="auditTypeFilter">
    ///     Since we currently do not have enum support with our expression parser, we cannot query on AuditType in the query
    ///     or the custom filter
    ///     so we need to do that here.
    /// </param>
    /// <param name="customFilter">
    ///     Optional filter to be applied.
    /// </param>
    /// <returns></returns>
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IAuditItem> GetPagedItemsByUser(
        int userId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null);

    /// <summary>
    /// Gets audit logs for a specific user filtered by audit type.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="type">The audit type to filter by.</param>
    /// <param name="sinceDate">Optional date to filter logs since.</param>
    /// <returns>A collection of audit items for the specified user matching the criteria.</returns>
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null);

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
    /// <returns>The created audit entry.</returns>
    [Obsolete("Use IAuditEntryService.WriteAsync() instead. Scheduled for removal in Umbraco 19.")]
    IAuditEntry Write(
        int performingUserId,
        string perfomingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string affectedDetails,
        string eventType,
        string eventDetails);

    /// <summary>
    ///    Cleans the audit logs older than the specified maximum age.
    /// </summary>
    /// <param name="maximumAgeOfLogsInMinutes">The maximum age of logs in minutes.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    public Task CleanLogsAsync(int maximumAgeOfLogsInMinutes) => throw new NotImplementedException();

    /// <summary>
    /// Cleans audit logs older than the specified maximum age.
    /// </summary>
    /// <param name="maximumAgeOfLogsInMinutes">The maximum age of logs to keep, in minutes.</param>
    [Obsolete("Use CleanLogsAsync() instead. Scheduled for removal in Umbraco 19.")]
    void CleanLogs(int maximumAgeOfLogsInMinutes);
}
