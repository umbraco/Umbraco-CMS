using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     Provides a base class for async repositories managing <see cref="IContentBase" /> entities,
///     covering versioning, counting, recycle bin, and data integrity operations.
/// </summary>
/// <remarks>
///     Mirrors the role of the NPoco <c>ContentRepositoryBase</c>, sitting between
///     <see cref="AsyncEntityRepositoryBase{TKey,TEntity}" /> and the publishable content layer.
///     Extend this class for media and other <see cref="IContentBase" /> types that do not need
///     publishing/scheduling support.
/// </remarks>
/// <typeparam name="TEntity">The content entity type.</typeparam>
/// <typeparam name="TRepository">The concrete repository type (self-referential, used for cache policy resolution).</typeparam>
internal abstract class AsyncContentRepositoryBase<TEntity, TRepository>
    : AsyncEntityRepositoryBase<Guid, TEntity>, IAsyncContentRepository<TEntity>
    where TEntity : class, IContentBase
    where TRepository : class, IRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncContentRepositoryBase{TEntity,TRepository}" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="loggerFactory">
    ///     The logger factory used to create a logger for the correct closed generic type at runtime.
    /// </param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="relationRepository">The relation repository.</param>
    /// <param name="relationTypeRepository">The relation type repository.</param>
    /// <param name="propertyEditors">The property editor collection.</param>
    /// <param name="dataValueReferenceFactories">The data value reference factory collection.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="eventAggregator">The event aggregator for unit-of-work notifications.</param>
    /// <param name="repositoryCacheVersionService">The repository cache version service.</param>
    /// <param name="cacheSyncService">The cache synchronization service.</param>
    protected AsyncContentRepositoryBase(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches appCaches,
        ILoggerFactory loggerFactory,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IEventAggregator eventAggregator,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            appCaches,
            loggerFactory.CreateLogger<AsyncEntityRepositoryBase<Guid, TEntity>>(),
            repositoryCacheVersionService,
            cacheSyncService)
    {
        LanguageRepository = languageRepository;
        RelationRepository = relationRepository;
        RelationTypeRepository = relationTypeRepository;
        PropertyEditors = propertyEditors;
        DataValueReferenceFactories = dataValueReferenceFactories;
        DataTypeService = dataTypeService;
        EventAggregator = eventAggregator;
    }

    /// <summary>
    ///     Gets the node object type Guid for this repository's entity kind.
    /// </summary>
    protected abstract Guid NodeObjectTypeKey { get; }

    /// <summary>
    ///     Gets the self-referential <typeparamref name="TRepository" /> reference.
    /// </summary>
    protected abstract TRepository This { get; }

    /// <summary>Gets the language repository.</summary>
    protected ILanguageRepository LanguageRepository { get; }

    /// <summary>Gets the relation repository.</summary>
    protected IRelationRepository RelationRepository { get; }

    /// <summary>Gets the relation type repository.</summary>
    protected IRelationTypeRepository RelationTypeRepository { get; }

    /// <summary>Gets the property editor collection.</summary>
    protected PropertyEditorCollection PropertyEditors { get; }

    /// <summary>Gets the data value reference factory collection.</summary>
    protected DataValueReferenceFactoryCollection DataValueReferenceFactories { get; }

    /// <summary>Gets the data type service.</summary>
    protected IDataTypeService DataTypeService { get; }

    /// <summary>Gets the event aggregator for publishing unit-of-work notifications.</summary>
    protected IEventAggregator EventAggregator { get; }

    /// <inheritdoc />
    public abstract Guid RecycleBinKey { get; }

    /// <inheritdoc />
    public abstract Task<IEnumerable<TEntity>> GetAllVersionsAsync(Guid nodeKey, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllVersionsSlimAsync(Guid nodeKey, int skip, int take, CancellationToken cancellationToken)
    {
        IEnumerable<TEntity> allVersions = await GetAllVersionsAsync(nodeKey, cancellationToken);
        return allVersions.Skip(skip).Take(take);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<Guid>> GetVersionKeysAsync(Guid nodeKey, int maxRows, CancellationToken cancellationToken) =>
        await AmbientScope.ExecuteWithContextAsync<IEnumerable<Guid>>(async db =>
        {
            return await db.ContentVersions
                .Join(db.Nodes, version => version.NodeId, node => node.NodeId, (version, node) => new { version, node })
                .Where(x => x.node.UniqueId == nodeKey)
                .OrderByDescending(x => x.version.Current)
                .ThenByDescending(x => x.version.VersionDate)
                .Take(maxRows)
                .Select(x => x.version.Key)
                .ToListAsync(cancellationToken);
        });

    /// <inheritdoc />
    public abstract Task<TEntity?> GetVersionAsync(Guid versionKey, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual async Task DeleteVersionAsync(Guid versionKey, CancellationToken cancellationToken)
    {
        var versionId = await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            return await db.ContentVersions
                .Where(x => x.Key == versionKey)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        });

        if (versionId == 0)
        {
            return;
        }

        await PerformDeleteVersionAsync(versionId, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteVersionsAsync(Guid nodeKey, DateTime versionDate, CancellationToken cancellationToken)
    {
        IEnumerable<int> versionIds = await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            return await db.ContentVersions
                .Join(db.Nodes, version => version.NodeId, node => node.NodeId, (version, node) => new { version, node })
                .Where(x => x.node.UniqueId == nodeKey && !x.version.Current && x.version.VersionDate < versionDate)
                .Select(x => x.version.Id)
                .ToListAsync(cancellationToken);
        });

        foreach (int versionId in versionIds)
        {
            await PerformDeleteVersionAsync(versionId, cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual Task<int> CountAsync(CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(db =>
            db.Nodes
                .Where(n => n.NodeObjectType == NodeObjectTypeKey)
                .CountAsync(cancellationToken));

    // TODO: replace raw SQL with LINQ join on ContentTypeDto DbSet once ContentTypeDto is migrated to EF Core
    /// <inheritdoc />
    public virtual Task<int> CountAsync(string contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(db =>
            db.Database
                .SqlQueryRaw<int>(
                    $"SELECT COUNT(*) " +
                    $"FROM {NodeDto.TableName} n " +
                    $"INNER JOIN {ContentDto.TableName} c ON n.{NodeDto.IdColumnName} = c.{ContentDto.PrimaryKeyColumnName} " +
                    $"INNER JOIN {Constants.DatabaseSchema.Tables.ContentType} ct ON c.{ContentDto.ContentTypeIdColumnName} = ct.{Constants.DatabaseSchema.Columns.NodeIdName} " +
                    $"WHERE n.{NodeDto.NodeObjectTypeColumnName} = @p0 AND ct.alias = @p1",
                    NodeObjectTypeKey,
                    contentTypeAlias)
                .SingleAsync(cancellationToken));

    /// <inheritdoc />
    public virtual Task<int> CountChildrenAsync(Guid parentKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await db.Nodes
                .Where(n => n.UniqueId == parentKey)
                .Select(n => n.NodeId)
                .SingleOrDefaultAsync(cancellationToken);

            return await db.Nodes
                .Where(n => n.NodeObjectType == NodeObjectTypeKey && n.ParentId == parentNodeId)
                .CountAsync(cancellationToken);
        });

    // TODO: replace raw SQL with LINQ join on ContentTypeDto DbSet once ContentTypeDto is migrated to EF Core
    /// <inheritdoc />
    public virtual Task<int> CountChildrenAsync(Guid parentKey, string contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await db.Nodes
                .Where(n => n.UniqueId == parentKey)
                .Select(n => n.NodeId)
                .SingleOrDefaultAsync(cancellationToken);

            return await db.Database
                .SqlQueryRaw<int>(
                    $"SELECT COUNT(*) " +
                    $"FROM {NodeDto.TableName} n " +
                    $"INNER JOIN {ContentDto.TableName} c ON n.{NodeDto.IdColumnName} = c.{ContentDto.PrimaryKeyColumnName} " +
                    $"INNER JOIN {Constants.DatabaseSchema.Tables.ContentType} ct ON c.{ContentDto.ContentTypeIdColumnName} = ct.{Constants.DatabaseSchema.Columns.NodeIdName} " +
                    $"WHERE n.{NodeDto.NodeObjectTypeColumnName} = @p0 AND n.{NodeDto.ParentIdColumnName} = @p1 AND ct.alias = @p2",
                    NodeObjectTypeKey,
                    parentNodeId,
                    contentTypeAlias)
                .SingleAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<int> CountDescendantsAsync(Guid parentKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await db.Nodes
                .Where(n => n.UniqueId == parentKey)
                .Select(n => n.NodeId)
                .SingleOrDefaultAsync(cancellationToken);

            string pathMatch = parentNodeId == -1 ? "-1," : $",{parentNodeId},";

            return await db.Nodes
                .Where(n => n.NodeObjectType == NodeObjectTypeKey && n.Path.Contains(pathMatch))
                .CountAsync(cancellationToken);
        });

    // TODO: replace raw SQL with LINQ join on ContentTypeDto DbSet once ContentTypeDto is migrated to EF Core
    /// <inheritdoc />
    public virtual Task<int> CountDescendantsAsync(Guid parentKey, string contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await db.Nodes
                .Where(n => n.UniqueId == parentKey)
                .Select(n => n.NodeId)
                .SingleOrDefaultAsync(cancellationToken);

            string pathMatch = parentNodeId == -1 ? "-1," : $",{parentNodeId},";
            string pathLikePattern = $"%{pathMatch}%";

            return await db.Database
                .SqlQueryRaw<int>(
                    $"SELECT COUNT(*) " +
                    $"FROM {NodeDto.TableName} n " +
                    $"INNER JOIN {ContentDto.TableName} c ON n.{NodeDto.IdColumnName} = c.{ContentDto.PrimaryKeyColumnName} " +
                    $"INNER JOIN {Constants.DatabaseSchema.Tables.ContentType} ct ON c.{ContentDto.ContentTypeIdColumnName} = ct.{Constants.DatabaseSchema.Columns.NodeIdName} " +
                    $"WHERE n.{NodeDto.NodeObjectTypeColumnName} = @p0 AND n.{NodeDto.PathColumnName} LIKE @p1 AND ct.alias = @p2",
                    NodeObjectTypeKey,
                    pathLikePattern,
                    contentTypeAlias)
                .SingleAsync(cancellationToken);
        });

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetChildrenAsync(Guid parentKey, long pageIndex, int pageSize, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetDescendantsAsync(Guid ancestorKey, long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<IEnumerable<TEntity>> GetRecycleBinAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetPagedRecycleBinAsync(long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual async Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(
        ContentDataIntegrityReportOptions options,
        CancellationToken cancellationToken) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var report = new Dictionary<int, ContentDataIntegrityReportEntry>();
            var nodesToRebuild = new Dictionary<int, List<NodeDto>>();
            var validNodes = new Dictionary<int, NodeDto>();

            int[] rootIds = [Constants.System.Root, Constants.System.RecycleBinContent, Constants.System.RecycleBinMedia];
            var currentParentIds = new HashSet<int>(rootIds);
            HashSet<int> prevParentIds = currentParentIds;
            var lastLevel = -1;

            List<NodeDto> nodes = await db.Nodes
                .Where(n => n.NodeObjectType == NodeObjectTypeKey)
                .OrderBy(n => n.Level)
                .ThenBy(n => n.ParentId)
                .ThenBy(n => n.SortOrder)
                .ToListAsync(cancellationToken);

            foreach (NodeDto node in nodes)
            {
                if (node.Level != lastLevel)
                {
                    prevParentIds = currentParentIds;
                    currentParentIds = new HashSet<int>();
                    lastLevel = node.Level;
                }

                currentParentIds.Add(node.NodeId);

                string[] pathParts = node.Path
                    .Split(Constants.CharArrays.Comma)
                    .Where(x => !rootIds.Contains(int.Parse(x, CultureInfo.InvariantCulture)))
                    .ToArray();

                ContentDataIntegrityReport.IssueType? issue = null;
                if (!prevParentIds.Contains(node.ParentId))
                {
                    issue = ContentDataIntegrityReport.IssueType.InvalidPathAndLevelByParentId;
                }
                else if (pathParts.Length == 0)
                {
                    issue = ContentDataIntegrityReport.IssueType.InvalidPathEmpty;
                }
                else if (pathParts.Length != node.Level)
                {
                    issue = ContentDataIntegrityReport.IssueType.InvalidPathLevelMismatch;
                }
                else if (pathParts[^1] != node.NodeId.ToString())
                {
                    issue = ContentDataIntegrityReport.IssueType.InvalidPathById;
                }
                else if (!rootIds.Contains(node.ParentId) && pathParts[^2] != node.ParentId.ToString())
                {
                    issue = ContentDataIntegrityReport.IssueType.InvalidPathByParentId;
                }

                if (issue.HasValue)
                {
                    report.Add(node.NodeId, new ContentDataIntegrityReportEntry(issue.Value));
                    AppendNodeToRebuild(nodesToRebuild, node);
                }
                else if (options.FixIssues)
                {
                    validNodes.Add(node.NodeId, node);
                }
            }

            if (options.FixIssues)
            {
                var updated = new List<NodeDto>();

                foreach (var (nodeId, validNode) in validNodes)
                {
                    if (!nodesToRebuild.TryGetValue(nodeId, out List<NodeDto>? invalidNodes))
                    {
                        continue;
                    }

                    foreach (NodeDto invalidNode in invalidNodes)
                    {
                        invalidNode.Level = (short)(validNode.Level + 1);
                        invalidNode.Path = validNode.Path + "," + invalidNode.NodeId;
                        updated.Add(invalidNode);
                    }
                }

                foreach (NodeDto node in updated)
                {
                    await db.Nodes
                        .Where(n => n.NodeId == node.NodeId)
                        .ExecuteUpdateAsync(
                            s => s
                                .SetProperty(n => n.Level, node.Level)
                                .SetProperty(n => n.Path, node.Path),
                            cancellationToken);

                    if (report.TryGetValue(node.NodeId, out ContentDataIntegrityReportEntry? entry))
                    {
                        entry.Fixed = true;
                    }
                }
            }

            return new ContentDataIntegrityReport(report);
        });

    private static void AppendNodeToRebuild(Dictionary<int, List<NodeDto>> nodesToRebuild, NodeDto node)
    {
        if (nodesToRebuild.TryGetValue(node.ParentId, out List<NodeDto>? children))
        {
            children.Add(node);
        }
        else
        {
            nodesToRebuild[node.ParentId] = [node];
        }
    }

    /// <summary>
    ///     Gets the cache key used to cache the recycle bin contents for this entity type.
    /// </summary>
    protected abstract string RecycleBinCacheKey { get; }

    /// <summary>
    ///     Performs the low-level deletion of a specific version row from the data store.
    /// </summary>
    /// <param name="versionId">The integer primary key of the content version to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected abstract Task PerformDeleteVersionAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>
    ///     Called after a scope is refreshed for an entity, allowing the repository to publish
    ///     the appropriate unit-of-work notification (e.g. a cache-refresher notification).
    /// </summary>
    /// <param name="entity">The entity that was refreshed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected abstract Task OnUowRefreshedEntityAsync(TEntity entity, CancellationToken cancellationToken);
}
