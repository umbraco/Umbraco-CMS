using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Implement;

/// <summary>
/// Audit service for logging and retrieving audit entries.
/// </summary>
public sealed class AuditService : RepositoryService, IAuditService
{
    private readonly IAuditEntryRepository _auditEntryRepository;
    private readonly IUserService _userService;
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityService _entityService;
    private readonly Lazy<bool> _isAvailable;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    public AuditService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        IAuditEntryRepository auditEntryRepository,
        IUserService userService,
        IEntityService entityService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _auditRepository = auditRepository;
        _auditEntryRepository = auditEntryRepository;
        _userService = userService;
        _entityService = entityService;
        _isAvailable = new Lazy<bool>(DetermineIsAvailable);
    }

    /// <inheritdoc />
    public Task<Attempt<AuditLogOperationStatus>> AddAsync(
        AuditType type,
        int userId,
        int objectId,
        string? entityType,
        string comment,
        string? parameters = null) =>
        Task.FromResult(AddInner(type, userId, objectId, entityType, comment, parameters));

    /// <inheritdoc />
    [Obsolete("Use AddAsync() instead. Scheduled for removal in Umbraco 18.")]
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
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue
            ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate)
            : null;

        PagedModel<IAuditItem> result = GetItemsInner(query: null, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsAsync() instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<IAuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
    {
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue
            ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate)
            : null;

        PagedModel<IAuditItem> result = GetItemsInner(query: null, 0, int.MaxValue, Direction.Ascending, customFilter: customFilter);
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
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        Attempt<int> keyToIdAttempt = _entityService.GetId(entityKey, entityType);
        if (keyToIdAttempt.Success is false)
        {
            return Task.FromResult(new PagedModel<IAuditItem> { Items = [], Total = 0 });
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;

        PagedModel<IAuditItem> result = GetItemsByEntityInner(keyToIdAttempt.Result, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter);
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
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        if (entityId is Constants.System.Root or <= 0)
        {
            return Task.FromResult(new PagedModel<IAuditItem> { Items = [], Total = 0 });
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        PagedModel<IAuditItem> result = GetItemsByEntityInner(entityId, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<IAuditItem> GetPagedItemsByEntity(
        int entityId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (entityId is Constants.System.Root or <= 0)
        {
            totalRecords = 0L;
            return [];
        }

        PagedModel<IAuditItem> result = GetItemsByEntityInner(entityId, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter);
        totalRecords = result.Total;

        return result.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 18.")]
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
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        IUser? user = await _userService.GetAsync(userKey);
        if (user is null)
        {
            return new PagedModel<IAuditItem>();
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageIndex, out var pageSize);
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;

        PagedModel<IAuditItem> result = GetItemsByUserInner(user.Id, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter);
        return result;
    }

    /// <inheritdoc />
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<IAuditItem> GetPagedItemsByUser(
        int userId,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        if (pageIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (userId is Constants.System.Root or <= 0)
        {
            totalRecords = 0L;
            return [];
        }

        PagedModel<IAuditItem> items = GetItemsByUserInner(userId, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter);
        totalRecords = items.Total;

        return items.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
    {
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.AuditType == type && x.CreateDate >= sinceDate) : null;
        PagedModel<IAuditItem> result = GetItemsByUserInner(userId, 0, int.MaxValue, Direction.Ascending, auditTypeFilter: [type], customFilter: customFilter);
        return result.Items;
    }

    /// <inheritdoc />
    [Obsolete("Use WriteAsync() instead. Scheduled for removal in Umbraco 18.")]
    public IAuditEntry Write(
        int performingUserId,
        string perfomingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails) =>
            WriteInner(
                performingUserId,
                perfomingDetails,
                performingIp,
                eventDateUtc,
                affectedUserId,
                affectedDetails,
                eventType,
                eventDetails).Result;

    /// <inheritdoc />
    public Task<Attempt<IAuditEntry, AuditLogOperationStatus>> WriteAsync(
        int performingUserId,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails) =>
        Task.FromResult(
            WriteInner(
            performingUserId,
            performingDetails,
            performingIp,
            eventDateUtc,
            affectedUserId,
            affectedDetails,
            eventType,
            eventDetails));

    /// <inheritdoc />
    public Task CleanLogsAsync(int maximumAgeOfLogsInMinutes)
    {
        CleanLogsInner(maximumAgeOfLogsInMinutes);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    [Obsolete("Use CleanLogsAsync() instead. Scheduled for removal in Umbraco 18.")]
    public void CleanLogs(int maximumAgeOfLogsInMinutes)
        => CleanLogsInner(maximumAgeOfLogsInMinutes);

    // TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
    internal IEnumerable<IAuditEntry> GetAll()
    {
        if (_isAvailable.Value == false)
        {
            return [];
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.GetMany();
        }
    }

    private Attempt<AuditLogOperationStatus> AddInner(
        AuditType type,
        int userId,
        int objectId,
        string? entityType,
        string comment,
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

    private Attempt<IAuditEntry, AuditLogOperationStatus> WriteInner(
        int performingUserId,
        string performingDetails,
        string performingIp,
        DateTime eventDateUtc,
        int affectedUserId,
        string? affectedDetails,
        string eventType,
        string eventDetails)
    {
        if (performingUserId < Constants.Security.SuperUserId)
        {
            throw new ArgumentOutOfRangeException(nameof(performingUserId));
        }

        if (string.IsNullOrWhiteSpace(performingDetails))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(performingDetails));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(eventDetails))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(eventDetails));
        }

        // we need to truncate the data else we'll get SQL errors
        affectedDetails =
            affectedDetails?[..Math.Min(affectedDetails.Length, Constants.Audit.DetailsLength)];
        eventDetails = eventDetails[..Math.Min(eventDetails.Length, Constants.Audit.DetailsLength)];

        // validate the eventType - must contain a forward slash, no spaces, no special chars
        var eventTypeParts = eventType.ToCharArray();
        if (eventTypeParts.Contains('/') == false ||
            eventTypeParts.All(c => char.IsLetterOrDigit(c) || c == '/' || c == '-') == false)
        {
            throw new ArgumentException(
                nameof(eventType) + " must contain only alphanumeric characters, hyphens and at least one '/' defining a category");
        }

        if (eventType.Length > Constants.Audit.EventTypeLength)
        {
            throw new ArgumentException($"Must be max {Constants.Audit.EventTypeLength} chars.", nameof(eventType));
        }

        if (performingIp is { Length: > Constants.Audit.IpLength })
        {
            throw new ArgumentException($"Must be max {Constants.Audit.EventTypeLength} chars.", nameof(performingIp));
        }

        var entry = new AuditEntry
        {
            PerformingUserId = performingUserId,
            PerformingDetails = performingDetails,
            PerformingIp = performingIp,
            EventDateUtc = eventDateUtc,
            AffectedUserId = affectedUserId,
            AffectedDetails = affectedDetails,
            EventType = eventType,
            EventDetails = eventDetails,
        };

        if (_isAvailable.Value == false)
        {
            return Attempt.SucceedWithStatus(AuditLogOperationStatus.Success, (IAuditEntry)entry);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditEntryRepository.Save(entry);
            scope.Complete();
        }

        return Attempt.SucceedWithStatus(AuditLogOperationStatus.Success, (IAuditEntry)entry);
    }

    private void CleanLogsInner(int maximumAgeOfLogsInMinutes)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        _auditRepository.CleanLogs(maximumAgeOfLogsInMinutes);
        scope.Complete();
    }

    /// <summary>
    ///     Determines whether the repository is available.
    /// </summary>
    private bool DetermineIsAvailable()
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.IsAvailable();
        }
    }
}
