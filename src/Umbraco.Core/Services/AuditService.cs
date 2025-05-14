using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Implement;

/// <summary>
///     Audit service for logging and retrieving audit entries.
/// </summary>
public sealed class AuditService : RepositoryService, IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditService" /> class.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public AuditService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        IUserService userService,
        IEntityService entityService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditRepository = auditRepository;
        _userService = userService;
        _entityService = entityService;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditService" /> class.
    /// </summary>
    [Obsolete("Use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public AuditService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        IAuditEntryRepository auditEntryRepository,
        IUserService userService,
        IEntityService entityService)
        : this(provider, loggerFactory, eventMessagesFactory, auditRepository, userService, entityService)
    {
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
        var userId = userKey switch
        {
            { } when userKey == Constants.Security.UnknownUserKey => Constants.Security.UnknownUserId,
            _ => (await _userService.GetAsync(userKey))?.Id,
        };

        if (userId is null)
        {
            return Attempt.Fail(AuditLogOperationStatus.UserNotFound);
        }

        return AddInner(type, userId.Value, objectId, entityType, comment, parameters);
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
        AddInner(type, userId, objectId, entityType, comment, parameters);

    /// <inheritdoc />
    public Task<PagedModel<IAuditItem>> GetItemsAsync(
        int skip,
        int take,
        Direction orderDirection = Direction.Descending,
        DateTimeOffset? sinceDate = null,
        AuditType[]? auditTypeFilter = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue
            ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate)
            : null;

        PagedModel<IAuditItem> result = GetItemsInner(
            null,
            pageIndex,
            pageSize,
            orderDirection,
            auditTypeFilter,
            customFilter);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
    {
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue
            ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate)
            : null;

        PagedModel<IAuditItem> result = GetItemsInner(
            null,
            0,
            int.MaxValue,
            Direction.Ascending,
            customFilter: customFilter);
        return result.Items;
    }

    /// <inheritdoc />
    public Task<PagedModel<IAuditItem>> GetItemsByKeyAsync(
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
            return Task.FromResult(new PagedModel<IAuditItem> { Items = [], Total = 0 });
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        IQuery<IAuditItem>? customFilter =
            sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;

        PagedModel<IAuditItem> result = GetItemsByEntityInner(
            keyToIdAttempt.Result,
            pageIndex,
            pageSize,
            orderDirection,
            auditTypeFilter,
            customFilter);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<PagedModel<IAuditItem>> GetItemsByEntityAsync(
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
            return Task.FromResult(new PagedModel<IAuditItem> { Items = [], Total = 0 });
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        PagedModel<IAuditItem> result = GetItemsByEntityInner(
            entityId,
            pageIndex,
            pageSize,
            orderDirection,
            auditTypeFilter,
            customFilter);

        return Task.FromResult(result);
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

        PagedModel<IAuditItem> result = GetItemsByEntityInner(
            entityId,
            pageIndex,
            pageSize,
            orderDirection,
            auditTypeFilter,
            customFilter);
        totalRecords = result.Total;

        return result.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetLogs(int objectId)
    {
        PagedModel<IAuditItem> result = GetItemsByEntityInner(objectId, 0, int.MaxValue, Direction.Ascending);
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

        IUser? user = await _userService.GetAsync(userKey);
        if (user is null)
        {
            return new PagedModel<IAuditItem>();
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        IQuery<IAuditItem>? customFilter =
            sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;

        PagedModel<IAuditItem> result = GetItemsByUserInner(
            user.Id,
            pageIndex,
            pageSize,
            orderDirection,
            auditTypeFilter,
            customFilter);
        return result;
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

        PagedModel<IAuditItem> items = GetItemsByUserInner(
            userId,
            pageIndex,
            pageSize,
            orderDirection,
            auditTypeFilter,
            customFilter);
        totalRecords = items.Total;

        return items.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
    {
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue
            ? Query<IAuditItem>().Where(x => x.AuditType == type && x.CreateDate >= sinceDate)
            : null;
        PagedModel<IAuditItem> result = GetItemsByUserInner(
            userId,
            0,
            int.MaxValue,
            Direction.Ascending,
            [type],
            customFilter);
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
            eventDetails).Result;
    }

    /// <inheritdoc />
    public Task CleanLogsAsync(int maximumAgeOfLogsInMinutes)
    {
        CleanLogsInner(maximumAgeOfLogsInMinutes);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    [Obsolete("Use CleanLogsAsync() instead. Scheduled for removal in Umbraco 19.")]
    public void CleanLogs(int maximumAgeOfLogsInMinutes)
        => CleanLogsInner(maximumAgeOfLogsInMinutes);

    private Attempt<AuditLogOperationStatus> AddInner(
        AuditType type,
        int userId,
        int objectId,
        string? entityType,
        string? comment = null,
        string? parameters = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, comment, parameters));
        scope.Complete();

        return Attempt.Succeed(AuditLogOperationStatus.Success);
    }

    private PagedModel<IAuditItem> GetItemsInner(
        IQuery<IAuditItem>? query,
        long pageIndex,
        int pageSize,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEnumerable<IAuditItem> auditItems = _auditRepository.GetPagedResultsByQuery(
                query ?? Query<IAuditItem>(),
                pageIndex,
                pageSize,
                out var totalRecords,
                orderDirection,
                auditTypeFilter,
                customFilter);
            return new PagedModel<IAuditItem> { Items = auditItems, Total = totalRecords };
        }
    }

    private PagedModel<IAuditItem> GetItemsByEntityInner(
        int entityId,
        long pageIndex,
        int pageSize,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.Id == entityId);

        PagedModel<IAuditItem> result = GetItemsInner(
            query,
            pageIndex,
            pageSize,
            orderDirection,
            auditTypeFilter,
            customFilter);
        return result;
    }

    private PagedModel<IAuditItem> GetItemsByUserInner(
        int userId,
        long pageIndex,
        int pageSize,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        if (userId is < Constants.Security.SuperUserId)
        {
            return new PagedModel<IAuditItem> { Items = [], Total = 0 };
        }

        IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.UserId == userId);
        return GetItemsInner(query, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter);
    }

    private void CleanLogsInner(int maximumAgeOfLogsInMinutes)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        _auditRepository.CleanLogs(maximumAgeOfLogsInMinutes);
        scope.Complete();
    }
}
