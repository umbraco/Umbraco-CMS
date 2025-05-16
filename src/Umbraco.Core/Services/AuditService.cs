using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement;

public sealed class AuditService : RepositoryService, IAuditService
{
    private readonly IUserService _userService;
    private readonly IAuditRepository _auditRepository;
    private readonly IEntityService _entityService;

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

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public AuditService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IAuditRepository auditRepository,
        IAuditEntryRepository auditEntryRepository,
        IUserService userService,
        IEntityService entityService)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            auditRepository,
            userService,
            entityService)
    {
    }

    public void Add(AuditType type, int userId, int objectId, string? entityType, string comment, string? parameters = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, entityType, comment, parameters));
            scope.Complete();
        }
    }

    public IEnumerable<IAuditItem> GetLogs(int objectId)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            IEnumerable<IAuditItem> result = _auditRepository.Get(Query<IAuditItem>().Where(x => x.Id == objectId));
            scope.Complete();
            return result;
        }
    }

    public IEnumerable<IAuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null)
    {
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

    public void CleanLogs(int maximumAgeOfLogsInMinutes)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _auditRepository.CleanLogs(maximumAgeOfLogsInMinutes);
            scope.Complete();
        }
    }

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

        if (entityId == Constants.System.Root || entityId <= 0)
        {
            totalRecords = 0;
            return Enumerable.Empty<IAuditItem>();
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.Id == entityId);

            return _auditRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderDirection, auditTypeFilter, customFilter);
        }
    }

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

        if (userId < Constants.Security.SuperUserId)
        {
            totalRecords = 0;
            return Enumerable.Empty<IAuditItem>();
        }

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.UserId == userId);

            return _auditRepository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderDirection, auditTypeFilter, customFilter);
        }
    }

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
                return Task.FromResult(new PagedModel<IAuditItem> { Items = Enumerable.Empty<IAuditItem>(), Total = 0 });
            }

            using (ScopeProvider.CreateCoreScope(autoComplete: true))
            {
                IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.Id == keyToIdAttempt.Result);
                IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate.Value.LocalDateTime) : null;
                PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

                IEnumerable<IAuditItem> auditItems = _auditRepository.GetPagedResultsByQuery(query, pageNumber, pageSize, out var totalRecords, orderDirection, auditTypeFilter, customFilter);
                return Task.FromResult(new PagedModel<IAuditItem> { Items = auditItems, Total = totalRecords });
            }
        }

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

        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IAuditItem> query = Query<IAuditItem>().Where(x => x.UserId == user.Id);
            IQuery<IAuditItem>? customFilter = sinceDate.HasValue ? Query<IAuditItem>().Where(x => x.CreateDate >= sinceDate) : null;
            PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

            IEnumerable<IAuditItem> auditItems = _auditRepository.GetPagedResultsByQuery(query, pageNumber, pageSize, out var totalRecords, orderDirection, auditTypeFilter, customFilter);
            return new PagedModel<IAuditItem> { Items = auditItems, Total = totalRecords };
        }
    }

    /// <inheritdoc />
    [Obsolete("Use AuditEntryService.WriteAsync() instead. Scheduled for removal in Umbraco 18.")]
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
            eventDetails).GetAwaiter().GetResult().Result;
    }
}
