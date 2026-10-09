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
    ///    Cleans the audit logs older than the specified maximum age.
    /// </summary>
    /// <param name="maximumAgeOfLogsInMinutes">The maximum age of logs in minutes.</param>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task CleanLogsAsync(int maximumAgeOfLogsInMinutes);
}
