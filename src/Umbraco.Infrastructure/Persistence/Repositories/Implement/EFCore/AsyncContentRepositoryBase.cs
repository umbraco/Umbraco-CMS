using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
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

    // --- IAsyncContentRepository ---

    /// <inheritdoc />
    public abstract Guid RecycleBinKey { get; }

    /// <inheritdoc />
    public abstract Task<IEnumerable<TEntity>> GetAllVersionsAsync(Guid nodeKey, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetAllVersionsSlimAsync(Guid nodeKey, int skip, int take, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public virtual Task<IEnumerable<int>> GetVersionIdsAsync(Guid nodeKey, int topRows, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public abstract Task<TEntity?> GetVersionAsync(int versionId, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual Task DeleteVersionAsync(int versionId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public virtual Task DeleteVersionsAsync(Guid nodeKey, DateTime versionDate, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public virtual Task<int> CountAsync(string? contentTypeAlias, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public virtual Task<int> CountChildrenAsync(Guid parentKey, string? contentTypeAlias, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public virtual Task<int> CountDescendantsAsync(Guid parentKey, string? contentTypeAlias, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetChildrenAsync(Guid parentKey, long pageIndex, int pageSize, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetDescendantsAsync(Guid ancestorKey, long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<IEnumerable<TEntity>> GetRecycleBinAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetPagedRecycleBinAsync(long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    // --- Protected helpers ---

    /// <summary>
    ///     Gets the cache key used to cache the recycle bin contents for this entity type.
    /// </summary>
    protected abstract string RecycleBinCacheKey { get; }

    /// <summary>
    ///     Performs the low-level deletion of a specific version row from the data store.
    /// </summary>
    /// <param name="nodeId">The integer node identifier owning the version.</param>
    /// <param name="versionId">The integer identifier of the version to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected abstract Task PerformDeleteVersionAsync(int nodeId, int versionId, CancellationToken cancellationToken);

    /// <summary>
    ///     Called after a scope is refreshed for an entity, allowing the repository to publish
    ///     the appropriate unit-of-work notification (e.g. a cache-refresher notification).
    /// </summary>
    /// <param name="entity">The entity that was refreshed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected abstract Task OnUowRefreshedEntityAsync(TEntity entity, CancellationToken cancellationToken);
}
