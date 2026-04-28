using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class AuditRepository : AsyncEntityRepositoryBase<int, IAuditItem>, IAuditRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current EF Core database scope.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="repositoryCacheVersionService">Service responsible for managing cache versioning for repository data.</param>
    /// <param name="cacheSyncService">Service used to synchronize cache state across distributed environments.</param>
    public AuditRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        ILogger<AuditRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            AppCaches.NoCache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    /// <inheritdoc />
    public async Task CleanLogsAsync(int maximumAgeOfLogsInMinutes)
    {
        DateTime oldestPermittedLogEntry = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(maximumAgeOfLogsInMinutes));
        var headers = new[]
        {
            AuditType.Open.ToString(),
            AuditType.System.ToString(),
        };

        await AmbientScope.ExecuteWithContextAsync(async db =>
            await db.Logs
                .Where(x => x.Datestamp < oldestPermittedLogEntry && headers.Contains(x.Header))
                .ExecuteDeleteAsync());
    }

    /// <inheritdoc />
    public Task<PagedModel<IAuditItem>> GetPagedAsync(
        int skip,
        int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null,
        CancellationToken cancellationToken = default)
        => GetPagedInternalAsync(
            q => q,
            skip,
            take,
            orderDirection,
            sinceDate,
            auditTypeFilter,
            cancellationToken);

    /// <inheritdoc />
    public Task<PagedModel<IAuditItem>> GetPagedForEntityAsync(
        int entityId,
        int skip,
        int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null,
        CancellationToken cancellationToken = default)
        => GetPagedInternalAsync(
            q => q.Where(x => x.NodeId == entityId),
            skip,
            take,
            orderDirection,
            sinceDate,
            auditTypeFilter,
            cancellationToken);

    /// <inheritdoc />
    public Task<PagedModel<IAuditItem>> GetPagedForUserAsync(
        int userId,
        int skip,
        int take,
        Direction orderDirection,
        DateTime? sinceDate = null,
        AuditType[]? auditTypeFilter = null,
        CancellationToken cancellationToken = default)
        => GetPagedInternalAsync(
            q => q.Where(x => x.UserId == userId),
            skip,
            take,
            orderDirection,
            sinceDate,
            auditTypeFilter,
            cancellationToken);

    /// <inheritdoc />
    protected override async Task<IAuditItem?> PerformGetAsync(int key) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            LogDto? dto = await db.Logs.FirstOrDefaultAsync(x => x.Id == key);
            return dto is null ? null : AuditItemFactory.BuildEntity(dto);
        });

    /// <inheritdoc />
    /// <remarks>
    ///     This override exists to satisfy the <see cref="IAsyncReadRepository{TKey,TEntity}"/>.
    /// </remarks>
    protected override async Task<IEnumerable<IAuditItem>?> PerformGetAllAsync()
        => throw new NotSupportedException(
            "The audit log can be extremely huge. Use the GetPagedAsync method instead.");

    /// <inheritdoc />
    protected override async Task<IEnumerable<IAuditItem>?> PerformGetManyAsync(int[]? keys)
    {
        if (keys is null || keys.Length == 0)
        {
            return [];
        }

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<LogDto> dtos = await db.Logs.Where(x => keys.Contains(x.Id)).ToListAsync();
            return AuditItemFactory.BuildEntities(dtos);
        });
    }

    /// <inheritdoc />
    protected override Task<bool> PerformExistsAsync(int key)
        => AmbientScope.ExecuteWithContextAsync(async db =>
            await db.Logs.AnyAsync(x => x.Id == key));

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IAuditItem item)
    {
        LogDto dto = AuditItemFactory.BuildDto(item);
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            await db.Logs.AddAsync(dto);
            return await db.SaveChangesAsync();
        });
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Audit log entries are append-only. The base <see cref="AsyncEntityRepositoryBase{TKey,TEntity}.SaveAsync"/>
    ///     routes here whenever <see cref="IAuditItem.HasIdentity"/> is <see langword="true"/>, which is always the
    ///     case because <see cref="AuditItem.Id"/> stores the audited entity's NodeId (never zero). To preserve the
    ///     legacy "every save creates a new row" contract we delegate to <see cref="PersistNewItemAsync"/>.
    /// </remarks>
    protected override Task PersistUpdatedItemAsync(IAuditItem item) => PersistNewItemAsync(item);

    /// <inheritdoc />
    /// <remarks>
    ///     Single-row deletion is intentionally unsupported — the model never round-trips the log row primary key,
    ///     so an <see cref="IAuditItem"/> instance cannot uniquely identify a row to delete.
    ///     Use <see cref="CleanLogsAsync"/> to bulk-delete by date.
    /// </remarks>
    protected override Task PersistDeletedItemAsync(IAuditItem entity)
        => throw new NotSupportedException(
            "Audit log entries cannot be deleted individually. Use CleanLogsAsync to bulk-delete by date.");

    private Task<PagedModel<IAuditItem>> GetPagedInternalAsync(
        Func<IQueryable<LogDto>, IQueryable<LogDto>> baseFilter,
        int skip,
        int take,
        Direction orderDirection,
        DateTime? sinceDate,
        AuditType[]? auditTypeFilter,
        CancellationToken cancellationToken)
        => AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<LogDto> query = baseFilter(db.Logs);

            if (sinceDate.HasValue)
            {
                DateTime since = sinceDate.Value;
                query = query.Where(x => x.Datestamp >= since);
            }

            if (auditTypeFilter is { Length: > 0 })
            {
                var headers = auditTypeFilter.Select(t => t.ToString()).ToArray();
                query = query.Where(x => headers.Contains(x.Header));
            }

            int total = await query.CountAsync(cancellationToken);

            query = orderDirection == Direction.Ascending
                ? query.OrderBy(x => x.Datestamp)
                : query.OrderByDescending(x => x.Datestamp);

            List<LogDto> page = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

            return new PagedModel<IAuditItem>
            {
                Items = AuditItemFactory.BuildEntities(page),
                Total = total,
            };
        });
}
