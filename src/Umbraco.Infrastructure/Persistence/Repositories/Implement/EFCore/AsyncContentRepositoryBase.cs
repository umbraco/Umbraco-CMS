using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

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
    private readonly IIdKeyMap _idKeyMap;
    private readonly ITagRepository _tagRepository;
    private readonly IJsonSerializer _jsonSerializer;

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
    public abstract Task<IEnumerable<TEntity>> GetAllVersionsAsync(Guid nodeKey, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllVersionsSlimAsync(Guid nodeKey, int skip, int take, CancellationToken cancellationToken)
    {
        IEnumerable<TEntity> allVersions = await GetAllVersionsAsync(nodeKey, cancellationToken);
        return allVersions.Skip(skip).Take(take);
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
    public abstract Task<TEntity?> GetVersionAsync(int versionId, CancellationToken cancellationToken);

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
    // above) so AsyncPublishableContentRepositoryBase.CountPublishedAsync can reuse this tail on top
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
        var allPropertyData = new List<PropertyDataDto>();
        foreach (IEnumerable<int> batch in versionIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            var batchIds = batch.ToList();

            if (propertyAliases is { Length: > 0 })
            {
                List<string> aliases = new(propertyAliases);
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
    internal static IOrderedQueryable<TRow> ApplyVariantNameOrdering<TRow>(
        IQueryable<TRow> source,
        Expression<Func<TRow, string?>> variantNameSelector,
        Expression<Func<TRow, int>> nodeIdSelector,
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
    public abstract Task<PagedModel<TEntity>> GetChildrenAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetDescendantsAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true);

    /// <inheritdoc />
    public abstract Task<IEnumerable<TEntity>> GetRootContentAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<IEnumerable<TEntity>> GetRecycleBinAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task<PagedModel<TEntity>> GetPagedRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

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
