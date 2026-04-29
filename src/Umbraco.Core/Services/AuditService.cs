using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Implement;

/// <summary>
///     Audit service for logging and retrieving audit entries.
/// </summary>
public sealed class AuditService : AsyncRepositoryService, IAuditService
{
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityService _entityService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditService" /> class.
    /// </summary>
    public AuditService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        IUserIdKeyResolver userIdKeyResolver,
        IEntityService entityService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditRepository = auditRepository;
        _userIdKeyResolver = userIdKeyResolver;
        _entityService = entityService;
    }

    /// <inheritdoc />
    public async Task<Attempt<AuditLogOperationStatus>> AddAsync(
        AuditType type,
        Guid userKey,
        int objectId,
        string? entityType,
        string? comment = null,
        string? parameters = null)
    {
        int? userId = await _userIdKeyResolver.TryGetAsync(userKey) is { Success: true } userIdAttempt
            ? userIdAttempt.Result
            : null;
        if (userId is null)
        {
            return Attempt.Fail(AuditLogOperationStatus.UserNotFound);
        }

        return await AddInnerAsync(type, userId.Value, objectId, entityType, comment, parameters);
    }

    /// <inheritdoc />
    [Obsolete("Use AddAsync() instead. Scheduled for removal in Umbraco 19.")]
    public void Add(
        AuditType type,
        int userId,
        int objectId,
        string? entityType,
        string comment,
        string? parameters = null) =>
        AddInnerAsync(type, userId, objectId, entityType, comment, parameters).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<PagedModel<IAuditItem>> GetItemsAsync(
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        DateTimeOffset? sinceDate = null,
        AuditType[]? auditTypeFilter = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        return await GetItemsInnerAsync(skip, take, orderDirection, sinceDate?.UtcDateTime, auditTypeFilter);
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
    {
        PagedModel<IAuditItem> result = GetItemsInnerAsync(0, int.MaxValue, Direction.Ascending, sinceDate)
            .GetAwaiter().GetResult();
        return result.Items;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IAuditItem>> GetItemsByKeyAsync(
        Guid entityKey,
        UmbracoObjectTypes entityType,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        DateTimeOffset? sinceDate = null,
        AuditType[]? auditTypeFilter = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        Attempt<int> keyToIdAttempt = _entityService.GetId(entityKey, entityType);
        if (keyToIdAttempt.Success is false)
        {
            return new PagedModel<IAuditItem> { Items = [], Total = 0 };
        }

        return await GetItemsByEntityInnerAsync(
            keyToIdAttempt.Result,
            skip,
            take,
            orderDirection,
            sinceDate?.UtcDateTime,
            auditTypeFilter);
    }

    /// <inheritdoc />
    public async Task<PagedModel<IAuditItem>> GetItemsByEntityAsync(
        int entityId,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        if (entityId is Constants.System.Root or <= 0)
        {
            return new PagedModel<IAuditItem> { Items = [], Total = 0 };
        }

        // customFilter is no longer applied: the EF Core repository exposes explicit
        // filter parameters instead of an arbitrary IQuery. The parameter is preserved
        // for binary compatibility and will be removed in Umbraco 19.
        return await GetItemsByEntityInnerAsync(entityId, skip, take, orderDirection, sinceDate: null, auditTypeFilter);
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetPagedItemsByEntity(
        int entityId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        if (entityId is Constants.System.Root or <= 0)
        {
            totalRecords = 0L;
            return [];
        }

        var skip = (int)(pageIndex * pageSize);
        PagedModel<IAuditItem> result = GetItemsByEntityInnerAsync(
            entityId,
            skip,
            pageSize,
            orderDirection,
            sinceDate: null,
            auditTypeFilter)
            .GetAwaiter().GetResult();
        totalRecords = result.Total;

        return result.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetLogs(int objectId)
    {
        PagedModel<IAuditItem> result = GetItemsByEntityInnerAsync(
            objectId,
            0,
            int.MaxValue,
            Direction.Ascending,
            sinceDate: null,
            auditTypeFilter: null)
            .GetAwaiter().GetResult();
        return result.Items;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IAuditItem>> GetPagedItemsByUserAsync(
        Guid userKey,
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        DateTime? sinceDate = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        if (await _userIdKeyResolver.TryGetAsync(userKey) is not { Success: true } userIdAttempt)
        {
            return new PagedModel<IAuditItem>();
        }

        return await GetItemsByUserInnerAsync(
            userIdAttempt.Result,
            skip,
            take,
            orderDirection,
            sinceDate,
            auditTypeFilter);
    }

    /// <inheritdoc />
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetPagedItemsByUser(
        int userId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        if (userId is Constants.System.Root or <= 0)
        {
            totalRecords = 0L;
            return [];
        }

        var skip = (int)(pageIndex * pageSize);
        PagedModel<IAuditItem> items = GetItemsByUserInnerAsync(
            userId,
            skip,
            pageSize,
            orderDirection,
            sinceDate: null,
            auditTypeFilter)
            .GetAwaiter().GetResult();
        totalRecords = items.Total;

        return items.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
    {
        PagedModel<IAuditItem> result = GetItemsByUserInnerAsync(
            userId,
            0,
            int.MaxValue,
            Direction.Ascending,
            sinceDate,
            [type])
            .GetAwaiter().GetResult();
        return result.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use AuditEntryService.WriteAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IAuditEntry Write(
        int performingUserId,
        string perfomingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails)
    {
        // Use the static service provider to resolve the audit entry service, as this is only needed for this obsolete method.
        var auditEntryService =
            (AuditEntryService)StaticServiceProvider.Instance.GetRequiredService<IAuditEntryService>();

        return auditEntryService.WriteInner(
            performingUserId,
            perfomingDetails,
            performingIp,
            eventDateUtc,
            affectedUserId,
            affectedDetails,
            eventType,
            eventDetails).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task CleanLogsAsync(int maximumAgeOfLogsInMinutes)
        => CleanLogsInnerAsync(maximumAgeOfLogsInMinutes);

    /// <inheritdoc />
    [Obsolete("Use CleanLogsAsync() instead. Scheduled for removal in Umbraco 19.")]
    public void CleanLogs(int maximumAgeOfLogsInMinutes)
        => CleanLogsInnerAsync(maximumAgeOfLogsInMinutes).GetAwaiter().GetResult();

    private async Task<Attempt<AuditLogOperationStatus>> AddInnerAsync(
        AuditType type,
        int userId,
        int objectId,
        string? entityType,
        string? comment = null,
        string? parameters = null)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        await _auditRepository.SaveAsync(new AuditItem(objectId, type, userId, entityType, comment, parameters), CancellationToken.None);
        scope.Complete();

        return Attempt.Succeed(AuditLogOperationStatus.Success);
    }

    private async Task<PagedModel<IAuditItem>> GetItemsInnerAsync(
        int skip,
        int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        var paged = await _auditRepository.GetPagedAsync(skip, take, orderDirection, sinceDate, auditTypeFilter);
        scope.Complete();

        return paged;
    }

    private async Task<PagedModel<IAuditItem>> GetItemsByEntityInnerAsync(
        int entityId,
        int skip,
        int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        var paged = await _auditRepository.GetPagedForEntityAsync(entityId, skip, take, orderDirection, sinceDate, auditTypeFilter);
        scope.Complete();

        return paged;
    }

    private async Task<PagedModel<IAuditItem>> GetItemsByUserInnerAsync(
        int userId,
        int skip,
        int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null)
    {
        if (userId < Constants.Security.SuperUserId)
        {
            return new PagedModel<IAuditItem> { Items = [], Total = 0 };
        }

        using ICoreScope scope = ScopeProvider.CreateScope();
        var paged = await _auditRepository.GetPagedForUserAsync(userId, skip, take, orderDirection, sinceDate, auditTypeFilter);
        scope.Complete();

        return paged;
    }

    private async Task CleanLogsInnerAsync(int maximumAgeOfLogsInMinutes)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        await _auditRepository.CleanLogsAsync(maximumAgeOfLogsInMinutes);
        scope.Complete();
    }
}
