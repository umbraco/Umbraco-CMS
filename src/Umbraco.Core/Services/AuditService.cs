using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Implement;

public sealed class AuditService : RepositoryService, IAuditService
{
    private readonly IAuditEntryRepository _auditEntryRepository;
    private readonly IUserService _userService;
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityService _entityService;
    private readonly Lazy<bool> _isAvailable;

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
    [Obsolete("Use AddAsync() instead. Scheduled for removal in Umbraco 18.")]
    public void Add(
        AuditType type,
        int userId,
        int objectId,
        string? entityType,
        string comment,
        string? parameters = null) =>
        AddAsync(type, userId, objectId, entityType, comment, parameters).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task<Attempt<AuditLogOperationStatus>> AddAsync(
        AuditType type,
        int userId,
        int objectId,
        string? entityType,
        string comment,
        string? parameters = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, comment, parameters));
            scope.Complete();

            return Task.FromResult(Attempt.Succeed(AuditLogOperationStatus.Success));
        }
    }

    public IEnumerable<IAuditItem> GetLogs(AuditType type, DateTime? sinceDate = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IEnumerable<IAuditItem> result = sinceDate.HasValue == false
                ? _auditRepository.Get(type, Query<IAuditItem>())
                : _auditRepository.Get(type, Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate.Value));
            scope.Complete();
            return result;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use GetItemsByEntityAsync() instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<IAuditItem> GetLogs(int objectId)
    {
        // TODO: Call GetItemsByEntityAsync() - check whether it causes a behavioral change
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IEnumerable<IAuditItem> result = _auditRepository.Get(Query<IAuditItem>().Where(x => x.Id == objectId));
            scope.Complete();
            return result;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use GetPagedItemsByUserAsync() instead. Scheduled for removal in Umbraco 18.")]
    public IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
    {
        // TODO: Call GetPagedItemsByUserAsync() - check whether it causes a behavioral change
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IEnumerable<IAuditItem> result = sinceDate.HasValue == false
                ? _auditRepository.Get(type, Query<IAuditItem>().Where(x => x.UserId == userId))
                : _auditRepository.Get(
                    type,
                    Query<IAuditItem>().Where(x => x.UserId == userId && x.CreateDate >= sinceDate.Value));
            scope.Complete();
            return result;
        }
    }

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

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;

        return GetItemsAsyncInner(Query<IAuditItem>(), pageNumber, pageSize, orderDirection, auditTypeFilter, customFilter);
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

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);
        IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.Id == keyToIdAttempt.Result);
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;

        return GetItemsAsyncInner(query, pageNumber, pageSize, orderDirection, auditTypeFilter, customFilter);
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

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);
        IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.Id == entityId);

        return GetItemsAsyncInner(query, pageNumber, pageSize, orderDirection, auditTypeFilter, customFilter);
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

        IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.Id == entityId);

        PagedModel<IAuditItem> items = GetItemsAsyncInner(query, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter).GetAwaiter().GetResult();
        totalRecords = items.Total;

        return items.Items;
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

        IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.UserId == userId);

        PagedModel<IAuditItem> items = GetItemsAsyncInner(query, pageIndex, pageSize, orderDirection, auditTypeFilter, customFilter).GetAwaiter().GetResult();
        totalRecords = items.Total;

        return items.Items;
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

        IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.UserId == user.Id);
        IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        return await GetItemsAsyncInner(query, pageNumber, pageSize, orderDirection, auditTypeFilter, customFilter);
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
            WriteAsync(
                performingUserId,
                perfomingDetails,
                performingIp,
                eventDateUtc,
                affectedUserId,
                affectedDetails,
                eventType,
                eventDetails).GetAwaiter().GetResult().Result;

    /// <inheritdoc />
    public Task<Attempt<IAuditEntry, AuditLogOperationStatus>> WriteAsync(int performingUserId, string perfomingDetails, string performingIp, DateTime eventDateUtc, int affectedUserId, string? affectedDetails, string eventType, string eventDetails)
    {
        if (performingUserId < 0 && performingUserId != Constants.Security.SuperUserId)
        {
            throw new ArgumentOutOfRangeException(nameof(performingUserId));
        }

        if (string.IsNullOrWhiteSpace(perfomingDetails))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(perfomingDetails));
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
            throw new ArgumentException(nameof(eventType) +
                                        " must contain only alphanumeric characters, hyphens and at least one '/' defining a category");
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
            PerformingDetails = perfomingDetails,
            PerformingIp = performingIp,
            EventDateUtc = eventDateUtc,
            AffectedUserId = affectedUserId,
            AffectedDetails = affectedDetails,
            EventType = eventType,
            EventDetails = eventDetails,
        };

        if (_isAvailable.Value == false)
        {
            return Task.FromResult(Attempt.SucceedWithStatus(AuditLogOperationStatus.Success, (IAuditEntry)entry));
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditEntryRepository.Save(entry);
            scope.Complete();
        }

        return Task.FromResult(Attempt.SucceedWithStatus(AuditLogOperationStatus.Success, (IAuditEntry)entry));
    }

    /// <inheritdoc />
    [Obsolete("Use CleanLogsAsync() instead. Scheduled for removal in Umbraco 18.")]
    public void CleanLogs(int maximumAgeOfLogsInMinutes)
        => CleanLogsAsync(maximumAgeOfLogsInMinutes).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task CleanLogsAsync(int maximumAgeOfLogsInMinutes)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditRepository.CleanLogs(maximumAgeOfLogsInMinutes);
            scope.Complete();
        }

        return Task.CompletedTask;
    }

    // TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
    internal IEnumerable<IAuditEntry>? GetAll()
    {
        if (_isAvailable.Value == false)
        {
            return Enumerable.Empty<IAuditEntry>();
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.GetMany();
        }
    }

    // TODO: Currently used in testing only, not part of the interface, need to add queryable methods to the interface instead
    internal IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records)
    {
        if (_isAvailable.Value == false)
        {
            records = 0;
            return [];
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _auditEntryRepository.GetPage(pageIndex, pageCount, out records);
        }
    }

    private Task<PagedModel<IAuditItem>> GetItemsAsyncInner(
        IQuery<IAuditItem> query,
        long pageNumber,
        int pageSize,
        Direction orderDirection = Direction.Descending,
        AuditType[]? auditTypeFilter = null,
        IQuery<IAuditItem>? customFilter = null)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IEnumerable<IAuditItem> auditItems = _auditRepository.GetPagedResultsByQuery(query, pageNumber, pageSize, out var totalRecords, orderDirection, auditTypeFilter, customFilter);
            return Task.FromResult(new PagedModel<IAuditItem> { Items = auditItems, Total = totalRecords });
        }
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
