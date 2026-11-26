using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     Provides a base class for async repositories managing <see cref="IContentBase" /> entities,
///     covering reading and entity assembly, versioning, counting, recycle bin, and data integrity operations.
/// </summary>
/// <remarks>
///     Mirrors the role of the NPoco <c>ContentRepositoryBase</c>, sitting between
///     <see cref="AsyncEntityRepositoryBase{TKey,TEntity}" /> and the publishable content layer.
///     Extend this class for media and other <see cref="IContentBase" /> types that do not need
///     publishing/scheduling support.
/// </remarks>
/// <typeparam name="TEntity">The content entity type.</typeparam>
/// <typeparam name="TRepository">The concrete repository type (self-referential, used for cache policy resolution).</typeparam>
/// <typeparam name="TContentType">The content type the entities are built against.</typeparam>
/// <typeparam name="TVersionDto">The EF Core DTO type for the entity-specific version rows.</typeparam>
/// <typeparam name="TJoinRow">The join shape the read queries are built on.</typeparam>
/// <typeparam name="TRow">The row shape entity assembly consumes.</typeparam>
internal abstract class AsyncContentRepositoryBase<TEntity, TRepository, TContentType, TVersionDto, TJoinRow, TRow>
    : AsyncEntityRepositoryBase<Guid, TEntity>, IAsyncContentRepository<TEntity>
    where TEntity : class, IContentBase
    where TRepository : class, IRepository
    where TContentType : class, IContentTypeComposition
    where TVersionDto : class, IEntityVersionDto
    where TJoinRow : ContentJoinRow<TVersionDto>
    where TRow : ContentRow<TVersionDto>
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly ITagRepository _tagRepository;
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="AsyncContentRepositoryBase{TEntity,TRepository,TContentType,TVersionDto,TJoinRow,TRow}" /> class.
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
    /// <param name="contentTypeRepository">The content type repository used to resolve content types by ID.</param>
    /// <param name="idKeyMap">The id/key map, used to resolve data types for tag and sortable-value handling.</param>
    /// <param name="tagRepository">The tag repository, used to persist tag values for tag-enabled properties on save.</param>
    /// <param name="jsonSerializer">The JSON serializer, used to parse legacy JSON-stored tag values.</param>
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
        ICacheSyncService cacheSyncService,
        IAsyncContentTypeRepositoryBase<TContentType> contentTypeRepository,
        IIdKeyMap idKeyMap,
        ITagRepository tagRepository,
        IJsonSerializer jsonSerializer)
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
        ContentTypeRepository = contentTypeRepository;
        _idKeyMap = idKeyMap;
        _tagRepository = tagRepository;
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    ///     Gets the node object type Guid for this repository's entity kind.
    /// </summary>
    protected abstract Guid NodeObjectTypeKey { get; }

    /// <summary>
    ///     Gets the self-referential <typeparamref name="TRepository" /> reference.
    /// </summary>
    protected abstract TRepository This { get; }

    /// <summary>
    ///     Builds a domain entity from the rows a read query returned for one version of it and its content type.
    /// </summary>
    /// <param name="row">The materialized row: the node, content and version rows the entity kind reads.</param>
    /// <param name="contentType">
    ///     The content type for the entity. The read fails before this is called when a row's content type cannot
    ///     be resolved.
    /// </param>
    /// <returns>The constructed domain entity.</returns>
    protected abstract TEntity BuildEntity(TRow row, TContentType contentType);

    /// <summary>
    ///     Builds the join every read query of this repository starts from, over nodes and content versions the
    ///     caller has already filtered: the content and entity-specific version rows joined on the node and
    ///     version ids, and the LEFT JOINed content type and parent node, plus whatever the entity kind adds.
    /// </summary>
    /// <remarks>
    ///     Implementations keep the joins concrete to their own tables; everything downstream of the row is
    ///     shared. Project with object-initializer syntax only, as
    ///     <see cref="ContentJoinRow{TVersionDto}" /> explains.
    /// </remarks>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="nodes">The nodes to read, already filtered by the caller.</param>
    /// <param name="contentVersions">The content versions to join each node with, already filtered by the caller.</param>
    /// <returns>One row per node and version pair.</returns>
    protected abstract IQueryable<TJoinRow> BuildBaseQuery(
        UmbracoDbContext db,
        IQueryable<NodeDto> nodes,
        IQueryable<ContentVersionDto> contentVersions);

    /// <summary>
    ///     Builds the join every read query of this repository starts from, joining each node with its current version.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="nodes">The nodes to read, already filtered by the caller.</param>
    /// <returns>One row per node.</returns>
    protected IQueryable<TJoinRow> BuildBaseQuery(UmbracoDbContext db, IQueryable<NodeDto> nodes) =>
        BuildBaseQuery(db, nodes, db.ContentVersions.Where(contentVersion => contentVersion.Current));

    /// <summary>
    ///     Gets the single shared projection from the join shape into the row entity assembly consumes; every read
    ///     query ends with <c>.Select(ToContentRow)</c> instead of restating this construction.
    /// </summary>
    protected abstract Expression<Func<TJoinRow, TRow>> ToContentRow { get; }

    /// <summary>
    ///     Gets the id of the published version a row carries, or 0 when the row has none or the entity kind has no
    ///     notion of a published version.
    /// </summary>
    /// <param name="row">The materialized row.</param>
    /// <returns>The published version id, or 0.</returns>
    protected virtual int GetPublishedVersionId(TRow row) => 0;

    /// <summary>
    ///     Loads whatever variation data the entity kind keeps for a batch of rows, before any entity is built, and
    ///     returns the step that applies it to each built entity. The default applies nothing.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="rows">The rows about to be assembled.</param>
    /// <param name="versionIds">Every version id the rows carry, current and published, deduplicated.</param>
    /// <param name="contentTypeMap">The content types of the rows, keyed by id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The step applied to each entity once it is built, given the entity and its row.</returns>
    protected virtual Task<Action<TEntity, TRow>> PrepareVariationsAsync(
        UmbracoDbContext db,
        IReadOnlyList<TRow> rows,
        IReadOnlyList<int> versionIds,
        IReadOnlyDictionary<int, TContentType> contentTypeMap,
        CancellationToken cancellationToken) =>
        Task.FromResult<Action<TEntity, TRow>>((entity, row) => { });

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

    /// <summary>Gets the content type repository used to resolve content types by their integer ID.</summary>
    protected IAsyncContentTypeRepositoryBase<TContentType> ContentTypeRepository { get; }

    /// <inheritdoc />
    // The base AsyncDefaultRepositoryCachePolicy defaults to the int-style "uRepo_" cache key prefix even
    // though TKey is Guid here. The NPoco Guid-keyed cache policy and ContentCacheRefresher already clear
    // the "uRepoGuid_" prefix on every save/refresh, so this override reuses that exact prefix instead of
    // sitting in an entirely uninvalidated key.
    protected override IAsyncRepositoryCachePolicy<TEntity, Guid> CreateCachePolicy()
        => new AsyncGuidReadRepositoryCachePolicy<TEntity>(
            GlobalIsolatedCache,
            ScopeAccessor,
            DefaultOptions,
            RepositoryCacheVersionService,
            CacheSyncService,
            EntityTypeCacheKeyPrefix);

    /// <summary>
    ///     Gets the cache key prefix this repository files its entities under.
    /// </summary>
    /// <remarks>
    ///     Two repositories over the same entity type would otherwise share one prefix, and each could serve
    ///     entries the other cached. ContentCacheRefresher clears a refreshed item by its exact key under the
    ///     default prefix, so entries filed under a longer prefix are not reached by it; only a branch refresh
    ///     or removal, which matches on the cached entities' paths, clears those. A repository with its own
    ///     prefix therefore clears its own entries as it persists.
    /// </remarks>
    protected virtual string EntityTypeCacheKeyPrefix => RepositoryCacheKeys.GetGuidKey<TEntity>();

    /// <inheritdoc />
    public abstract Guid RecycleBinKey { get; }

    /// <inheritdoc />
    public virtual async Task UpdateSortOrderAsync(IReadOnlyList<Guid> orderedNodeKeys, CancellationToken cancellationToken)
    {
        if (orderedNodeKeys.Count == 0)
        {
            return;
        }

        await AmbientScope.ExecuteWithContextAsync<object>(async db =>
        {
            var nodesByKey = new Dictionary<Guid, NodeDto>();

            foreach (IEnumerable<Guid> batch in orderedNodeKeys.Distinct().InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                List<Guid> batchKeys = [.. batch];

                List<NodeDto> batchNodes = await db.Nodes
                    .AsTracking()
                    .Where(node => batchKeys.Contains(node.UniqueId))
                    .ToListAsync(cancellationToken);

                foreach (NodeDto node in batchNodes)
                {
                    nodesByKey[node.UniqueId] = node;
                }
            }

            for (var sortOrder = 0; sortOrder < orderedNodeKeys.Count; sortOrder++)
            {
                if (nodesByKey.TryGetValue(orderedNodeKeys[sortOrder], out NodeDto? node))
                {
                    node.SortOrder = sortOrder;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        });
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<int>> GetVersionIdsAsync(Guid nodeKey, int skip, int take, CancellationToken cancellationToken) =>
        await AmbientScope.ExecuteWithContextAsync<IEnumerable<int>>(async db =>
        {
            return await db.ContentVersions
                .Join(db.Nodes, version => version.NodeId, node => node.NodeId, (version, node) => new { version, node })
                .Where(joined => joined.node.UniqueId == nodeKey)
                .OrderByDescending(joined => joined.version.Current)
                .ThenByDescending(joined => joined.version.VersionDate)
                // Versions saved within the same clock tick share a VersionDate, so without an id tiebreak
                // their relative order is whatever the provider happens to return - and paging over an
                // unstable order can repeat or skip a version.
                .ThenByDescending(joined => joined.version.Id)
                .Skip(skip)
                .Take(take)
                .Select(joined => joined.version.Id)
                .ToListAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task DeleteVersionAsync(int versionId, CancellationToken cancellationToken) =>
        PerformDeleteVersionAsync(versionId, cancellationToken);

    /// <inheritdoc />
    public virtual async Task DeleteVersionsAsync(Guid nodeKey, DateTime versionDate, CancellationToken cancellationToken)
    {
        IEnumerable<int> versionIds = await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            return await db.ContentVersions
                .Join(db.Nodes, version => version.NodeId, node => node.NodeId, (version, node) => new { version, node })
                .Where(joined => joined.node.UniqueId == nodeKey && !joined.version.Current && joined.version.VersionDate < versionDate)
                .Select(joined => joined.version.Id)
                .ToListAsync(cancellationToken);
        });

        foreach (int versionId in versionIds)
        {
            await PerformDeleteVersionAsync(versionId, cancellationToken);
        }
    }

    /// <summary>
    ///     Counts through the node table for the same reason <see cref="PerformExistsAsync" /> does. The base's
    ///     count runs only once something is already cached, which is why this went unnoticed for so long.
    /// </summary>
    protected override AsyncRepositoryCachePolicyOptions DefaultOptions =>
        field ??= new AsyncRepositoryCachePolicyOptions(() => CountAsync(CancellationToken.None));

    /// <summary>
    ///     Checks existence against the node table rather than the generic base's <c>DbSet&lt;TEntity&gt;()</c>, which
    ///     cannot work here: <typeparamref name="TEntity" /> is an interface and so is not part of the EF Core model.
    /// </summary>
    protected override Task<bool> PerformExistsAsync(Guid key) =>
        AmbientScope.ExecuteWithContextAsync(db =>
            db.Nodes.AnyAsync(node => node.UniqueId == key && node.NodeObjectType == NodeObjectTypeKey));

    /// <inheritdoc />
    public virtual Task<int> CountAsync(CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(db =>
            db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey)
                .CountAsync(cancellationToken));

    /// <inheritdoc />
    public virtual Task<int> CountAsync(string contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(db =>
            NodesFilteredByContentTypeAlias(db, contentTypeAlias)
                .CountAsync(cancellationToken));

    /// <inheritdoc />
    public virtual Task<int> CountChildrenAsync(Guid? parentKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await ResolveParentNodeIdAsync(db, parentKey, cancellationToken);

            return await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentNodeId)
                .CountAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<int> CountChildrenAsync(Guid? parentKey, string contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await ResolveParentNodeIdAsync(db, parentKey, cancellationToken);

            return await NodesFilteredByContentTypeAlias(db, contentTypeAlias)
                .Where(node => node.ParentId == parentNodeId)
                .CountAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<int> CountDescendantsAsync(Guid? parentKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await ResolveParentNodeIdAsync(db, parentKey, cancellationToken);

            string pathMatch = parentNodeId == -1 ? "-1," : $",{parentNodeId},";

            return await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && EF.Functions.Like(node.Path, $"%{pathMatch}%"))
                .CountAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<int> CountDescendantsAsync(Guid? parentKey, string contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await ResolveParentNodeIdAsync(db, parentKey, cancellationToken);

            string pathMatch = parentNodeId == -1 ? "-1," : $",{parentNodeId},";

            return await NodesFilteredByContentTypeAlias(db, contentTypeAlias)
                .Where(node => EF.Functions.Like(node.Path, $"%{pathMatch}%"))
                .CountAsync(cancellationToken);
        });

    // Joins Nodes → Content → ContentTypes filtered by NodeObjectTypeKey and content type alias,
    // projecting back to NodeDto so callers can compose further predicates before counting.
    private IQueryable<NodeDto> NodesFilteredByContentTypeAlias(UmbracoDbContext db, string contentTypeAlias) =>
        FilterByContentTypeAlias(db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey), db, contentTypeAlias);

    // Joins the given Nodes query onward through Content → ContentTypes filtered by content type
    // alias, projecting back to NodeDto. Protected (not folded into NodesFilteredByContentTypeAlias
    // above) so PublishableContentRepositoryBase.CountPublishedAsync can reuse this tail on top
    // of its own Published/Trashed-filtered query instead of duplicating the join inline.
    protected static IQueryable<NodeDto> FilterByContentTypeAlias(IQueryable<NodeDto> nodes, UmbracoDbContext db, string contentTypeAlias) =>
        nodes
            .Join(db.Content, node => node.NodeId, content => content.NodeId, (node, content) => new { node, content })
            .Join(db.ContentTypes, joined => joined.content.ContentTypeId, contentType => contentType.NodeId, (joined, contentType) => new { joined.node, contentType })
            .Where(joined => joined.contentType.Alias == contentTypeAlias)
            .Select(joined => joined.node);

    protected static Task<int> ResolveNodeIdAsync(UmbracoDbContext db, Guid key, CancellationToken cancellationToken) =>
        db.Nodes
            .Where(node => node.UniqueId == key)
            .Select(node => node.NodeId)
            .SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    ///     Resolves the node id of a parent, treating <c>null</c> as the root of the tree.
    /// </summary>
    /// <remarks>
    ///     Root has no Guid identity at all - <see cref="Constants.System.RootKey" /> is deliberately
    ///     <c>null</c> - so it cannot be resolved by key.
    /// </remarks>
    protected static async Task<int> ResolveParentNodeIdAsync(UmbracoDbContext db, Guid? parentKey, CancellationToken cancellationToken)
        => parentKey.HasValue
            ? await ResolveNodeIdAsync(db, parentKey.Value, cancellationToken)
            : Constants.System.Root;

    /// <summary>
    ///     Gets the selector for the published flag <see cref="ApplyOrdering" /> orders by when a caller asks for
    ///     "published", or <see langword="null" /> for an entity kind without one.
    /// </summary>
    protected virtual Expression<Func<TJoinRow, bool>>? PublishedOrderingSelector => null;

    /// <summary>
    ///     Applies paging/ordering to a query, breaking ties on node id for stable paging.
    /// </summary>
    /// <remarks>
    ///     Internal (not private) so the ordering unit tests can exercise the tiebreak logic
    ///     directly, decoupling it from a real database's incidental row order.
    /// </remarks>
    /// <param name="source">The rows to order.</param>
    /// <param name="users">The users, for ordering by owner or updater name.</param>
    /// <param name="ordering">The ordering to apply; <see langword="null" /> for sort order.</param>
    /// <param name="publishedSelector">
    ///     Selects the published flag to order by, or <see langword="null" /> for an entity kind without one.
    /// </param>
    internal static IOrderedQueryable<TJoinRow> ApplyOrdering(
        IQueryable<TJoinRow> source,
        IQueryable<UserDto> users,
        Ordering? ordering,
        Expression<Func<TJoinRow, bool>>? publishedSelector)
    {
        bool descending = ordering is { Direction: Direction.Descending };
        // No ordering at all means sort order - the default every caller relies on - which is distinct from
        // asking for a field this repository cannot order by.
        string orderBy = ordering?.OrderBy is { Length: > 0 } field ? field.ToLowerInvariant() : "sortorder";
        IOrderedQueryable<TJoinRow> ordered = orderBy switch
        {
            // Invariant name ordering (node.Text). Culture-specific name ordering is handled
            // by the callers via a ContentVersionCultureVariation JOIN before reaching this method.
            "name" => descending
                ? source.OrderByDescending(joined => joined.Node.Text)
                : source.OrderBy(joined => joined.Node.Text),
            "createdate" => descending
                ? source.OrderByDescending(joined => joined.Node.CreateDate)
                : source.OrderBy(joined => joined.Node.CreateDate),
            "versiondate" or "updatedate" => descending
                ? source.OrderByDescending(joined => joined.ContentVersion.VersionDate)
                : source.OrderBy(joined => joined.ContentVersion.VersionDate),
            "id" => descending
                ? source.OrderByDescending(joined => joined.Node.NodeId)
                : source.OrderBy(joined => joined.Node.NodeId),
            // Ordered by the user's name rather than their id, which is the order a caller listing
            // "owner" or "updater" is asking for. Owner is who created the node; updater is who created
            // its latest draft version. The name is a correlated subquery written out in each key selector:
            // the query provider only translates what it can see in the expression tree, and the lookup is
            // then only paid for when a caller orders by a user name.
            "owner" => descending
                ? source.OrderByDescending(joined => users.Where(user => user.Id == joined.Node.UserId).Select(user => user.UserName).FirstOrDefault())
                : source.OrderBy(joined => users.Where(user => user.Id == joined.Node.UserId).Select(user => user.UserName).FirstOrDefault()),
            "updater" => descending
                ? source.OrderByDescending(joined => users.Where(user => user.Id == joined.ContentVersion.UserId).Select(user => user.UserName).FirstOrDefault())
                : source.OrderBy(joined => users.Where(user => user.Id == joined.ContentVersion.UserId).Select(user => user.UserName).FirstOrDefault()),
            // Only entity kinds with a published flag supply a selector; for the others the field is as
            // unsupported as any unknown one.
            "published" when publishedSelector is not null => descending
                ? source.OrderByDescending(publishedSelector)
                : source.OrderBy(publishedSelector),
            // Null-propagating operators aren't valid inside an Expression<Func<...>> tree - use a
            // conditional instead of ContentType?.Alias.
            "contenttypealias" => descending
                ? source.OrderByDescending(joined => joined.ContentType == null ? null : joined.ContentType.Alias)
                : source.OrderBy(joined => joined.ContentType == null ? null : joined.ContentType.Alias),
            "path" => descending
                ? source.OrderByDescending(joined => joined.Node.Path)
                : source.OrderBy(joined => joined.Node.Path),
            "sortorder" => descending
                ? source.OrderByDescending(joined => joined.Node.SortOrder)
                : source.OrderBy(joined => joined.Node.SortOrder),
            // Custom-field ordering (ordering.IsCustomField) is intercepted by callers before reaching
            // this method — see ResolveCustomFieldOrderedNodeIdsAsync. Anything else is rejected rather
            // than quietly ordered by something the caller did not ask for.
            _ => throw new NotSupportedException($"Ordering by {ordering?.OrderBy} not supported."),
        };

        // Break ties on node id so paged results stay stable/non-duplicated across separate fetches —
        // mirrors NPoco's ContentRepositoryBase.PreparePageSql, which unconditionally appends "ORDER BY
        // umbracoNode.id" after any user ordering (see http://issues.umbraco.org/issue/U4-8831). Skipped
        // when already ordering by id, since that's already unique.
        return orderBy == "id" ? ordered : ordered.ThenBy(joined => joined.Node.NodeId);
    }

    // Re-sequences a set of already-fetched rows to match a previously computed node ID order. A node ID
    // present in orderedNodeIds is silently skipped if it has no corresponding row — the ordering-key
    // resolution and the row fetch are two separate round trips (unlike the other ordering paths, which
    // select/page within a single query), so a node concurrently deleted/moved between them would
    // otherwise fail the whole page with a KeyNotFoundException instead of just omitting that one node.
    private static List<TRow> ReorderRowsByNodeIds(List<TRow> rows, List<int> orderedNodeIds)
    {
        Dictionary<int, TRow> rowsByNodeId = rows.ToDictionary(row => row.Node.NodeId);
        return [.. orderedNodeIds
            .Where(rowsByNodeId.ContainsKey)
            .Select(nodeId => rowsByNodeId[nodeId])];
    }

    /// <summary>
    ///     Fetches one page of rows ordered by a custom property field: resolves the full ordered candidate id
    ///     list, pages it, then fetches and re-sequences the page's rows.
    /// </summary>
    /// <remarks>
    ///     Every branching read query shares this path for custom-field ordering; callers differ only in how
    ///     <paramref name="candidateNodeIds" /> is filtered and how <paramref name="fetchRowsForPageNodeIds" />
    ///     queries the base join.
    /// </remarks>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="candidateNodeIds">The node ids to page over, already filtered by the caller.</param>
    /// <param name="ordering">The custom-field ordering to apply.</param>
    /// <param name="skip">The number of ordered nodes to skip.</param>
    /// <param name="take">The number of ordered nodes to take.</param>
    /// <param name="fetchRowsForPageNodeIds">Fetches the rows for a batch of the page's node ids, in any order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page's rows in the requested order.</returns>
    protected static async Task<IReadOnlyList<TRow>> FetchCustomFieldOrderedPageAsync(
        UmbracoDbContext db,
        List<int> candidateNodeIds,
        Ordering ordering,
        int skip,
        int take,
        Func<List<int>, Task<List<TRow>>> fetchRowsForPageNodeIds,
        CancellationToken cancellationToken)
    {
        List<int> orderedNodeIds = await ResolveCustomFieldOrderedNodeIdsAsync(
            db, candidateNodeIds, ordering.OrderBy!, ordering.Culture ?? string.Empty, ordering.Direction, cancellationToken);

        List<int> pageNodeIds = [.. orderedNodeIds.Skip(skip).Take(take)];
        if (pageNodeIds.Count == 0)
        {
            return [];
        }

        // A page is only as small as the caller asked for, and callers pass int.MaxValue to mean "all of
        // them", so the node ids going into the fetch predicate are batched like any other runtime-sized
        // set. The ordering is restored once, over the whole page.
        var unorderedRows = new List<TRow>();
        foreach (IEnumerable<int> batch in pageNodeIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            unorderedRows.AddRange(await fetchRowsForPageNodeIds([.. batch]));
        }

        return ReorderRowsByNodeIds(unorderedRows, pageNodeIds);
    }

    /// <summary>
    ///     Fetches one page of rows ordered by name in a specific culture, falling back to the invariant name for
    ///     nodes without a variant name in that culture.
    /// </summary>
    /// <remarks>
    ///     Every branching read query shares this path for culture-specific name ordering. The culture variations
    ///     are LEFT JOINed filtered to the resolved language, so variantName = COALESCE(cultureVariation.Name, node.Text).
    ///     The whole
    ///     <typeparamref name="TJoinRow" /> is carried forward as one nested member
    ///     through the GroupJoin/SelectMany so it can be unwrapped again for ordering and fed straight into the
    ///     shared <see cref="ToContentRow" /> projection.
    /// </remarks>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="baseQuery">The base join to order and page, already filtered by the caller.</param>
    /// <param name="ordering">The name ordering to apply; its culture selects the variant names.</param>
    /// <param name="skip">The number of ordered rows to skip.</param>
    /// <param name="take">The number of ordered rows to take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page's rows in the requested order.</returns>
    protected async Task<IReadOnlyList<TRow>> FetchCultureNameOrderedAsync(
        UmbracoDbContext db,
        IQueryable<TJoinRow> baseQuery,
        Ordering ordering,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // Pre-fetch the language ID — the Language table is tiny (bounded by configured languages).
        // An unknown culture yields languageId = 0, which matches no culture-variation rows, so variantName falls
        // back to node.Text for every row (graceful degradation).
        int languageId = await db.Language
            .Where(language => language.IsoCode == ordering.Culture)
            .Select(language => language.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var withVariantName = baseQuery
            .GroupJoin(
                db.ContentVersionCultureVariations.Where(cultureVariation => cultureVariation.LanguageId == languageId),
                joined => joined.ContentVersion.Id,
                cultureVariation => cultureVariation.VersionId,
                (joined, cultureVariationGroup) => new { joined, cultureVariationGroup })
            .SelectMany(
                joined => joined.cultureVariationGroup.DefaultIfEmpty(),
                (joined, cultureVariation) => new
                {
                    joined.joined,
                    variantName = cultureVariation != null ? cultureVariation.Name ?? joined.joined.Node.Text : joined.joined.Node.Text,
                });

        bool descending = ordering is { Direction: Direction.Descending };

        var ordered = ApplyVariantNameOrdering(
            withVariantName,
            joined => joined.variantName,
            joined => joined.joined.Node.NodeId,
            descending);

        return await ordered
            .Skip(skip)
            .Take(take)
            .Select(joined => joined.joined)
            .Select(ToContentRow)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetAllVersionsAsync(Guid nodeKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // All versions for the node, no Current filter.
            List<TRow> rows = await BuildBaseQuery(
                    db,
                    db.Nodes.Where(node => node.UniqueId == nodeKey && node.NodeObjectType == NodeObjectTypeKey),
                    db.ContentVersions)
                .OrderByDescending(joined => joined.ContentVersion.Current)
                .ThenByDescending(joined => joined.ContentVersion.VersionDate)
                // Versions saved within the same clock tick share a VersionDate, so without an id tiebreak
                // their relative order is whatever the provider happens to return.
                .ThenByDescending(joined => joined.ContentVersion.Id)
                .Select(ToContentRow)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return Enumerable.Empty<TEntity>();
            }

            return await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);
        });

    /// <inheritdoc />
    /// <remarks>
    ///     Pages in SQL and hydrates without property data or templates - variants are still loaded, because
    ///     this feeds rollback, which needs them.
    /// </remarks>
    public virtual Task<IEnumerable<TEntity>> GetAllVersionsSlimAsync(Guid nodeKey, int skip, int take, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<TRow> rows = await BuildBaseQuery(
                    db,
                    db.Nodes.Where(node => node.UniqueId == nodeKey && node.NodeObjectType == NodeObjectTypeKey),
                    db.ContentVersions)
                .OrderByDescending(joined => joined.ContentVersion.Current)
                .ThenByDescending(joined => joined.ContentVersion.VersionDate)
                // Versions saved within the same clock tick share a VersionDate, so without an id tiebreak
                // paging over them can repeat or skip a version.
                .ThenByDescending(joined => joined.ContentVersion.Id)
                .Skip(skip)
                .Take(take)
                .Select(ToContentRow)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return Enumerable.Empty<TEntity>();
            }

            return await AssembleEntitiesAsync(db, rows, propertyAliases: [], loadTemplates: false, cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<TEntity?> GetVersionAsync(int versionId, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // Filter by the version's own id; no Current filter — historical versions are valid targets.
            // The Nodes join guards against returning versions that belong to a different object type.
            TRow? row = await BuildBaseQuery(
                    db,
                    db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey),
                    db.ContentVersions.Where(contentVersion => contentVersion.Id == versionId))
                .Select(ToContentRow)
                .FirstOrDefaultAsync(cancellationToken);

            if (row is null)
            {
                return null;
            }

            List<TEntity> entities = await AssembleEntitiesAsync(db, [row], propertyAliases: null, loadTemplates: true, cancellationToken);
            return entities.FirstOrDefault();
        });

    /// <inheritdoc />
    public virtual Task<PagedModel<TEntity>> GetChildrenAsync(
        Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken)
        => GetChildrenCoreAsync(parentKey, skip, take, propertyAliases, ordering, loadTemplates: true, cancellationToken);

    /// <summary>
    ///     Gets a page of the children of a node, or of the root when <paramref name="parentKey" /> is
    ///     <see langword="null" />, ordered as requested.
    /// </summary>
    /// <param name="parentKey">The parent key, or <see langword="null" /> for the root of the tree.</param>
    /// <param name="skip">The number of ordered children to skip.</param>
    /// <param name="take">The number of ordered children to take.</param>
    /// <param name="propertyAliases">
    ///     The property aliases to load, <see langword="null" /> for every property, or empty for none.
    /// </param>
    /// <param name="ordering">The ordering to apply; must not be <see langword="null" />.</param>
    /// <param name="loadTemplates">Forwarded to the post-assembly hook, for entity kinds that have templates.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page of children and the total number of children.</returns>
    protected Task<PagedModel<TEntity>> GetChildrenCoreAsync(
        Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, bool loadTemplates, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ordering);

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // A null parentKey means the root of the content tree - root has no Guid identity at all
            // (Constants.System.RootKey is deliberately null), so it can't be resolved via ResolveNodeIdAsync.
            int parentNodeId = parentKey.HasValue
                ? await ResolveNodeIdAsync(db, parentKey.Value, cancellationToken)
                : Constants.System.Root;

            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentNodeId)
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            IQueryable<TJoinRow> baseQuery = BuildBaseQuery(
                db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentNodeId));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<TRow> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases, loadTemplates, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchCustomFieldOrdered()
            {
                List<int> candidateNodeIds = await db.Nodes
                    .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentNodeId)
                    .Select(node => node.NodeId)
                    .ToListAsync(cancellationToken);

                return await FetchCustomFieldOrderedPageAsync(
                    db,
                    candidateNodeIds,
                    ordering!,
                    skip,
                    take,
                    pageNodeIds => baseQuery
                        .Where(joined => pageNodeIds.Contains(joined.Node.NodeId))
                        .Select(ToContentRow)
                        .ToListAsync(cancellationToken),
                    cancellationToken);
            }

            Task<IReadOnlyList<TRow>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchDefaultOrdered()
            {
                // Invariant name ordering falls through here: ApplyOrdering's default "name"
                // arm uses node.Text.
                IOrderedQueryable<TJoinRow> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering, PublishedOrderingSelector);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(ToContentRow)
                    .ToListAsync(cancellationToken);
            }

            return new PagedModel<TEntity> { Total = total, Items = items };
        });
    }

    /// <inheritdoc />
    public virtual Task<PagedModel<TEntity>> GetDescendantsAsync(
        Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true)
        => GetDescendantsCoreAsync(ancestorKey, skip, take, ordering, loadTemplates: true, includeTrashed, cancellationToken);

    /// <summary>
    ///     Gets a page of the descendants of a node, ordered as requested.
    /// </summary>
    /// <param name="ancestorKey">The ancestor key.</param>
    /// <param name="skip">The number of ordered descendants to skip.</param>
    /// <param name="take">The number of ordered descendants to take.</param>
    /// <param name="ordering">The ordering to apply; must not be <see langword="null" />.</param>
    /// <param name="loadTemplates">Forwarded to the post-assembly hook, for entity kinds that have templates.</param>
    /// <param name="includeTrashed">Whether descendants in the recycle bin are included.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page of descendants and the total number of descendants.</returns>
    protected Task<PagedModel<TEntity>> GetDescendantsCoreAsync(
        Guid ancestorKey, int skip, int take, Ordering? ordering, bool loadTemplates, bool includeTrashed, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ordering);

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // Matching on the ancestor's own path plus a separator keeps the predicate anchored. A leading
            // wildcard matches the same rows but can never use an index and is not sargable, so the anchored
            // form is the one worth keeping if umbracoNode.path is ever indexed. Root's seeded path is "-1",
            // so this produces "-1," for it - every content path begins with that.
            var ancestor = await db.Nodes
                .Where(node => node.UniqueId == ancestorKey)
                .Select(node => new { node.NodeId, node.Path })
                .SingleOrDefaultAsync(cancellationToken);

            if (ancestor is null)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            var pathMatch = $"{ancestor.Path},";

            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey
                    && EF.Functions.Like(node.Path, $"{pathMatch}%")
                    && (includeTrashed || node.Trashed == false))
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            IQueryable<TJoinRow> baseQuery = BuildBaseQuery(
                db,
                db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey
                    && EF.Functions.Like(node.Path, $"{pathMatch}%")
                    && (includeTrashed || node.Trashed == false)));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<TRow> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: loadTemplates, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchCustomFieldOrdered()
            {
                List<int> candidateNodeIds = await db.Nodes
                    .Where(node => node.NodeObjectType == NodeObjectTypeKey
                        && EF.Functions.Like(node.Path, $"{pathMatch}%")
                        && (includeTrashed || node.Trashed == false))
                    .Select(node => node.NodeId)
                    .ToListAsync(cancellationToken);

                return await FetchCustomFieldOrderedPageAsync(
                    db,
                    candidateNodeIds,
                    ordering!,
                    skip,
                    take,
                    pageNodeIds => baseQuery
                        .Where(joined => pageNodeIds.Contains(joined.Node.NodeId))
                        .Select(ToContentRow)
                        .ToListAsync(cancellationToken),
                    cancellationToken);
            }

            Task<IReadOnlyList<TRow>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchDefaultOrdered()
            {
                // Invariant name ordering falls through here: ApplyOrdering's default "name"
                // arm uses node.Text.
                IOrderedQueryable<TJoinRow> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering, PublishedOrderingSelector);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(ToContentRow)
                    .ToListAsync(cancellationToken);
            }

            return new PagedModel<TEntity> { Total = total, Items = items };
        });
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetRootContentAsync(CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<IEnumerable<TEntity>>(async db =>
        {
            List<TRow> rows = await BuildBaseQuery(
                    db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == Constants.System.Root))
                .Select(ToContentRow)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return [];
            }

            return await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetRecycleBinAsync(CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<IEnumerable<TEntity>>(async db =>
        {
            // Every trashed node of this object type, regardless of tree depth — not just the direct
            // children of the recycle bin folder itself.
            List<TRow> rows = await BuildBaseQuery(
                    db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed))
                .Select(ToContentRow)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return [];
            }

            return await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<PagedModel<TEntity>> GetPagedRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ordering);

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed)
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            IQueryable<TJoinRow> baseQuery = BuildBaseQuery(
                db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<TRow> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchCustomFieldOrdered()
            {
                List<int> candidateNodeIds = await db.Nodes
                    .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed)
                    .Select(node => node.NodeId)
                    .ToListAsync(cancellationToken);

                return await FetchCustomFieldOrderedPageAsync(
                    db,
                    candidateNodeIds,
                    ordering!,
                    skip,
                    take,
                    pageNodeIds => baseQuery
                        .Where(joined => pageNodeIds.Contains(joined.Node.NodeId))
                        .Select(ToContentRow)
                        .ToListAsync(cancellationToken),
                    cancellationToken);
            }

            Task<IReadOnlyList<TRow>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchDefaultOrdered()
            {
                IOrderedQueryable<TJoinRow> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering, PublishedOrderingSelector);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(ToContentRow)
                    .ToListAsync(cancellationToken);
            }

            return new PagedModel<TEntity> { Total = total, Items = items };
        });
    }

    /// <inheritdoc />
    public virtual Task<PagedModel<TEntity>> GetPagedOfContentTypesAsync(Guid[] contentTypeKeys, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ordering);

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // Content types are themselves nodes (their own umbracoNode row), so their Guid key lives on
            // NodeDto, not on ContentTypeDto — resolve keys to the underlying node IDs that ContentDto.ContentTypeId
            // actually stores, in one batched query rather than per-key IIdKeyMap round-trips.
            List<Guid> contentTypeKeysList = [.. contentTypeKeys];
            List<int> contentTypeIdsList = await db.ContentTypes
                .Join(
                    db.Nodes.Where(node => contentTypeKeysList.Contains(node.UniqueId)),
                    contentType => contentType.NodeId,
                    node => node.NodeId,
                    (contentType, node) => contentType.NodeId)
                .ToListAsync(cancellationToken);

            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey)
                .Join(
                    db.Content,
                    node => node.NodeId,
                    content => content.NodeId,
                    (node, content) => content)
                .Where(content => contentTypeIdsList.Contains(content.ContentTypeId))
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            // The content-type filter is applied as an ordinary Where() on the shared join tail's
            // output, rather than injected mid-chain — Content.ContentTypeId is exposed on every join
            // row, and every join after Content in the shared tail is cardinality-preserving
            // for a given (Node, Content) pair (current-version join is 1:1 under the "one current
            // version per node" invariant every method in this file already relies on; both LEFT JOINs
            // are 0-or-1), so the predicate commutes freely regardless of where it's written.
            IQueryable<TJoinRow> baseQuery = BuildBaseQuery(db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey))
                .Where(joined => contentTypeIdsList.Contains(joined.Content.ContentTypeId));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<TRow> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchCustomFieldOrdered()
            {
                List<int> candidateNodeIds = await db.Nodes
                    .Where(node => node.NodeObjectType == NodeObjectTypeKey)
                    .Join(
                        db.Content,
                        node => node.NodeId,
                        content => content.NodeId,
                        (node, content) => new { node, content })
                    .Where(joined => contentTypeIdsList.Contains(joined.content.ContentTypeId))
                    .Select(joined => joined.node.NodeId)
                    .ToListAsync(cancellationToken);

                return await FetchCustomFieldOrderedPageAsync(
                    db,
                    candidateNodeIds,
                    ordering!,
                    skip,
                    take,
                    pageNodeIds => baseQuery
                        .Where(joined => pageNodeIds.Contains(joined.Node.NodeId))
                        .Select(ToContentRow)
                        .ToListAsync(cancellationToken),
                    cancellationToken);
            }

            Task<IReadOnlyList<TRow>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchDefaultOrdered()
            {
                IOrderedQueryable<TJoinRow> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering, PublishedOrderingSelector);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(ToContentRow)
                    .ToListAsync(cancellationToken);
            }

            return new PagedModel<TEntity> { Total = total, Items = items };
        });
    }

    /// <summary>
    ///     Gets a page of the items of this kind at a tree level, excluding trashed ones, ordered as requested.
    ///     Exposed by the kinds whose contract has a level concept.
    /// </summary>
    /// <param name="level">The tree level.</param>
    /// <param name="skip">The number of ordered items to skip.</param>
    /// <param name="take">The number of ordered items to take.</param>
    /// <param name="ordering">The ordering to apply; must not be <see langword="null" />.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page of items and the total number of items at the level.</returns>
    protected Task<PagedModel<TEntity>> GetByLevelCoreAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ordering);

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Level == level && !node.Trashed)
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            // Trashed items are excluded here, unlike the other paged reads.
            IQueryable<TJoinRow> baseQuery = BuildBaseQuery(
                db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Level == level && !node.Trashed));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<TRow> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchCustomFieldOrdered()
            {
                List<int> candidateNodeIds = await db.Nodes
                    .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Level == level && !node.Trashed)
                    .Select(node => node.NodeId)
                    .ToListAsync(cancellationToken);

                return await FetchCustomFieldOrderedPageAsync(
                    db,
                    candidateNodeIds,
                    ordering!,
                    skip,
                    take,
                    pageNodeIds => baseQuery
                        .Where(joined => pageNodeIds.Contains(joined.Node.NodeId))
                        .Select(ToContentRow)
                        .ToListAsync(cancellationToken),
                    cancellationToken);
            }

            Task<IReadOnlyList<TRow>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<TRow>> FetchDefaultOrdered()
            {
                IOrderedQueryable<TJoinRow> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering, PublishedOrderingSelector);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(ToContentRow)
                    .ToListAsync(cancellationToken);
            }

            return new PagedModel<TEntity> { Total = total, Items = items };
        });
    }

    /// <summary>
    ///     Gets a page of the ancestors of an item of this kind, root-first. Only ancestors of the same kind are
    ///     returned and counted: a recycle bin or a container on the path is not one. Exposed by the kinds whose
    ///     contract has an ancestor concept.
    /// </summary>
    /// <param name="key">The key of the item.</param>
    /// <param name="skip">The number of ancestors to skip.</param>
    /// <param name="take">The number of ancestors to take.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page of ancestors and their total number; empty when the key is not an item of this kind.</returns>
    protected Task<PagedModel<TEntity>> GetAncestorsCoreAsync(Guid key, int skip, int take, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var self = await db.Nodes
                .Where(node => node.UniqueId == key && node.NodeObjectType == NodeObjectTypeKey)
                .Select(node => new { node.NodeId, node.Path })
                .FirstOrDefaultAsync(cancellationToken);

            if (self is null)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            // Path is stored root-first (e.g. "-1,1063,1066,1092"), so splitting it already yields the
            // ancestors in root-first order — excluding the root node itself and the node whose ancestors
            // are being requested, mirroring ContentExtensions.GetAncestorIds().
            List<int> pathNodeIds = [.. self.Path.Split(',')
                .Select(segment => int.Parse(segment, CultureInfo.InvariantCulture))
                .Where(nodeId => nodeId != Constants.System.Root && nodeId != self.NodeId)];

            if (pathNodeIds.Count == 0)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            List<int> ancestorsOfThisKind = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && pathNodeIds.Contains(node.NodeId))
                .Select(node => node.NodeId)
                .ToListAsync(cancellationToken);
            List<int> ancestorNodeIds = [.. pathNodeIds.Where(ancestorsOfThisKind.Contains)];

            int total = ancestorNodeIds.Count;
            if (total == 0)
            {
                return new PagedModel<TEntity> { Total = 0, Items = [] };
            }

            List<int> pageNodeIds = [.. ancestorNodeIds.Skip(skip).Take(take)];
            if (pageNodeIds.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TRow> rows = await BuildBaseQuery(
                    db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && pageNodeIds.Contains(node.NodeId)))
                .Select(ToContentRow)
                .ToListAsync(cancellationToken);

            // The Contains-filtered fetch above does not preserve pageNodeIds' root-first order, so
            // re-sort in memory to match it before assembling entities.
            Dictionary<int, int> position = pageNodeIds
                .Select((nodeId, index) => (nodeId, index))
                .ToDictionary(x => x.nodeId, x => x.index);
            List<TRow> orderedRows = [.. rows.OrderBy(row => position[row.Node.NodeId])];

            List<TEntity> items = await AssembleEntitiesAsync(db, orderedRows, propertyAliases: null, loadTemplates: true, cancellationToken);
            return new PagedModel<TEntity> { Total = total, Items = items };
        });

    /// <summary>
    ///     Gets whether the recycle bin of this kind holds an item of this kind directly under it. Exposed by the
    ///     kinds whose bin holds nothing but items of that kind.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true" /> when the bin is not empty.</returns>
    // A direct child of the recycle bin node is what makes the bin non-empty; anything trashed deeper down
    // has such an ancestor. The cached answer is read and then inserted in two steps rather than through
    // IAppPolicyCache.Get, whose factory is synchronous and would force this query to block on its result.
    protected async Task<bool> RecycleBinSmellsCoreAsync(CancellationToken cancellationToken)
    {
        // Answered on every backoffice tree render, so it is cached. The kind's cache refresher clears this
        // key on refresh, so the answer cannot outlive a change to what is in the bin.
        var cached = AppCaches.RuntimeCache.GetCacheItem<bool?>(RecycleBinCacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var smells = await AmbientScope.ExecuteWithContextAsync(db =>
            db.Nodes.AnyAsync(
                node => node.NodeObjectType == NodeObjectTypeKey
                    && db.Nodes.Where(bin => bin.UniqueId == RecycleBinKey).Select(bin => bin.NodeId).Contains(node.ParentId),
                cancellationToken));

        AppCaches.RuntimeCache.Insert(RecycleBinCacheKey, () => (bool?)smells);
        return smells;
    }

    /// <inheritdoc />
    protected override async Task<TEntity?> PerformGetAsync(Guid key)
    {
        List<TEntity> results = await PerformGetRangeAsync([key], propertyAliases: null, loadTemplates: true, CancellationToken.None);
        return results.FirstOrDefault();
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>?> PerformGetAllAsync()
            => await PerformGetRangeAsync(null, propertyAliases: null, loadTemplates: true, CancellationToken.None);

    /// <inheritdoc />
    protected override async Task<IEnumerable<TEntity>?> PerformGetManyAsync(Guid[] keys)
        => await PerformGetRangeAsync(keys, propertyAliases: null, loadTemplates: true, CancellationToken.None);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetManyAsync(Guid[] keys, string[]? propertyAliases, CancellationToken cancellationToken)
        => keys.Length == 0 ? [] : await PerformGetRangeAsync(keys, propertyAliases, loadTemplates: true, cancellationToken);

    /// <summary>
    ///     Reads and assembles the entities with the given keys, or every entity of this repository's kind when
    ///     <paramref name="keys" /> is <see langword="null" />.
    /// </summary>
    /// <param name="keys">The keys to read, or <see langword="null" /> for every entity.</param>
    /// <param name="propertyAliases">
    ///     The property aliases to load, <see langword="null" /> for every property, or empty for none.
    /// </param>
    /// <param name="loadTemplates">Forwarded to the post-assembly hook, for entity kinds that have templates.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The assembled entities.</returns>
    protected Task<List<TEntity>> PerformGetRangeAsync(Guid[]? keys, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<NodeDto> nodeQuery = db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey);
            if (keys is not null)
            {
                List<Guid> keyList = new(keys);
                nodeQuery = nodeQuery.Where(node => keyList.Contains(node.UniqueId));
            }

            List<TRow> rows = await BuildBaseQuery(db, nodeQuery)
                .Select(ToContentRow)
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return [];
            }

            return await AssembleEntitiesAsync(db, rows, propertyAliases, loadTemplates, cancellationToken);
        });

    /// <summary>
    ///     Builds fully populated entities from the rows a read query materialized: content types, property data,
    ///     the entity kind's variations, the post-assembly hook, then a clean dirty state and the parent key.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="rows">The rows to assemble, one per entity.</param>
    /// <param name="propertyAliases">
    ///     The property aliases to load, <see langword="null" /> for every property, or empty for none.
    /// </param>
    /// <param name="loadTemplates">Whether templates should be loaded, for entity kinds that have them.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The assembled entities, in row order.</returns>
    protected async Task<List<TEntity>> AssembleEntitiesAsync(UmbracoDbContext db, IReadOnlyList<TRow> rows, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken)
    {
        // All relevant version IDs (current + published, deduplicated).
        // Current and published are always different IDs after the first publish.
        var allVersionIds = rows
            .Select(row => row.ContentVersion.Id)
            .Concat(rows
                .Select(GetPublishedVersionId)
                .Where(publishedVersionId => publishedVersionId > 0))
            .Distinct()
            .ToList();

        Dictionary<int, List<PropertyDataDto>> propertyDtosByVersionId =
            await LoadPropertyDataAsync(db, allVersionIds, propertyAliases, cancellationToken);

        // Pre-populate content type map. ContentTypeRepository caches, so no extra DB round-trips
        // after the first call per type — and we need the types up front to gate variation queries.
        var contentTypeMap = new Dictionary<int, TContentType>();
        foreach (int contentTypeId in rows.Select(row => row.Content.ContentTypeId).Distinct())
        {
            TContentType? contentType = await ContentTypeRepository.GetAsync(contentTypeId, cancellationToken);
            if (contentType is null)
            {
                throw new InvalidOperationException($"Content type {contentTypeId} was not found.");
            }

            contentTypeMap[contentTypeId] = contentType;
        }

        Action<TEntity, TRow> applyVariations = await PrepareVariationsAsync(db, rows, allVersionIds, contentTypeMap, cancellationToken);

        var entities = new List<TEntity>(rows.Count);

        foreach (TRow row in rows)
        {
            TContentType contentType = contentTypeMap[row.Content.ContentTypeId];

            TEntity entity = BuildEntity(row, contentType);

            int publishedVersionId = GetPublishedVersionId(row);

            var versionPropertyDtos = new List<PropertyDataDto>();
            if (propertyDtosByVersionId.TryGetValue(row.ContentVersion.Id, out List<PropertyDataDto>? currentProps))
            {
                versionPropertyDtos.AddRange(currentProps);

                // A fetched version can itself be the node's published version (e.g. fetching a specific,
                // still-published version by id). PropertyFactory.BuildEntities classifies a dto as published
                // purely by VersionId == publishedVersionId, so without a distinct draft-side entry every
                // value would be published-only, leaving the unqualified (draft) accessor empty. Mirrors the
                // "dirty corner case" handled the same way in ContentRepositoryBase.GetPropertyCollections.
                if (row.ContentVersion.Id == publishedVersionId)
                {
                    versionPropertyDtos.AddRange(currentProps.Select(dto => dto.Clone(-1)));
                }
            }

            if (publishedVersionId > 0 &&
                publishedVersionId != row.ContentVersion.Id &&
                propertyDtosByVersionId.TryGetValue(publishedVersionId, out List<PropertyDataDto>? pubProps))
            {
                versionPropertyDtos.AddRange(pubProps);
            }

            if (propertyAliases is { Length: 0 })
            {
                // Empty alias list means "no custom properties" - mirrors the NPoco path's short-circuit in
                // ContentRepositoryBase.GetPropertyCollections, which skips property-slot creation entirely
                // rather than creating slots with unset values.
                entity.Properties = new PropertyCollection(new List<IProperty>());
            }
            else
            {
                IPropertyType[] compositionProperties = contentType.CompositionPropertyTypes.ToArray();
                entity.Properties = new PropertyCollection(
                    await PropertyFactory.BuildEntities(
                        compositionProperties,
                        versionPropertyDtos,
                        publishedVersionId,
                        LanguageRepository));
            }

            applyVariations(entity, row);

            entities.Add(entity);
        }

        await OnEntitiesAssembledAsync(db, entities, rows, loadTemplates, cancellationToken);

        for (var i = 0; i < entities.Count; i++)
        {
            TEntity entity = entities[i];
            TRow row = rows[i];

            // Applying variations above, and anything the post-assembly hook set, dirties the entity via
            // collection-changed notifications, so a freshly-assembled entity must be reset to a clean state
            // before being handed to the caller.
            entity.ResetDirtyProperties(false);

            // Root's node row exists (umbracoNode id -1) but carries Constants.System.RootSystemKey, not
            // the semantic "no parent" value ParentKey contracts to - row.ParentUniqueId comes straight
            // from a join on umbracoNode and must not be trusted for Root without this override.
            entity.ParentKey = row.Node.ParentId == Constants.System.Root
                ? null
                : row.ParentUniqueId;
        }

        return entities;
    }

    /// <summary>
    ///     Called once per assembled batch, after every entity has been built with its properties and variations and
    ///     before the entities are reset to a clean state. Override to populate entity-kind-specific members from the
    ///     rows, for example an entity kind with templates resolves and assigns them here.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="entities">The assembled entities, in row order.</param>
    /// <param name="rows">The rows the entities were built from, in the same order.</param>
    /// <param name="loadTemplates">Whether the caller asked for templates to be loaded.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected virtual Task OnEntitiesAssembledAsync(
        UmbracoDbContext db,
        IReadOnlyList<TEntity> entities,
        IReadOnlyList<TRow> rows,
        bool loadTemplates,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <summary>
    ///     Loads the property data rows of the given versions, grouped by version id.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="versionIds">The version ids to load property data for.</param>
    /// <param name="propertyAliases">
    ///     The property aliases to load, <see langword="null" /> for every property, or empty for none.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The property data rows keyed by version id.</returns>
    protected static async Task<Dictionary<int, List<PropertyDataDto>>> LoadPropertyDataAsync(
        UmbracoDbContext db, List<int> versionIds, string[]? propertyAliases, CancellationToken cancellationToken)
    {
        // Empty alias list means "no custom properties" — skip the query entirely.
        if (propertyAliases is { Length: 0 })
        {
            return new Dictionary<int, List<PropertyDataDto>>();
        }

        // Batched to stay within SQL Server's 2100-parameter limit.
        // versionIds can reach 2× the entity count (current + published version per entity).
        // The property aliases are parameters of the same statement as each batch of version ids. If there are too many
        // to leave a useful batch size, load all properties instead, which includes the requested ones.
        var filterByAliases = propertyAliases is { Length: > 0 } && propertyAliases.Length <= Constants.Sql.MaxParameterCount / 2;
        var batchSize = filterByAliases ? Constants.Sql.MaxParameterCount - propertyAliases!.Length : Constants.Sql.MaxParameterCount;
        var allPropertyData = new List<PropertyDataDto>();
        foreach (IEnumerable<int> batch in versionIds.InGroupsOf(batchSize))
        {
            var batchIds = batch.ToList();

            if (filterByAliases)
            {
                List<string> aliases = new(propertyAliases!);
                allPropertyData.AddRange(await db.PropertyData
                    .Where(propertyData => batchIds.Contains(propertyData.VersionId))
                    .Join(
                        db.PropertyTypes.Where(propertyType => propertyType.Alias != null && aliases.Contains(propertyType.Alias)),
                        propertyData => propertyData.PropertyTypeId,
                        propertyType => propertyType.Id,
                        (propertyData, propertyType) => propertyData)
                    .ToListAsync(cancellationToken));
            }
            else
            {
                allPropertyData.AddRange(await db.PropertyData
                    .Where(propertyData => batchIds.Contains(propertyData.VersionId))
                    .ToListAsync(cancellationToken));
            }
        }

        return allPropertyData
            .GroupBy(propertyData => propertyData.VersionId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    // Ties in the variant name are common - every node falling back to node.Text shares one - so the node id
    // breaks them, keeping paged results stable and non-duplicated across separate fetches; the node id is
    // appended after any user ordering for that reason (see http://issues.umbraco.org/issue/U4-8831).
    // Internal so the tiebreak can be exercised directly against a sequence with tied names, which a
    // database-backed test cannot reliably produce.
    internal static IOrderedQueryable<TSource> ApplyVariantNameOrdering<TSource>(
        IQueryable<TSource> source,
        Expression<Func<TSource, string?>> variantNameSelector,
        Expression<Func<TSource, int>> nodeIdSelector,
        bool descending)
        => descending
            ? source.OrderByDescending(variantNameSelector).ThenBy(nodeIdSelector)
            : source.OrderBy(variantNameSelector).ThenBy(nodeIdSelector);

    // The four typed PropertyData value columns, plus the SortableValue override some property editors
    // (e.g. IDataValueSortable) populate to take priority over the raw column. Mirrors the column
    // priority in ContentRepositoryBase.ApplyCustomOrdering (NPoco), but compares each with its native
    // .NET type instead of reformatting into a zero-padded string.
    private sealed record PropertyOrderingValue(
        string? SortableValue,
        int? IntegerValue,
        decimal? DecimalValue,
        DateTime? DateValue,
        string? VarcharValue);

    /// <summary>
    ///     Sorts, without paging, the full candidate node id set by the value of a custom property field.
    /// </summary>
    /// <remarks>
    ///     Values are fetched via ordinary translatable LINQ (no raw SQL, no per-provider SQL fragments); the sort
    ///     itself happens in memory, since no single SQL column can hold all four typed columns
    ///     (int/decimal/date/varchar) plus the string SortableValue override in one comparable form.
    /// </remarks>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="candidateNodeIds">The node ids to order, already filtered by the caller.</param>
    /// <param name="alias">The alias of the property type to order by.</param>
    /// <param name="culture">The culture whose variant values take part alongside the invariant ones.</param>
    /// <param name="direction">The direction to order in.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Every candidate node id, in the requested order.</returns>
    protected static async Task<List<int>> ResolveCustomFieldOrderedNodeIdsAsync(
        UmbracoDbContext db,
        List<int> candidateNodeIds,
        string alias,
        string culture,
        Direction direction,
        CancellationToken cancellationToken)
    {
        int languageId = await db.Language
            .Where(language => language.IsoCode == culture)
            .Select(language => language.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var valueByNodeId = new Dictionary<int, PropertyOrderingValue>();
        foreach (IEnumerable<int> batch in candidateNodeIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            var batchIds = batch.ToList();
            var batchRows = await db.ContentVersions
                .Where(contentVersion => contentVersion.Current && batchIds.Contains(contentVersion.NodeId))
                .Join(
                    db.PropertyData,
                    contentVersion => contentVersion.Id,
                    propertyData => propertyData.VersionId,
                    (contentVersion, propertyData) => new { contentVersion.NodeId, propertyData })
                .Join(
                    db.PropertyTypes.Where(propertyType => propertyType.Alias == alias),
                    joined => joined.propertyData.PropertyTypeId,
                    propertyType => propertyType.Id,
                    (joined, propertyType) => joined)
                .Where(joined => joined.propertyData.LanguageId == null || joined.propertyData.LanguageId == languageId)
                .Select(joined => new
                {
                    joined.NodeId,
                    joined.propertyData.Id,
                    joined.propertyData.SortableValue,
                    joined.propertyData.IntegerValue,
                    joined.propertyData.DecimalValue,
                    joined.propertyData.DateValue,
                    joined.propertyData.VarcharValue,
                })
                .ToListAsync(cancellationToken);

            // Deterministic tie-break for the (rare) case of multiple PropertyData rows per node
            // (e.g. segmented variants) — matches NPoco's equivalent ambiguity, not a new gap.
            foreach (var group in batchRows.GroupBy(row => row.NodeId))
            {
                var row = group.OrderBy(row => row.Id).First();
                valueByNodeId[group.Key] = new PropertyOrderingValue(row.SortableValue, row.IntegerValue, row.DecimalValue, row.DateValue, row.VarcharValue);
            }
        }

        List<int> ordered = new(candidateNodeIds);
        ordered.Sort((left, right) => CompareNodesByOrderingValue(left, right, valueByNodeId, direction));
        return ordered;
    }

    // Nodes without a value sort first ascending / last descending, matching both SQL Server's and
    // SQLite's default NULL placement — the same placement NPoco's LEFT JOIN implicitly relies on.
    private static int CompareNodesByOrderingValue(
        int leftNodeId,
        int rightNodeId,
        Dictionary<int, PropertyOrderingValue> valueByNodeId,
        Direction direction)
    {
        bool hasLeft = valueByNodeId.TryGetValue(leftNodeId, out PropertyOrderingValue? left);
        bool hasRight = valueByNodeId.TryGetValue(rightNodeId, out PropertyOrderingValue? right);

        if (!hasLeft && !hasRight)
        {
            return leftNodeId.CompareTo(rightNodeId);
        }

        if (!hasLeft || !hasRight)
        {
            int missingFirst = !hasLeft ? -1 : 1;
            return direction is Direction.Descending ? -missingFirst : missingFirst;
        }

        int comparison = CompareOrderingValues(left!, right!);
        if (comparison == 0)
        {
            return leftNodeId.CompareTo(rightNodeId);
        }

        return direction is Direction.Descending ? -comparison : comparison;
    }

    // Column priority mirrors NPoco's CASE expression (ContentRepositoryBase.ApplyCustomOrdering):
    // SortableValue overrides everything else, then IntegerValue, DecimalValue, DateValue, VarcharValue.
    // Unlike NPoco — which collapses every row to one string and does a single flat ORDER BY — this forms
    // a strict tier per row (whichever column is populated), so a mismatched pair (e.g. one row only has
    // IntegerValue, another only has VarcharValue, because the alias is reused across content types with
    // different storage types) compares by tier, not by NPoco's arbitrary cross-type string comparison.
    private static int CompareOrderingValues(PropertyOrderingValue left, PropertyOrderingValue right)
    {
        if (left.SortableValue is not null || right.SortableValue is not null)
        {
            return string.CompareOrdinal(left.SortableValue ?? string.Empty, right.SortableValue ?? string.Empty);
        }

        if (left.IntegerValue is not null || right.IntegerValue is not null)
        {
            return Nullable.Compare(left.IntegerValue, right.IntegerValue);
        }

        if (left.DecimalValue is not null || right.DecimalValue is not null)
        {
            return Nullable.Compare(left.DecimalValue, right.DecimalValue);
        }

        if (left.DateValue is not null || right.DateValue is not null)
        {
            return Nullable.Compare(left.DateValue, right.DateValue);
        }

        return string.CompareOrdinal(left.VarcharValue ?? string.Empty, right.VarcharValue ?? string.Empty);
    }

    /// <inheritdoc />
    public virtual async Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(
        ContentDataIntegrityReportOptions options,
        CancellationToken cancellationToken) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var report = new Dictionary<int, ContentDataIntegrityReportEntry>();
            var nodesToRebuild = new Dictionary<int, List<NodeDto>>();
            var validNodes = new Dictionary<int, NodeDto>();

            int[] rootIds = [Constants.System.Root, Constants.System.RecycleBinContent, Constants.System.RecycleBinMedia, Constants.System.RecycleBinElement];
            var currentParentIds = new HashSet<int>(rootIds);
            HashSet<int> prevParentIds = currentParentIds;
            var lastLevel = -1;

            List<NodeDto> nodes = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey)
                .OrderBy(node => node.Level)
                .ThenBy(node => node.ParentId)
                .ThenBy(node => node.SortOrder)
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

                string[] pathParts = [.. node.Path
                    .Split(Constants.CharArrays.Comma)
                    .Where(pathPart => !rootIds.Contains(int.Parse(pathPart, CultureInfo.InvariantCulture)))];

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
                        .Where(storedNode => storedNode.NodeId == node.NodeId)
                        .ExecuteUpdateAsync(
                            setters => setters
                                .SetProperty(storedNode => storedNode.Level, node.Level)
                                .SetProperty(storedNode => storedNode.Path, node.Path),
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

    /// <summary>
    ///     Builds the entity-specific version row (<typeparamref name="TVersionDto" />) for the entity's current
    ///     version, from <see cref="IContentBase.VersionId" />. Called once the version id is assigned.
    /// </summary>
    /// <param name="entity">The domain entity whose current version is being persisted.</param>
    /// <param name="publishing">
    ///     Whether that version is being published as part of the same save; a kind without a published flag on its
    ///     version row ignores it.
    /// </param>
    /// <returns>The version row ready to be saved.</returns>
    protected abstract TVersionDto BuildEntityVersionRow(TEntity entity, bool publishing);

    /// <summary>
    ///     Writes every column of the entity-specific version row with the given id, from a row built by
    ///     <see cref="BuildEntityVersionRow" />. Issued as an update statement rather than a tracked save because the
    ///     same context may still track the instance the row was inserted from.
    /// </summary>
    /// <param name="db">The database context of the ambient scope.</param>
    /// <param name="entityVersion">The row carrying the id and the values to write.</param>
    protected abstract Task UpdateEntityVersionRowAsync(UmbracoDbContext db, TVersionDto entityVersion);

    /// <summary>
    ///     Builds the node, content and content version rows of the entity, the rows every content kind shares.
    /// </summary>
    /// <param name="entity">The entity being persisted.</param>
    /// <param name="publishing">Whether the current version is being published as part of the same save.</param>
    /// <returns>The three rows, not yet written.</returns>
    protected ContentWriteRow BuildWriteRow(TEntity entity, bool publishing) =>
        new(
            ContentBaseFactory.BuildEFCoreNodeDto(entity, NodeObjectTypeKey),
            ContentBaseFactory.BuildEFCoreContentDto(entity),
            ContentBaseFactory.BuildEFCoreContentVersionDto(entity, publishing));

    /// <summary>
    ///     Deletes every row across every table that references a content node, in FK-safe (child-to-parent)
    ///     order, before deleting the <c>umbracoNode</c> row itself. Entity-specific tables (e.g. Document's
    ///     redirect URLs, domains) are deleted first via <see cref="PersistEntitySpecificDeleteClausesAsync" />,
    ///     then the tables shared across all content kinds.
    /// </summary>
    protected override async Task PersistDeletedItemAsync(TEntity entity)
    {
        // Raise event first else potential FK issues.
        EventAggregator.Publish(new ScopedEntityRemoveNotification(entity, new EventMessages()));

        var nodeId = entity.Id;

        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            await PersistEntitySpecificDeleteClausesAsync(db, nodeId);
            await PersistIntermediateDeleteClausesAsync(db, nodeId);

            await db.User2NodeNotifies.Where(notification => notification.NodeId == nodeId).ExecuteDeleteAsync();

            IQueryable<Guid> uniqueIdQuery = db.Nodes.Where(node => node.NodeId == nodeId).Select(node => node.UniqueId);
            await db.UserGroup2GranularPermissions
                .Where(granularPermission => granularPermission.UniqueId.HasValue && uniqueIdQuery.Contains(granularPermission.UniqueId.Value))
                .ExecuteDeleteAsync();

            await db.UserStartNodes.Where(startNode => startNode.StartNode == nodeId).ExecuteDeleteAsync();
            await db.Relations.Where(relation => relation.ParentId == nodeId).ExecuteDeleteAsync();
            await db.Relations.Where(relation => relation.ChildId == nodeId).ExecuteDeleteAsync();
            await db.TagRelationships.Where(tagRelationship => tagRelationship.NodeId == nodeId).ExecuteDeleteAsync();

            IQueryable<int> versionIdQuery = db.ContentVersions.Where(contentVersion => contentVersion.NodeId == nodeId).Select(contentVersion => contentVersion.Id);
            await db.PropertyData.Where(propertyData => versionIdQuery.Contains(propertyData.VersionId)).ExecuteDeleteAsync();
            await db.ContentVersionCultureVariations.Where(cultureVariation => versionIdQuery.Contains(cultureVariation.VersionId)).ExecuteDeleteAsync();
            await db.Set<TVersionDto>().Where(entityVersion => versionIdQuery.Contains(entityVersion.Id)).ExecuteDeleteAsync();
            await db.ContentVersions.Where(contentVersion => contentVersion.NodeId == nodeId).ExecuteDeleteAsync();
            await db.Content.Where(content => content.NodeId == nodeId).ExecuteDeleteAsync();
            await db.Nodes.Where(node => node.NodeId == nodeId).ExecuteDeleteAsync();

            return true;
        });

        // We need to flush the isolated cache by key explicitly here. The ContentCacheRefresher does the same
        // thing, but by the time it's invoked, custom notification handlers might have already consumed the
        // cached version. The insert and update paths flush for the same reason.
        IsolatedCache.Clear(EntityTypeCacheKeyPrefix + entity.Key);

        entity.DeleteDate = DateTime.UtcNow;
    }

    /// <summary>
    ///     Deletes the rows in tables specific to this content kind (e.g. Document's redirect URLs, domains,
    ///     access rules) that reference the node being deleted. Called by <see cref="PersistDeletedItemAsync" />
    ///     before the tables shared across all content kinds. The <typeparamref name="TVersionDto" /> rows are not
    ///     part of this: the caller deletes them with the versions they belong to.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="nodeId">The node identifier of the content being deleted.</param>
    protected abstract Task PersistEntitySpecificDeleteClausesAsync(UmbracoDbContext db, int nodeId);

    /// <summary>
    ///     Deletes the rows in tables an intermediate base adds for its kinds (e.g. the schedules of publishable
    ///     content) that reference the node being deleted. Called by <see cref="PersistDeletedItemAsync" /> after
    ///     the entity-specific clauses and before the tables shared across all content kinds. Does nothing by
    ///     default.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="nodeId">The node identifier of the content being deleted.</param>
    protected virtual Task PersistIntermediateDeleteClausesAsync(UmbracoDbContext db, int nodeId) => Task.CompletedTask;

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(TEntity item) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            item.AddingEntity();

            await SanitizeNamesAsync(db, item);
            item.SanitizeEntityPropertiesForXmlStorage();

            (NodeDto nodeDto, ContentDto contentDto, ContentVersionDto contentVersionDto) = BuildWriteRow(item, publishing: false);

            await PersistNewNodeAsync(db, item, nodeDto);
            await PersistNewContentAsync(db, item, contentDto);
            await PersistNewVersionAsync(db, item, contentVersionDto, publishing: false);
            await PersistNewPropertyDataAsync(db, item, publishedVersionId: 0);

            await SetEntityTagsAsync(item);
            await OnUowRefreshedEntityAsync(item, CancellationToken.None);

            item.ResetDirtyProperties();

            return true;
        });

    /// <inheritdoc />
    protected override Task PersistUpdatedItemAsync(TEntity item) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            ContentVersionDto version = await GetCurrentVersionRowAsync(db, item);

            item.UpdatingEntity();

            // A descendant moved as part of a bulk move has only Path, Level and UpdateDate dirty, so the
            // version, property data and tag steps are skipped; a bulk move cannot be rolled back anyway.
            var isMoving = item.IsMoving();

            if (!isMoving)
            {
                await SanitizeNamesAsync(db, item);
                item.SanitizeEntityPropertiesForXmlStorage();
                await ApplyParentChangeAsync(db, item);
            }

            (NodeDto nodeDto, ContentDto contentDto, ContentVersionDto contentVersionDto) = BuildWriteRow(item, publishing: false);

            await UpdateNodeRowAsync(db, item, nodeDto);

            if (isMoving)
            {
                await OnUowRefreshedEntityAsync(item, CancellationToken.None);
                item.ResetDirtyProperties();
                IsolatedCache.Clear(EntityTypeCacheKeyPrefix + item.Key);
                return true;
            }

            await UpdateContentRowAsync(db, item, contentDto);
            await UpdateVersionRowsAsync(db, item, contentVersionDto, version.PreventCleanup, BuildEntityVersionRow(item, publishing: false));
            await PersistUpdatedPropertyDataAsync(db, item, item.VersionId, publishedVersionId: 0);

            await SetEntityTagsAsync(item);
            await OnUowRefreshedEntityAsync(item, CancellationToken.None);

            item.ResetDirtyProperties();

            // Flushed by key explicitly: the cache refresher does the same, but by the time it runs, custom
            // notification handlers might already have consumed the cached version.
            IsolatedCache.Clear(EntityTypeCacheKeyPrefix + item.Key);

            return true;
        });

    /// <summary>
    ///     Inserts the node row (or converts the placeholder that reserved its id), resolving the path, level and sort
    ///     order from the parent, and assigns the entity its id, path, level, sort order and parent key.
    /// </summary>
    protected async Task PersistNewNodeAsync(UmbracoDbContext db, TEntity item, NodeDto nodeDto)
    {
        NodeDto parent = await GetParentNodeDtoAsync(db, item.ParentId);
        var level = parent.Level + 1;
        int sortOrder = await ResolveNewItemSortOrderAsync(db, item);

        nodeDto.Path = parent.Path;
        nodeDto.Level = Convert.ToInt16(level);
        nodeDto.SortOrder = sortOrder;

        // Supports blueprint/import flows that pre-reserve a Key<->NodeId mapping via a placeholder
        // IdReservation node before the real entity is created.
        var reservedId = await GetReservedIdAsync(db, nodeDto.UniqueId);
        if (reservedId > 0)
        {
            nodeDto.NodeId = reservedId;
            nodeDto.Path = string.Concat(parent.Path, ",", reservedId);
            ValidatePath(nodeDto);

            await db.Nodes.Where(node => node.NodeId == reservedId).ExecuteUpdateAsync(setters => setters
                .SetProperty(node => node.UniqueId, nodeDto.UniqueId)
                .SetProperty(node => node.ParentId, nodeDto.ParentId)
                .SetProperty(node => node.Level, nodeDto.Level)
                .SetProperty(node => node.Path, nodeDto.Path)
                .SetProperty(node => node.SortOrder, nodeDto.SortOrder)
                .SetProperty(node => node.Trashed, nodeDto.Trashed)
                .SetProperty(node => node.UserId, nodeDto.UserId)
                .SetProperty(node => node.Text, nodeDto.Text)
                .SetProperty(node => node.NodeObjectType, nodeDto.NodeObjectType)
                .SetProperty(node => node.CreateDate, nodeDto.CreateDate));
        }
        else
        {
            db.Nodes.Add(nodeDto);
            await db.SaveChangesAsync();

            // The node stays tracked from the Add above, so mutating it directly and saving again is
            // enough to patch the Path — no ExecuteUpdateAsync needed despite the context's global
            // NoTracking query setting (that setting only affects query results, not tracked Adds).
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            ValidatePath(nodeDto);
            await db.SaveChangesAsync();
        }

        item.Id = nodeDto.NodeId;
        item.Path = nodeDto.Path;
        item.SortOrder = sortOrder;
        item.Level = level;
        item.ParentKey = ResolveParentKey(item.ParentId, parent);
    }

    /// <summary>
    ///     Resolves the sort order a new item is inserted with. The default takes the next free sort order under
    ///     the parent; a kind that lets callers place an item overrides it.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity being inserted.</param>
    /// <returns>The sort order to insert with.</returns>
    protected virtual Task<int> ResolveNewItemSortOrderAsync(UmbracoDbContext db, TEntity item) =>
        GetNewChildSortOrderAsync(db, item.ParentId, 0);

    /// <summary>Inserts the content row of a new entity.</summary>
    protected static async Task PersistNewContentAsync(UmbracoDbContext db, TEntity item, ContentDto contentDto)
    {
        contentDto.NodeId = item.Id;
        db.Content.Add(contentDto);
        await db.SaveChangesAsync();
    }

    /// <summary>
    ///     Inserts the content version row of a new entity as its current version and the kind-specific version row
    ///     for it, and assigns the entity its version id.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity, with its id assigned.</param>
    /// <param name="contentVersionDto">The content version row built for the entity.</param>
    /// <param name="publishing">Whether the version is being published as part of the same save.</param>
    protected async Task PersistNewVersionAsync(UmbracoDbContext db, TEntity item, ContentVersionDto contentVersionDto, bool publishing)
    {
        contentVersionDto.NodeId = item.Id;
        db.ContentVersions.Add(contentVersionDto);
        await db.SaveChangesAsync();
        item.VersionId = contentVersionDto.Id;

        db.Set<TVersionDto>().Add(BuildEntityVersionRow(item, publishing));
        await db.SaveChangesAsync();
    }

    /// <summary>
    ///     Reads the row of the version the entity is saving, which must be its current version.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity being updated.</param>
    /// <returns>The current version row.</returns>
    /// <exception cref="InvalidOperationException">The entity's version is not its current version.</exception>
    protected static async Task<ContentVersionDto> GetCurrentVersionRowAsync(UmbracoDbContext db, TEntity item)
    {
        // whatever we do, we must check that we are saving the current version
        ContentVersionDto? version = await db.ContentVersions.FirstOrDefaultAsync(contentVersion => contentVersion.Id == item.VersionId);
        if (version is null || !version.Current)
        {
            throw new InvalidOperationException("Cannot save a non-current version.");
        }

        return version;
    }

    /// <summary>
    ///     Re-homes the entity under its new parent when its parent changed: path, level, sort order and parent key.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity being updated.</param>
    protected async Task ApplyParentChangeAsync(UmbracoDbContext db, TEntity item)
    {
        if (!item.IsPropertyDirty(nameof(item.ParentId)))
        {
            return;
        }

        NodeDto parent = await GetParentNodeDtoAsync(db, item.ParentId);
        item.Path = string.Concat(parent.Path, ",", item.Id);
        item.Level = parent.Level + 1;
        item.SortOrder = await GetNewChildSortOrderAsync(db, item.ParentId, 0);
        item.ParentKey = ResolveParentKey(item.ParentId, parent);
    }

    /// <summary>Writes the mutable columns of the entity's node row.</summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity being updated.</param>
    /// <param name="nodeDto">The node row built for the entity.</param>
    protected static async Task UpdateNodeRowAsync(UmbracoDbContext db, TEntity item, NodeDto nodeDto)
    {
        ValidatePath(nodeDto);
        await db.Nodes.Where(node => node.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(node => node.Text, nodeDto.Text)
            .SetProperty(node => node.ParentId, nodeDto.ParentId)
            .SetProperty(node => node.Level, nodeDto.Level)
            .SetProperty(node => node.Path, nodeDto.Path)
            .SetProperty(node => node.SortOrder, nodeDto.SortOrder)
            .SetProperty(node => node.Trashed, nodeDto.Trashed)
            .SetProperty(node => node.UserId, nodeDto.UserId));
    }

    /// <summary>Writes the mutable columns of the entity's content row.</summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity being updated.</param>
    /// <param name="contentDto">The content row built for the entity.</param>
    protected static Task UpdateContentRowAsync(UmbracoDbContext db, TEntity item, ContentDto contentDto) =>
        db.Content.Where(content => content.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(content => content.ContentTypeId, contentDto.ContentTypeId));

    /// <summary>
    ///     Writes the content version row of the entity's current version and the kind-specific version row for it.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity being updated.</param>
    /// <param name="contentVersionDto">The content version row built for the entity.</param>
    /// <param name="preventCleanup">The stored prevent-cleanup flag, which a save keeps.</param>
    /// <param name="entityVersionDto">The kind-specific version row built for the entity.</param>
    protected async Task UpdateVersionRowsAsync(UmbracoDbContext db, TEntity item, ContentVersionDto contentVersionDto, bool preventCleanup, TVersionDto entityVersionDto)
    {
        contentVersionDto.PreventCleanup = preventCleanup;

        await db.ContentVersions.Where(contentVersion => contentVersion.Id == item.VersionId).ExecuteUpdateAsync(setters => setters
            .SetProperty(contentVersion => contentVersion.VersionDate, contentVersionDto.VersionDate)
            .SetProperty(contentVersion => contentVersion.UserId, contentVersionDto.UserId)
            .SetProperty(contentVersion => contentVersion.Current, contentVersionDto.Current)
            .SetProperty(contentVersion => contentVersion.Text, contentVersionDto.Text)
            .SetProperty(contentVersion => contentVersion.PreventCleanup, contentVersionDto.PreventCleanup));

        await UpdateEntityVersionRowAsync(db, entityVersionDto);
    }

    /// <summary>
    ///     Inserts the property data rows of a new entity's current version and, when the kind has one, of its
    ///     published version.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity, with its version id assigned.</param>
    /// <param name="publishedVersionId">The id of the published version, or 0 when there is none.</param>
    /// <returns>Whether the entity is edited and, for variant content, the cultures that are.</returns>
    protected async Task<(bool Edited, HashSet<string>? EditedCultures)> PersistNewPropertyDataAsync(UmbracoDbContext db, TEntity item, int publishedVersionId)
    {
        (List<PropertyDataDto> propertyDataDtos, var edited, HashSet<string>? editedCultures) = await PropertyFactory.BuildEFCoreDtosAsync(
            item.ContentType.Variations,
            item.VersionId,
            publishedVersionId,
            item.Properties,
            LanguageRepository,
            PropertyEditors);

        SetEntitySortableValues(item, propertyDataDtos);

        if (propertyDataDtos.Count > 0)
        {
            db.PropertyData.AddRange(propertyDataDtos);
            await db.SaveChangesAsync();
        }

        return (edited, editedCultures);
    }

    /// <summary>
    ///     Diff-reconciles the property data rows of a version against the freshly built values of the entity's
    ///     current properties: matches existing rows by property type, version, language and segment, updates the
    ///     matches in place, inserts rows for unmatched values and deletes rows that no longer have a value.
    /// </summary>
    /// <remarks>
    ///     The fetch takes no row lock: every caller holds the tree write lock, and the scope shares one connection
    ///     and transaction, so that lock already serializes this read-then-write sequence.
    /// </remarks>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="item">The entity being updated.</param>
    /// <param name="versionId">The version whose rows are reconciled.</param>
    /// <param name="publishedVersionId">The id of the published version, or 0 when there is none.</param>
    /// <returns>Whether the entity is edited and, for variant content, the cultures that are.</returns>
    protected async Task<(bool Edited, HashSet<string>? EditedCultures)> PersistUpdatedPropertyDataAsync(
        UmbracoDbContext db, TEntity item, int versionId, int publishedVersionId)
    {
        // Tracked (overriding the context's global NoTracking default) so the toUpdate loop below can mutate
        // these instances directly and have EF Core batch them into a single SaveChangesAsync round-trip,
        // instead of issuing one ExecuteUpdateAsync per row.
        List<PropertyDataDto> existingPropertyData = await db.PropertyData
            .AsTracking()
            .Where(propertyData => propertyData.VersionId == versionId)
            .ToListAsync();

        var propertyTypeToPropertyData = new Dictionary<(int PropertyTypeId, int VersionId, int? LanguageId, string? Segment), PropertyDataDto>();
        var trackedById = new Dictionary<int, PropertyDataDto>();
        var existingPropertyDataIds = new List<int>();
        foreach (PropertyDataDto propertyData in existingPropertyData)
        {
            existingPropertyDataIds.Add(propertyData.Id);
            propertyTypeToPropertyData[(propertyData.PropertyTypeId, propertyData.VersionId, propertyData.LanguageId, propertyData.Segment)] = propertyData;
            trackedById[propertyData.Id] = propertyData;
        }

        (List<PropertyDataDto> propertyDataDtos, var edited, HashSet<string>? editedCultures) = await PropertyFactory.BuildEFCoreDtosAsync(
            item.ContentType.Variations,
            item.VersionId,
            publishedVersionId,
            item.Properties,
            LanguageRepository,
            PropertyEditors);

        SetEntitySortableValues(item, propertyDataDtos);

        var toUpdate = new List<PropertyDataDto>();
        var toInsert = new List<PropertyDataDto>();
        foreach (PropertyDataDto propertyDataDto in propertyDataDtos)
        {
            // Check if this already exists and update, else insert a new one.
            if (propertyTypeToPropertyData.TryGetValue(
                    (propertyDataDto.PropertyTypeId, propertyDataDto.VersionId, propertyDataDto.LanguageId, propertyDataDto.Segment),
                    out PropertyDataDto? existing))
            {
                propertyDataDto.Id = existing.Id;
                toUpdate.Add(propertyDataDto);
            }
            else
            {
                toInsert.Add(propertyDataDto);
            }

            // Track which ones have been processed. For entries in toInsert, propertyDataDto.Id is still
            // 0 here — Remove(0) is a harmless no-op unless an existing row happens to have Id 0, which
            // can't happen for a real PK.
            existingPropertyDataIds.Remove(propertyDataDto.Id);
        }

        // Mutate the tracked instances directly rather than issuing one ExecuteUpdateAsync per row — EF Core
        // batches these together with the toInsert Adds below into a single SaveChangesAsync round-trip.
        foreach (PropertyDataDto propertyDataDto in toUpdate)
        {
            PropertyDataDto tracked = trackedById[propertyDataDto.Id];
            tracked.LanguageId = propertyDataDto.LanguageId;
            tracked.Segment = propertyDataDto.Segment;
            tracked.IntegerValue = propertyDataDto.IntegerValue;
            tracked.DecimalValue = propertyDataDto.DecimalValue;
            tracked.DateValue = propertyDataDto.DateValue;
            tracked.VarcharValue = propertyDataDto.VarcharValue;
            tracked.TextValue = propertyDataDto.TextValue;
            tracked.SortableValue = propertyDataDto.SortableValue;
        }

        if (toInsert.Count > 0)
        {
            db.PropertyData.AddRange(toInsert);
        }

        if (toUpdate.Count > 0 || toInsert.Count > 0)
        {
            await db.SaveChangesAsync();
        }

        // For any remaining that haven't been processed, they need to be deleted. Batched per this repo's
        // 2100-parameter convention (Infrastructure/CLAUDE.md §6) since an entity with many
        // culture/segment-variant properties can plausibly exceed it.
        if (existingPropertyDataIds.Count > 0)
        {
            foreach (IEnumerable<int> batch in existingPropertyDataIds.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                List<int> batchIds = [.. batch];
                await db.PropertyData.Where(propertyData => batchIds.Contains(propertyData.Id)).ExecuteDeleteAsync();
            }
        }

        return (edited, editedCultures);
    }

    /// <summary>
    ///     Gets a value indicating whether saving must keep names unique among siblings.
    /// </summary>
    protected virtual bool EnsureUniqueNaming => true;

    /// <summary>
    ///     Ensures the entity has a valid, sibling-unique invariant name.
    /// </summary>
    protected async Task SanitizeNamesAsync(UmbracoDbContext db, TEntity content)
    {
        await EnsureInvariantNameExistsAsync(content);
        await EnsureInvariantNameIsUniqueAsync(db, content);
    }

    private async Task EnsureInvariantNameExistsAsync(TEntity content)
    {
        if (content.ContentType.VariesByCulture())
        {
            // content varies by culture
            // then it must have at least a variant name, else it makes no sense
            if (content.CultureInfos?.Count == 0)
            {
                throw new InvalidOperationException("Cannot save content with an empty name.");
            }

            // and then, we need to set the invariant name implicitly,
            // using the default culture if it has a name, otherwise anything we can
            var defaultCulture = await LanguageRepository.GetDefaultIsoCodeAsync();
            content.Name = defaultCulture is not null &&
                           (content.CultureInfos?.TryGetValue(defaultCulture, out ContentCultureInfos? cultureName) ??
                            false)
                ? cultureName!.Name
                : content.CultureInfos![0].Name;
        }
        else
        {
            // content is invariant, and invariant content must have an explicit invariant name
            if (string.IsNullOrWhiteSpace(content.Name))
            {
                throw new InvalidOperationException("Cannot save content with an empty name.");
            }
        }
    }

    private async Task EnsureInvariantNameIsUniqueAsync(UmbracoDbContext db, TEntity content) =>
        content.Name = await EnsureUniqueNodeNameAsync(db, content.ParentId, content.Name, content.Id);

    /// <summary>
    ///     Resolves a sibling-unique name for <paramref name="nodeName" /> among the other nodes sharing
    ///     <paramref name="parentId" />. The default implementation only resolves literal duplicate names
    ///     (via <see cref="SimilarNodeName.GetUniqueName(IEnumerable{SimilarNodeName},int,string)" />) — override
    ///     to add further uniqueness checks (e.g. URL segment collisions), as <c>DocumentRepository</c> does.
    /// </summary>
    protected virtual async Task<string?> EnsureUniqueNodeNameAsync(UmbracoDbContext db, int parentId, string? nodeName, int id)
    {
        if (!EnsureUniqueNaming)
        {
            return nodeName;
        }

        (string? uniqueName, List<SimilarNodeName> _) = await GetUniqueNodeNameAndSiblingsAsync(db, parentId, nodeName, id);
        return uniqueName;
    }

    /// <summary>
    ///     Fetches every sibling under <paramref name="parentId" /> (of this repository's <c>NodeObjectTypeKey</c>)
    ///     and resolves a sibling-unique name for <paramref name="nodeName" />, returning both so overrides of
    ///     <see cref="EnsureUniqueNodeNameAsync" /> can run further checks against the same sibling list without
    ///     re-querying. Deliberately unfiltered (no name-prefix narrowing) — unlike NPoco's generic
    ///     Media/DataType-oriented sibling fetch, a URL-segment collision check (see <c>DocumentRepository</c>)
    ///     needs the full sibling list, since two names with different literal prefixes can still collide on
    ///     URL segment.
    /// </summary>
    protected async Task<(string? UniqueName, List<SimilarNodeName> Siblings)> GetUniqueNodeNameAndSiblingsAsync(
        UmbracoDbContext db, int parentId, string? nodeName, int id)
    {
        // TODO: fetch only siblings whose name starts with SimilarNodeName.GetBaseText(nodeName), as the NPoco
        // EnsureUniqueNodeName does; loading every sibling makes each write O(siblings) on flat trees. [NL]
        List<SimilarNodeName> siblings = await db.Nodes
            .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentId)
            .Select(node => new SimilarNodeName { Id = node.NodeId, Name = node.Text })
            .ToListAsync();

        return (SimilarNodeName.GetUniqueName(siblings, id, nodeName), siblings);
    }

    /// <summary>
    ///     Replaces the tags stored for every tag-enabled property of the entity with the tags its current values
    ///     yield.
    /// </summary>
    protected async Task SetEntityTagsAsync(TEntity entity)
    {
        foreach (IProperty property in entity.Properties)
        {
            if (PropertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? editor) is false)
            {
                continue;
            }

            if (editor.GetValueEditor() is not IDataValueTags tagsProvider)
            {
                // Support for legacy tag editors — everything from here down to the last continue can be
                // removed when TagsPropertyEditorAttribute is removed.
                TagConfiguration? tagConfiguration = property.GetTagConfiguration(PropertyEditors, DataTypeService, _idKeyMap);
                if (tagConfiguration is null)
                {
                    continue;
                }

                if (property.PropertyType.VariesByCulture())
                {
                    var tags = new List<ITag>();
                    foreach (IPropertyValue pvalue in property.Values)
                    {
                        IEnumerable<string> tagsValue = property.GetTagsValue(PropertyEditors, DataTypeService, _idKeyMap, _jsonSerializer, pvalue.Culture);
                        int? languageId = await LanguageRepository.GetIdByIsoCodeAsync(pvalue.Culture);
                        IEnumerable<Tag> cultureTags = tagsValue.Select(tagText => new Tag { Group = tagConfiguration.Group, Text = tagText, LanguageId = languageId });
                        tags.AddRange(cultureTags);
                    }

                    _tagRepository.Assign(entity.Id, property.PropertyTypeId, tags);
                }
                else
                {
                    IEnumerable<string> tagsValue = property.GetTagsValue(PropertyEditors, DataTypeService, _idKeyMap, _jsonSerializer);
                    IEnumerable<Tag> tags = tagsValue.Select(tagText => new Tag { Group = tagConfiguration.Group, Text = tagText });
                    _tagRepository.Assign(entity.Id, property.PropertyTypeId, tags);
                }

                continue;
            }

            object? configurationObject = property.PropertyType.GetDataType(DataTypeService, _idKeyMap)?.ConfigurationObject;

            if (property.PropertyType.VariesByCulture())
            {
                var tags = new List<ITag>();
                foreach (IPropertyValue pvalue in property.Values)
                {
                    int? languageId = await LanguageRepository.GetIdByIsoCodeAsync(pvalue.Culture);
                    tags.AddRange(tagsProvider.GetTags(pvalue.EditedValue, configurationObject, languageId));
                }

                _tagRepository.Assign(entity.Id, property.PropertyTypeId, tags);
            }
            else
            {
                IEnumerable<ITag> tags = tagsProvider.GetTags(property.GetValue(), configurationObject, null);
                _tagRepository.Assign(entity.Id, property.PropertyTypeId, tags);
            }
        }
    }

    /// <summary>Removes every tag stored for the entity.</summary>
    protected void ClearEntityTags(TEntity entity) => _tagRepository.RemoveAll(entity.Id);

    /// <summary>
    ///     Populates <see cref="PropertyDataDto.SortableValue" /> for any property whose editor implements
    ///     <see cref="IDataValueSortable" />, so custom-field ordering (see <see cref="ResolveCustomFieldOrderedNodeIdsAsync" />)
    ///     can prioritize it over the raw typed columns.
    /// </summary>
    protected void SetEntitySortableValues(TEntity entity, IEnumerable<PropertyDataDto> propertyDtos)
    {
        var dtosByPropertyTypeId = propertyDtos.GroupBy(dto => dto.PropertyTypeId).ToDictionary(group => group.Key, group => group.ToList());

        foreach (IProperty property in entity.Properties)
        {
            if (PropertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? editor) is false)
            {
                continue;
            }

            if (editor.GetValueEditor() is not IDataValueSortable sortableProvider)
            {
                continue;
            }

            if (dtosByPropertyTypeId.TryGetValue(property.PropertyTypeId, out List<PropertyDataDto>? dtos) is false)
            {
                continue;
            }

            object? configurationObject = property.PropertyType.GetDataType(DataTypeService, _idKeyMap)?.ConfigurationObject;

            foreach (PropertyDataDto dto in dtos)
            {
                object? value = dto.TextValue ?? dto.VarcharValue ?? (object?)dto.DateValue ?? dto.DecimalValue ?? dto.IntegerValue;
                dto.SortableValue = sortableProvider.GetSortableValue(value, configurationObject);
            }
        }
    }

    /// <summary>Gets the node row of the given parent, the root included.</summary>
    protected static Task<NodeDto> GetParentNodeDtoAsync(UmbracoDbContext db, int parentId) =>
        db.Nodes.FirstAsync(node => node.NodeId == parentId);

    /// <summary>
    ///     Resolves the parent key a persisted entity carries. Root's node row exists (umbracoNode id -1) but carries
    ///     <see cref="Constants.System.RootSystemKey" />, not the semantic "no parent" value ParentKey contracts to, so
    ///     its key must not be taken from the row. Every other parent, the recycle bin included, carries the key the
    ///     contract expects.
    /// </summary>
    protected static Guid? ResolveParentKey(int parentId, NodeDto parent) =>
        parentId == Constants.System.Root ? null : parent.UniqueId;

    /// <summary>Checks whether a sibling of this repository's kind already holds the given sort order.</summary>
    protected Task<bool> SortorderExistsAsync(UmbracoDbContext db, int parentId, int sortOrder) =>
        db.Nodes.AnyAsync(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentId && node.SortOrder == sortOrder);

    /// <summary>Gets the next free sort order under the parent for this repository's kind, or <paramref name="first" /> when it has no children.</summary>
    protected async Task<int> GetNewChildSortOrderAsync(UmbracoDbContext db, int parentId, int first)
    {
        int? maxSortOrder = await db.Nodes
            .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentId)
            .Select(node => (int?)node.SortOrder)
            .MaxAsync();

        return maxSortOrder + 1 ?? first;
    }

    /// <summary>
    ///     Gets the node id reserved for the given key by an id-reservation placeholder, or 0 when none exists.
    ///     Supports blueprint and import flows that pre-reserve a key-to-id mapping before the entity is created.
    /// </summary>
    protected static async Task<int> GetReservedIdAsync(UmbracoDbContext db, Guid uniqueId)
    {
        int? id = await db.Nodes
            .Where(node => node.UniqueId == uniqueId && node.NodeObjectType == Constants.ObjectTypes.IdReservation)
            .Select(node => (int?)node.NodeId)
            .FirstOrDefaultAsync();

        return id ?? 0;
    }

    /// <summary>
    ///     Sanity-checks that a freshly built path is well-formed, mirroring NPoco's
    ///     <c>NodeDto.ValidatePathWithException</c>, which is defined against the NPoco node type and so cannot be
    ///     reused against the EF Core one.
    /// </summary>
    protected static void ValidatePath(NodeDto node)
    {
        if (node.NodeId == default && string.IsNullOrWhiteSpace(node.Path))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(node.Path))
        {
            throw new InvalidDataException($"The content item {node.NodeId} has an empty path: {node.Path} with parentID: {node.ParentId}");
        }

        string[] pathParts = node.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length < 2)
        {
            throw new InvalidDataException($"The content item {node.NodeId} has an invalid path: {node.Path} with parentID: {node.ParentId}");
        }

        if (node.ParentId != default && pathParts[^2] != node.ParentId.ToInvariantString())
        {
            throw new InvalidDataException($"The content item {node.NodeId} has an invalid path: {node.Path} with parentID: {node.ParentId}");
        }
    }
}
