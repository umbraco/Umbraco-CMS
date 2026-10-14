using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
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
///     Provides a base class for async repositories managing <see cref="IPublishableContentBase" /> entities,
///     adding publishing and scheduling operations on top of <see cref="AsyncContentRepositoryBase{TEntity,TRepository}" />.
/// </summary>
/// <typeparam name="TEntity">The publishable content entity type.</typeparam>
/// <typeparam name="TRepository">The concrete repository type (self-referential, used for cache policy resolution).</typeparam>
/// <typeparam name="TEntityDto">The EF Core DTO type for the primary entity table.</typeparam>
/// <typeparam name="TContentVersionDto">The EF Core DTO type for content version rows.</typeparam>
/// <typeparam name="TContentCultureVariationDto">
///     The EF Core DTO type for culture variation rows; must implement <see cref="ICultureVariationDto" /> and
///     have a parameterless constructor.
/// </typeparam>
internal abstract class AsyncPublishableContentRepositoryBase<TEntity, TRepository, TEntityDto, TContentVersionDto, TContentCultureVariationDto>
    : AsyncContentRepositoryBase<TEntity, TRepository>, IAsyncPublishableContentRepository<TEntity>
    where TEntity : class, IPublishableContentBase
    where TRepository : class, IRepository
    where TEntityDto : class, IPublishableContentDto
    where TContentVersionDto : class, IContentVersionDto
    where TContentCultureVariationDto : class, ICultureVariationDto, new()
{
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="AsyncPublishableContentRepositoryBase{TEntity,TRepository,TEntityDto,TContentVersionDto,TContentCultureVariationDto}" />
    ///     class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="loggerFactory">
    ///     The logger factory used to create a logger for the correct closed generic type at runtime.
    ///     <see cref="ILoggerFactory" /> is used instead of <see cref="ILogger{T}" /> because the open generic
    ///     <typeparamref name="TEntity" /> prevents resolving the correct logger type via DI at registration time.
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
    /// <param name="tagRepository">The tag repository, used to persist tag values for tag-enabled properties on publish.</param>
    /// <param name="jsonSerializer">The JSON serializer, used to parse legacy JSON-stored tag values.</param>
    protected AsyncPublishableContentRepositoryBase(
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
        IContentTypeRepository contentTypeRepository,
        IIdKeyMap idKeyMap,
        ITagRepository tagRepository,
        IJsonSerializer jsonSerializer)
        : base(
            scopeAccessor,
            appCaches,
            loggerFactory,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditors,
            dataValueReferenceFactories,
            dataTypeService,
            eventAggregator,
            repositoryCacheVersionService,
            cacheSyncService,
            idKeyMap,
            tagRepository,
            jsonSerializer)
    {
        ContentTypeRepository = contentTypeRepository;
    }

    /// <summary>Gets the content type repository used to resolve content types by their integer ID.</summary>
    protected IContentTypeRepository ContentTypeRepository { get; }

    /// <summary>
    ///     Builds a domain entity from the rows a read query returned for one version of it and its content type.
    /// </summary>
    /// <param name="row">The materialized row: the node, entity, content and version rows, plus the published version when one exists.</param>
    /// <param name="contentType">
    ///     The content type for the entity, or <see langword="null" /> if the content type could not be resolved.
    /// </param>
    /// <returns>The constructed domain entity.</returns>
    protected abstract TEntity BuildEntity(ContentRow<TEntityDto, TContentVersionDto> row, IContentType? contentType);

    /// <summary>
    ///     Builds the entity row (<typeparamref name="TEntityDto" />) persisted for a domain entity. The node, content
    ///     and shared version rows are built by the base; this hook supplies only the kind-specific table row.
    /// </summary>
    /// <param name="entity">The domain entity being persisted.</param>
    /// <returns>The entity row ready to be saved.</returns>
    protected abstract TEntityDto BuildEntityRow(TEntity entity);

    /// <summary>
    ///     Builds the entity-specific version row (<typeparamref name="TContentVersionDto" />) for the entity's current
    ///     version, with <see cref="IContentVersionDto.Published" /> set to <paramref name="publishing" />.
    /// </summary>
    /// <param name="entity">The domain entity whose current version is being persisted.</param>
    /// <param name="publishing">Whether that version is being published as part of the same save.</param>
    /// <returns>The version row ready to be saved.</returns>
    protected abstract TContentVersionDto BuildEntityVersionRow(TEntity entity, bool publishing);

    /// <summary>
    ///     Writes every column of the entity-specific version row with the given id, from a row built by
    ///     <see cref="BuildEntityVersionRow" />. Issued as an update statement rather than a tracked save because the
    ///     same context may still track the instance the row was inserted from.
    /// </summary>
    /// <param name="db">The database context of the ambient scope.</param>
    /// <param name="entityVersion">The row carrying the id and the values to write.</param>
    protected abstract Task UpdateEntityVersionRowAsync(UmbracoDbContext db, TContentVersionDto entityVersion);

    private ContentWriteRow<TEntityDto, TContentVersionDto> BuildWriteRow(TEntity entity, bool publishing) =>
        new(
            ContentBaseFactory.BuildEFCoreNodeDto(entity, NodeObjectTypeKey),
            ContentBaseFactory.BuildEFCoreContentDto(entity),
            ContentBaseFactory.BuildEFCoreContentVersionDto(entity, publishing),
            BuildEntityVersionRow(entity, publishing),
            BuildEntityRow(entity));

    /// <summary>
    ///     Builds the join every read query of this repository starts from, over nodes and content versions the
    ///     caller has already filtered: the entity, content and entity-specific version rows joined on the node
    ///     and version ids, and the LEFT JOINed content type, published version and parent node.
    /// </summary>
    /// <remarks>
    ///     Implementations keep the joins concrete to their own tables; everything downstream of the row is
    ///     shared. Project with object-initializer syntax only, as
    ///     <see cref="ContentJoinRow{TEntityDto,TContentVersionDto}" /> explains.
    /// </remarks>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="nodes">The nodes to read, already filtered by the caller.</param>
    /// <param name="contentVersions">The content versions to join each node with, already filtered by the caller.</param>
    /// <returns>One row per node and version pair.</returns>
    protected abstract IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> BuildBaseQuery(
        UmbracoDbContext db,
        IQueryable<NodeDto> nodes,
        IQueryable<ContentVersionDto> contentVersions);

    /// <summary>
    ///     Builds the join every read query of this repository starts from, joining each node with its current version.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="nodes">The nodes to read, already filtered by the caller.</param>
    /// <returns>One row per node.</returns>
    protected IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> BuildBaseQuery(UmbracoDbContext db, IQueryable<NodeDto> nodes) =>
        BuildBaseQuery(db, nodes, db.ContentVersions.Where(contentVersion => contentVersion.Current));

    /// <summary>
    ///     Gets the single shared projection from the join shape into the row entity assembly consumes; every read
    ///     query ends with <c>.Select(ToContentRow)</c> instead of restating this construction.
    /// </summary>
    protected static Expression<Func<ContentJoinRow<TEntityDto, TContentVersionDto>, ContentRow<TEntityDto, TContentVersionDto>>> ToContentRow { get; } =
        joined => new ContentRow<TEntityDto, TContentVersionDto>(
            joined.Node,
            joined.Entity,
            joined.Content,
            joined.ContentVersion,
            joined.EntityVersion,
            joined.PublishedContentVersion,
            joined.PublishedEntityVersion,
            joined.ParentNode == null ? (Guid?)null : joined.ParentNode.UniqueId);

    /// <summary>
    ///     Applies paging/ordering to a query, breaking ties on node id for stable paging.
    /// </summary>
    /// <remarks>
    ///     Internal (not private) so the ordering unit tests can exercise the tiebreak logic
    ///     directly, decoupling it from a real database's incidental row order.
    /// </remarks>
    internal static IOrderedQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> ApplyOrdering(
        IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> source,
        IQueryable<UserDto> users,
        Ordering? ordering)
    {
        bool descending = ordering is { Direction: Direction.Descending };
        // No ordering at all means sort order - the default every caller relies on - which is distinct from
        // asking for a field this repository cannot order by.
        string orderBy = ordering?.OrderBy is { Length: > 0 } field ? field.ToLowerInvariant() : "sortorder";
        IOrderedQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> ordered = orderBy switch
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
            // The entity's own published flag, not the current version's - a draft version of a
            // published entity carries Published = false on its version row.
            "published" => descending
                ? source.OrderByDescending(joined => joined.Entity.Published)
                : source.OrderBy(joined => joined.Entity.Published),
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
    private static List<ContentRow<TEntityDto, TContentVersionDto>> ReorderRowsByNodeIds(List<ContentRow<TEntityDto, TContentVersionDto>> rows, List<int> orderedNodeIds)
    {
        Dictionary<int, ContentRow<TEntityDto, TContentVersionDto>> rowsByNodeId = rows.ToDictionary(row => row.Node.NodeId);
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
    protected static async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCustomFieldOrderedPageAsync(
        UmbracoDbContext db,
        List<int> candidateNodeIds,
        Ordering ordering,
        int skip,
        int take,
        Func<List<int>, Task<List<ContentRow<TEntityDto, TContentVersionDto>>>> fetchRowsForPageNodeIds,
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
        var unorderedRows = new List<ContentRow<TEntityDto, TContentVersionDto>>();
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
    ///     are LEFT JOINed filtered to the resolved language, so variantName = COALESCE(cultureVariation.Name, node.Text),
    ///     mirroring NPoco's VariantNameSqlExpression. The whole
    ///     <see cref="ContentJoinRow{TEntityDto,TContentVersionDto}" /> is carried forward as one nested member
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
    protected static async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCultureNameOrderedAsync(
        UmbracoDbContext db,
        IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> baseQuery,
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

    /// <summary>
    ///     Deletes the versions of a node that predate <paramref name="versionDate" />, sparing the published one.
    /// </summary>
    /// <remarks>
    ///     A published version stops being the current version as soon as a draft is saved on top of it, so the
    ///     current-version exclusion alone would take published content offline.
    /// </remarks>
    public override async Task DeleteVersionsAsync(Guid nodeKey, DateTime versionDate, CancellationToken cancellationToken)
    {
        IEnumerable<int> versionIds = await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            return await db.ContentVersions
                .Join(db.Nodes, version => version.NodeId, node => node.NodeId, (version, node) => new { version, node })
                .Join(db.Set<TContentVersionDto>(), joined => joined.version.Id, typedVersion => typedVersion.Id, (joined, typedVersion) => new { joined.version, joined.node, typedVersion })
                .Where(joined => joined.node.UniqueId == nodeKey
                    && !joined.version.Current
                    && !joined.typedVersion.Published
                    && joined.version.VersionDate < versionDate)
                .Select(joined => joined.version.Id)
                .ToListAsync(cancellationToken);
        });

        foreach (int versionId in versionIds)
        {
            await PerformDeleteVersionAsync(versionId, cancellationToken);
        }
    }

    /// <inheritdoc />
    protected override async Task PerformDeleteVersionAsync(int versionId, CancellationToken cancellationToken) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var version = await db.ContentVersions
                .Where(contentVersion => contentVersion.Id == versionId)
                .Join(db.Set<TContentVersionDto>(), contentVersion => contentVersion.Id, typedVersion => typedVersion.Id, (contentVersion, typedVersion) => new { contentVersion.Current, typedVersion.Published })
                .SingleOrDefaultAsync(cancellationToken);

            if (version is null)
            {
                return true;
            }

            if (version.Current)
            {
                throw new InvalidOperationException("Cannot delete the current version.");
            }

            if (version.Published)
            {
                throw new InvalidOperationException("Cannot delete the published version.");
            }

            await db.PropertyData
                .Where(propertyData => propertyData.VersionId == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            await db.ContentVersionCultureVariations
                .Where(cultureVariation => cultureVariation.VersionId == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            await db.Set<TContentVersionDto>()
                .Where(entityVersion => entityVersion.Id == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            await db.ContentVersions
                .Where(contentVersion => contentVersion.Id == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            return true;
        });

    /// <summary>
    ///     Deletes every row across every table that references a publishable content node, in FK-safe
    ///     (child-to-parent) order, before deleting the <c>umbracoNode</c> row itself. Entity-specific tables
    ///     (e.g. Document's redirect URLs, domains) are deleted first via
    ///     <see cref="PersistEntitySpecificDeleteClausesAsync" />, then the tables shared across all publishable
    ///     content kinds.
    /// </summary>
    protected override async Task PersistDeletedItemAsync(TEntity entity)
    {
        // Raise event first else potential FK issues — mirrors PublishableContentRepositoryBase.PersistDeletedItem.
        EventAggregator.Publish(new ScopedEntityRemoveNotification(entity, new EventMessages()));

        var nodeId = entity.Id;

        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            await PersistEntitySpecificDeleteClausesAsync(db, nodeId);

            await db.ContentSchedules.Where(contentSchedule => contentSchedule.NodeId == nodeId).ExecuteDeleteAsync();
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
    ///     Deletes the rows in tables specific to this publishable content kind (e.g. Document's redirect URLs,
    ///     domains, access rules) that reference the node being deleted. Called by
    ///     <see cref="PersistDeletedItemAsync" /> before the tables shared across all publishable content kinds.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="nodeId">The node identifier of the content being deleted.</param>
    protected abstract Task PersistEntitySpecificDeleteClausesAsync(UmbracoDbContext db, int nodeId);

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(TEntity item) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            item.AddingEntity();
            var publishing = item is { PublishedState: PublishedState.Publishing };

            AssignDefaultTemplateIfMissing(item);

            await SanitizeNamesAsync(db, item, publishing);

            item.SanitizeEntityPropertiesForXmlStorage();

            ContentWriteRow<TEntityDto, TContentVersionDto> row = BuildWriteRow(item, publishing);

            await PersistNewNodeAsync(db, item, row.Node);
            await PersistNewContentAsync(db, item, row.Content);
            await PersistNewVersionsAsync(db, item, row.ContentVersion, row.EntityVersion);

            (bool edited, HashSet<string>? editedCultures) = await PersistNewPropertyDataAsync(db, item);

            // if !publishing, we may have a new name != current publish name, also impacts 'edited'
            if (!publishing && item.PublishName != item.Name)
            {
                edited = true;
            }

            // at that point, when publishing, the entity still has its old Published value
            // so we need to explicitly update the row to persist the correct value
            if (publishing)
            {
                row.Entity.Published = true;
            }

            row.Entity.NodeId = item.Id;
            item.Edited = row.Entity.Edited = !row.Entity.Published || edited; // if not published, always edited
            db.Set<TEntityDto>().Add(row.Entity);
            await db.SaveChangesAsync();

            if (item.ContentType.VariesByCulture())
            {
                (List<ContentVersionCultureVariationDto> contentVariations, List<TContentCultureVariationDto> entityVariations, editedCultures, _) =
                    await ResolveCultureVariationChangesAsync(item, publishing, isNew: true, editedCultures, item.UpdateDate);

                if (contentVariations.Count > 0)
                {
                    db.ContentVersionCultureVariations.AddRange(contentVariations);
                }

                if (entityVariations.Count > 0)
                {
                    db.Set<TContentCultureVariationDto>().AddRange(entityVariations);
                }

                await db.SaveChangesAsync();
            }

            await OnUowRefreshedEntityAsync(item, CancellationToken.None);

            await ApplyPostPublishFlagFlipsAsync(item);

            item.ResetDirtyProperties();

            return true;
        });

    /// <inheritdoc />
    protected override Task PersistUpdatedItemAsync(TEntity item) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var isEntityDirty = item.IsDirty();
            var editedSnapshot = item.Edited;

            if (item is { PublishedState: PublishedState.Published or PublishedState.Unpublished }
                && !isEntityDirty && !item.IsAnyUserPropertyDirty())
            {
                // no change to save, do nothing, don't even update dates
                return true;
            }

            // whatever we do, we must check that we are saving the current version
            ContentVersionDto? version = await db.ContentVersions.FirstOrDefaultAsync(contentVersion => contentVersion.Id == item.VersionId);
            if (version is null || !version.Current)
            {
                throw new InvalidOperationException("Cannot save a non-current version.");
            }

            item.UpdatingEntity();

            // Check if this entity is being moved as a descendant as part of a bulk moving operation.
            // When moving, only Path + Level + UpdateDate are dirty, so we can skip version creation,
            // property-data reconciliation, culture-variation reconciliation and tag updates entirely —
            // we cannot roll a bulk move back anyway.
            var isMoving = item.IsMoving();

            var publishing = item is { PublishedState: PublishedState.Publishing };

            if (!isMoving)
            {
                if (publishing && item.PublishedVersionId > 0)
                {
                    // The published version is not published anymore — a new one is about to take its place.
                    await db.Set<TContentVersionDto>().Where(entityVersion => entityVersion.Id == item.PublishedVersionId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(entityVersion => entityVersion.Published, false));
                }

                await SanitizeNamesAsync(db, item, publishing);
                item.SanitizeEntityPropertiesForXmlStorage();

                if (item.IsPropertyDirty(nameof(item.ParentId)))
                {
                    NodeDto parent = await GetParentNodeDtoAsync(db, item.ParentId);
                    item.Path = string.Concat(parent.Path, ",", item.Id);
                    item.Level = parent.Level + 1;
                    item.SortOrder = await GetNewChildSortOrderAsync(db, item.ParentId, 0);
                    item.ParentKey = ResolveParentKey(item.ParentId, parent);
                }
            }

            ContentWriteRow<TEntityDto, TContentVersionDto> row = BuildWriteRow(item, publishing);

            NodeDto nodeDto = row.Node;
            ValidatePath(nodeDto);
            await db.Nodes.Where(node => node.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(node => node.Text, nodeDto.Text)
                .SetProperty(node => node.ParentId, nodeDto.ParentId)
                .SetProperty(node => node.Level, nodeDto.Level)
                .SetProperty(node => node.Path, nodeDto.Path)
                .SetProperty(node => node.SortOrder, nodeDto.SortOrder)
                .SetProperty(node => node.Trashed, nodeDto.Trashed)
                .SetProperty(node => node.UserId, nodeDto.UserId));

            if (isMoving)
            {
                // Skip version/property/culture-variation/tag handling entirely, and skip the post-publish
                // flag flips below (a move never changes PublishedState) — mirrors NPoco, which wraps its
                // equivalent blocks (including both SetEntityTags/ClearEntityTags call sites) in the same
                // isMoving guard.
                await OnUowRefreshedEntityAsync(item, CancellationToken.None);
                item.ResetDirtyProperties();
                IsolatedCache.Clear(EntityTypeCacheKeyPrefix + item.Key);
                return true;
            }

            await db.Content.Where(content => content.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(content => content.ContentTypeId, row.Content.ContentTypeId));

            ContentVersionDto contentVersionDto = row.ContentVersion;
            TContentVersionDto entityVersionDto = row.EntityVersion;

            // Preserve the existing flag — `version` is the ContentVersionDto fetched above for the
            // current-version assertion.
            contentVersionDto.PreventCleanup = version.PreventCleanup;

            await db.ContentVersions.Where(contentVersion => contentVersion.Id == item.VersionId).ExecuteUpdateAsync(setters => setters
                .SetProperty(contentVersion => contentVersion.VersionDate, contentVersionDto.VersionDate)
                .SetProperty(contentVersion => contentVersion.UserId, contentVersionDto.UserId)
                .SetProperty(contentVersion => contentVersion.Current, contentVersionDto.Current)
                .SetProperty(contentVersion => contentVersion.Text, contentVersionDto.Text)
                .SetProperty(contentVersion => contentVersion.PreventCleanup, contentVersionDto.PreventCleanup));

            await UpdateEntityVersionRowAsync(db, entityVersionDto);

            if (publishing)
            {
                // The row just flipped above (now Current=false, Published=true) becomes the published
                // version; a new draft pair is inserted to take over as the current version.
                item.PublishedVersionId = item.VersionId;

                var newContentVersionDto = new ContentVersionDto
                {
                    NodeId = item.Id,
                    VersionDate = contentVersionDto.VersionDate,
                    UserId = contentVersionDto.UserId,
                    Current = true,
                    Text = item.Name,
                    PreventCleanup = false, // new draft version disregards the existing prevent-cleanup flag
                };
                db.ContentVersions.Add(newContentVersionDto);
                await db.SaveChangesAsync();
                item.VersionId = newContentVersionDto.Id;

                TContentVersionDto newEntityVersionDto = BuildEntityVersionRow(item, publishing: false);
                db.Set<TContentVersionDto>().Add(newEntityVersionDto);
                await db.SaveChangesAsync();
            }

            var versionToDelete = publishing ? item.PublishedVersionId : item.VersionId;

            (bool edited, HashSet<string>? editedCultures) = await PersistUpdatedPropertyDataAsync(
                db, item, versionToDelete, publishing ? item.PublishedVersionId : 0);

            // if !publishing, we may have a new name != current publish name, also impacts 'edited'
            if (!publishing && item.PublishName != item.Name)
            {
                edited = true;
            }

            if (!publishing && editedSnapshot)
            {
                edited = true;
            }

            if (item.ContentType.VariesByCulture())
            {
                (List<ContentVersionCultureVariationDto> contentVariations, List<TContentCultureVariationDto> entityVariations, editedCultures, bool cultureEdited) =
                    await ResolveCultureVariationChangesAsync(item, publishing, isNew: false, editedCultures, contentVersionDto.VersionDate);

                if (cultureEdited)
                {
                    edited = true;
                }

                // Replace (rather than update) the content version variations — only for versionToDelete,
                // and the entity variations — for the whole node, unconditionally. These two tables are
                // deleted and reinserted rather than diff-reconciled, unlike PropertyData.
                await db.ContentVersionCultureVariations.Where(variation => variation.VersionId == versionToDelete).ExecuteDeleteAsync();
                await db.Set<TContentCultureVariationDto>().Where(variation => variation.NodeId == item.Id).ExecuteDeleteAsync();

                if (contentVariations.Count > 0)
                {
                    db.ContentVersionCultureVariations.AddRange(contentVariations);
                }

                if (entityVariations.Count > 0)
                {
                    db.Set<TContentCultureVariationDto>().AddRange(entityVariations);
                }

                await db.SaveChangesAsync();
            }

            if (item is { PublishedState: PublishedState.Publishing })
            {
                row.Entity.Published = true;
            }
            else if (item is { PublishedState: PublishedState.Unpublishing })
            {
                row.Entity.Published = false;
            }

            item.Edited = row.Entity.Edited = !row.Entity.Published || edited; // if not published, always edited

            await db.Set<TEntityDto>().Where(entityDto => entityDto.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(entityDto => entityDto.Published, row.Entity.Published)
                .SetProperty(entityDto => entityDto.Edited, row.Entity.Edited));

            // If entity is publishing, update tags; else leave tags there. This means that implicitly
            // unpublished, or trashed, entities *still* have tags in the database. Mirrors NPoco's
            // PublishableContentRepositoryBase.PersistUpdatedItem, which calls SetEntityTags here (before
            // the refresh trigger) and again below (after the publish-state flip) — see
            // ApplyPostPublishFlagFlipsAsync for the second call.
            if (publishing)
            {
                await SetEntityTagsAsync(item);
            }

            await OnUowRefreshedEntityAsync(item, CancellationToken.None);

            await ApplyPostPublishFlagFlipsAsync(item);

            item.ResetDirtyProperties();

            // We need to flush the isolated cache by key explicitly here. The ContentCacheRefresher does
            // the same thing, but by the time it's invoked, custom notification handlers might have
            // already consumed the cached version.
            IsolatedCache.Clear(EntityTypeCacheKeyPrefix + item.Key);

            return true;
        });

    // Inserts (or converts a reserved placeholder into) the umbracoNode row, resolving Path/Level/SortOrder
    // from the parent, then patches the Path once the real NodeId is known.
    private async Task PersistNewNodeAsync(UmbracoDbContext db, TEntity item, NodeDto nodeDto)
    {
        NodeDto parent = await GetParentNodeDtoAsync(db, item.ParentId);
        var level = parent.Level + 1;

        var calculateSortOrder = (item is { HasIdentity: false, SortOrder: 0 } && item.IsPropertyDirty(nameof(item.SortOrder)) is false)
                                  || await SortorderExistsAsync(db, item.ParentId, item.SortOrder);
        var sortOrder = calculateSortOrder ? await GetNewChildSortOrderAsync(db, item.ParentId, 0) : item.SortOrder;

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

    private static async Task PersistNewContentAsync(UmbracoDbContext db, TEntity item, ContentDto contentDto)
    {
        contentDto.NodeId = item.Id;
        db.Content.Add(contentDto);
        await db.SaveChangesAsync();
    }

    private async Task PersistNewVersionsAsync(UmbracoDbContext db, TEntity item, ContentVersionDto contentVersionDto, TContentVersionDto entityVersionDto)
    {
        contentVersionDto.NodeId = item.Id;
        db.ContentVersions.Add(contentVersionDto);
        await db.SaveChangesAsync();
        item.VersionId = contentVersionDto.Id;

        entityVersionDto.Id = item.VersionId;
        db.Set<TContentVersionDto>().Add(entityVersionDto);
        await db.SaveChangesAsync();

        if (item is { PublishedState: PublishedState.Publishing })
        {
            // The pair just inserted (Current=false, Published=true) becomes the published version.
            // A second (Current=true, Published=false) pair is inserted for the new draft.
            item.PublishedVersionId = item.VersionId;

            var newContentVersionDto = new ContentVersionDto
            {
                NodeId = item.Id,
                VersionDate = contentVersionDto.VersionDate,
                UserId = contentVersionDto.UserId,
                Current = true,
                Text = item.Name,
            };
            db.ContentVersions.Add(newContentVersionDto);
            await db.SaveChangesAsync();
            item.VersionId = newContentVersionDto.Id;

            TContentVersionDto newEntityVersionDto = BuildEntityVersionRow(item, publishing: false);
            db.Set<TContentVersionDto>().Add(newEntityVersionDto);
            await db.SaveChangesAsync();
        }
    }

    private async Task<(bool Edited, HashSet<string>? EditedCultures)> PersistNewPropertyDataAsync(UmbracoDbContext db, TEntity item)
    {
        (List<PropertyDataDto> propertyDataDtos, var edited, HashSet<string>? editedCultures) = await PropertyFactory.BuildEFCoreDtosAsync(
            item.ContentType.Variations,
            item.VersionId,
            item.PublishedVersionId,
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

    // Diff-reconciles PropertyData rows for the given version against the freshly built values for the
    // entity's current properties: matches existing rows by (PropertyTypeId, VersionId, LanguageId,
    // Segment), updates matches in place, inserts new rows for unmatched values, and deletes rows that no
    // longer have a corresponding value. The fetch takes no row lock: every caller holds the ContentTree
    // write lock, and the scope shares one connection and transaction, so that lock already serializes this
    // read-then-write sequence.
    private async Task<(bool Edited, HashSet<string>? EditedCultures)> PersistUpdatedPropertyDataAsync(
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

    // Shared culture-variation computation for PersistNewItemAsync/PersistUpdatedItemAsync: determines which
    // cultures have an edited name, updates SetCultureEdited/AdjustDates on the entity, and builds the DTOs
    // to persist. The DB-write strategy (plain AddRange vs. delete-then-reinsert) differs between callers and
    // stays at each call site. The isNew flag mirrors a real NPoco asymmetry — PersistNewItem never feeds a
    // culture-name mismatch into the enclosing method's own 'edited' flag, PersistUpdatedItem does — so the
    // returned Edited is only meaningful when isNew is false; callers must preserve this, not merge it away.
    private async Task<(
        List<ContentVersionCultureVariationDto> ContentVariations,
        List<TContentCultureVariationDto> EntityVariations,
        HashSet<string>? EditedCultures,
        bool Edited)> ResolveCultureVariationChangesAsync(
            TEntity item, bool publishing, bool isNew, HashSet<string>? editedCultures, DateTime versionDate)
    {
        var edited = false;
        foreach (ContentCultureInfos cultureInfo in item.CultureInfos!)
        {
            if (cultureInfo.Name != item.GetPublishName(cultureInfo.Culture))
            {
                if (!isNew)
                {
                    edited = true;
                }

                (editedCultures ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase)).Add(cultureInfo.Culture);
            }
        }

        item.SetCultureEdited(editedCultures!);
        item.AdjustDates(versionDate, publishing);

        List<ContentVersionCultureVariationDto> contentVariations = await BuildContentVariationDtosAsync(item, publishing);
        List<TContentCultureVariationDto> entityVariations = await BuildEntityVariationDtosAsync(item, editedCultures!);

        return (contentVariations, entityVariations, editedCultures, edited);
    }

    // Builds ContentVersionCultureVariationDto rows for the 'current' (non-published) version, one per
    // culture with a name, plus — only when publishing — additional rows for the 'published' version's
    // published cultures. Mirrors PublishableContentRepositoryBase.GetContentVariationDtos, but awaits
    // LanguageRepository properly instead of that method's sync-over-async GetAwaiter().GetResult() calls,
    // since this helper is genuinely async (no non-async NPoco caller to match).
    private async Task<List<ContentVersionCultureVariationDto>> BuildContentVariationDtosAsync(TEntity entity, bool publishing)
    {
        var dtos = new List<ContentVersionCultureVariationDto>();

        if (entity.CultureInfos is not null)
        {
            foreach (ContentCultureInfos cultureInfo in entity.CultureInfos)
            {
                int languageId = await LanguageRepository.GetIdByIsoCodeAsync(cultureInfo.Culture)
                    ?? throw new InvalidOperationException("Not a valid culture.");

                dtos.Add(new ContentVersionCultureVariationDto
                {
                    VersionId = entity.VersionId,
                    LanguageId = languageId,
                    Name = cultureInfo.Name ?? throw new InvalidOperationException("A content culture must have a name."),
                    UpdateDate = entity.GetUpdateDate(cultureInfo.Culture) ?? DateTime.MinValue,
                });
            }
        }

        // if not publishing, we're just updating the 'current' (non-published) version, so there are no
        // DTOs to create for the 'published' version which remains unchanged.
        if (!publishing)
        {
            return dtos;
        }

        if (entity.PublishCultureInfos is not null)
        {
            foreach (ContentCultureInfos cultureInfo in entity.PublishCultureInfos)
            {
                int languageId = await LanguageRepository.GetIdByIsoCodeAsync(cultureInfo.Culture)
                    ?? throw new InvalidOperationException("Not a valid culture.");

                dtos.Add(new ContentVersionCultureVariationDto
                {
                    VersionId = entity.PublishedVersionId,
                    LanguageId = languageId,
                    Name = cultureInfo.Name ?? throw new InvalidOperationException("A content culture must have a name."),
                    UpdateDate = entity.GetPublishDate(cultureInfo.Culture) ?? DateTime.MinValue,
                });
            }
        }

        return dtos;
    }

    // Builds one entity culture-variation row per culture the entity is available or published in. Mirrors
    // PublishableContentRepositoryBase.GetEntityVariationDtos field-by-field — the EF Core DTO carries only
    // LanguageId (no in-memory-only Culture convenience field like the NPoco DTO), so Culture is not set here.
    private async Task<List<TContentCultureVariationDto>> BuildEntityVariationDtosAsync(TEntity entity, HashSet<string>? editedCultures)
    {
        var dtos = new List<TContentCultureVariationDto>();

        IEnumerable<string> allCultures = entity.AvailableCultures.Union(entity.PublishedCultures); // union = distinct
        foreach (string culture in allCultures)
        {
            int languageId = await LanguageRepository.GetIdByIsoCodeAsync(culture)
                ?? throw new InvalidOperationException("Not a valid culture.");

            dtos.Add(new TContentCultureVariationDto
            {
                NodeId = entity.Id,
                LanguageId = languageId,
                Name = entity.GetCultureName(culture) ?? entity.GetPublishName(culture),
                Available = entity.IsCultureAvailable(culture),
                Published = entity.IsCulturePublished(culture),
                // note: can't use IsCultureEdited at that point - hasn't been updated yet - see PersistUpdatedItem
                Edited = entity.IsCultureAvailable(culture) &&
                         (!entity.IsCulturePublished(culture) || (editedCultures != null && editedCultures.Contains(culture))),
            });
        }

        return dtos;
    }

    // Flips the entity's in-memory published state to match what was just persisted — shared by
    // PersistNewItemAsync and PersistUpdatedItemAsync. Templates are a capability of the entity, not of the
    // repository kind, so they follow the ITemplatedContent cast exactly as in the NPoco base.
    private async Task ApplyPostPublishFlagFlipsAsync(TEntity item)
    {
        if (item is { PublishedState: PublishedState.Publishing })
        {
            item.Published = true;
            if (item is ITemplatedContent templatedContent)
            {
                templatedContent.PublishTemplateId = templatedContent.TemplateId;
            }

            item.PublisherId = item.WriterId;
            item.PublishName = item.Name;
            item.PublishDate = item.UpdateDate;

            await SetEntityTagsAsync(item);
        }
        else if (item is { PublishedState: PublishedState.Unpublishing })
        {
            item.Published = false;
            if (item is ITemplatedContent templatedContent)
            {
                templatedContent.PublishTemplateId = null;
            }

            item.PublisherId = null;
            item.PublishName = null;
            item.PublishDate = null;

            ClearEntityTags(item);
        }
    }

    private static void AssignDefaultTemplateIfMissing(TEntity entity)
    {
        if (entity is ITemplatedContent { TemplateId: null } templatedContent)
        {
            templatedContent.TemplateId = entity.ContentType.DefaultTemplate?.Id;
        }
    }

    /// <inheritdoc />
    public virtual Task<ContentScheduleCollection> GetContentScheduleAsync(Guid contentKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int nodeId = await ResolveNodeIdAsync(db, contentKey, cancellationToken);
            var result = new ContentScheduleCollection();
            if (nodeId == 0)
            {
                return result;
            }

            List<ContentScheduleDto> rows = await db.ContentSchedules
                .Where(contentSchedule => contentSchedule.NodeId == nodeId)
                .ToListAsync(cancellationToken);

            foreach (ContentScheduleDto row in rows)
            {
                result.Add(await ToContentScheduleAsync(row));
            }

            return result;
        });

    /// <inheritdoc />
    public virtual Task PersistContentScheduleAsync(IPublishableContentBase content, ContentScheduleCollection schedule, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<object>(async db =>
        {
            // Tracked (overriding the context's global NoTracking default) so entries can be mutated
            // in place and batched into a single SaveChangesAsync, matching the established pattern in
            // PersistUpdatedPropertyDataAsync.
            Dictionary<Guid, ContentScheduleDto> existing = await db.ContentSchedules
                .AsTracking()
                .Where(contentSchedule => contentSchedule.NodeId == content.Id)
                .ToDictionaryAsync(contentSchedule => contentSchedule.Id, cancellationToken);

            var keepIds = new HashSet<Guid>();

            foreach (ContentSchedule model in schedule.FullSchedule)
            {
                int? languageId = await LanguageRepository.GetIdByIsoCodeAsync(model.Culture, false);

                if (model.Id != Guid.Empty && existing.TryGetValue(model.Id, out ContentScheduleDto? dto))
                {
                    dto.Date = model.Date;
                    dto.Action = model.Action.ToString();
                    dto.LanguageId = languageId;
                }
                else
                {
                    model.Id = Guid.NewGuid();
                    db.ContentSchedules.Add(new ContentScheduleDto
                    {
                        Id = model.Id,
                        NodeId = content.Id,
                        LanguageId = languageId,
                        Date = model.Date,
                        Action = model.Action.ToString(),
                    });
                }

                keepIds.Add(model.Id);
            }

            foreach (KeyValuePair<Guid, ContentScheduleDto> entry in existing)
            {
                if (!keepIds.Contains(entry.Key))
                {
                    db.ContentSchedules.Remove(entry.Value);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task ClearScheduleAsync(DateTime date, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<object>(async db =>
        {
            await db.ContentSchedules
                .Where(contentSchedule => contentSchedule.Date <= date)
                .Where(IsForThisObjectType(db))
                .ExecuteDeleteAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task ClearScheduleAsync(DateTime date, ContentScheduleAction action, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<object>(async db =>
        {
            string actionString = action.ToString();
            await db.ContentSchedules
                .Where(contentSchedule => contentSchedule.Date <= date && contentSchedule.Action == actionString)
                .Where(IsForThisObjectType(db))
                .ExecuteDeleteAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task<bool> HasContentForExpirationAsync(DateTime date, CancellationToken cancellationToken) =>
        HasScheduledContentAsync(ContentScheduleAction.Expire, date, cancellationToken);

    /// <inheritdoc />
    public virtual Task<bool> HasContentForReleaseAsync(DateTime date, CancellationToken cancellationToken) =>
        HasScheduledContentAsync(ContentScheduleAction.Release, date, cancellationToken);

    private Task<bool> HasScheduledContentAsync(ContentScheduleAction action, DateTime date, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(db =>
        {
            string actionString = action.ToString();
            return db.ContentSchedules
                .Where(contentSchedule => contentSchedule.Action == actionString && contentSchedule.Date <= date)
                .AnyAsync(IsForThisObjectType(db), cancellationToken);
        });

    // Shared by ClearScheduleAsync (both overloads) and HasScheduledContentAsync - guards a
    // ContentScheduleDto against belonging to this repository's entity kind, since ContentSchedules
    // itself carries no object-type column of its own.
    private Expression<Func<ContentScheduleDto, bool>> IsForThisObjectType(UmbracoDbContext db) =>
        contentSchedule => db.Nodes.Any(node => node.NodeId == contentSchedule.NodeId && node.NodeObjectType == NodeObjectTypeKey);

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetContentForExpirationAsync(DateTime date, CancellationToken cancellationToken) =>
        GetContentForScheduleActionAsync(ContentScheduleAction.Expire, date, cancellationToken);

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> GetContentForReleaseAsync(DateTime date, CancellationToken cancellationToken) =>
        GetContentForScheduleActionAsync(ContentScheduleAction.Release, date, cancellationToken);

    private Task<IEnumerable<TEntity>> GetContentForScheduleActionAsync(ContentScheduleAction action, DateTime date, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            string actionString = action.ToString();
            List<Guid> keys = await db.ContentSchedules
                .Where(contentSchedule => contentSchedule.Action == actionString && contentSchedule.Date <= date)
                .Join(db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey), contentSchedule => contentSchedule.NodeId, node => node.NodeId, (contentSchedule, node) => node.UniqueId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (keys.Count == 0)
            {
                return Enumerable.Empty<TEntity>();
            }

            // PerformGetManyAsync is the same abstract hydration hook the public GetManyAsync uses
            // (declared on AsyncEntityRepositoryBase), reused directly here rather than duplicating the
            // concrete repository's multi-join entity-assembly logic. Batched since a site can have
            // arbitrarily many items due at once, and the hook's underlying query builds one SQL
            // parameter per key.
            var entities = new List<TEntity>();
            foreach (IEnumerable<Guid> batch in keys.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                IEnumerable<TEntity>? batchEntities = await PerformGetManyAsync([.. batch]);
                if (batchEntities is not null)
                {
                    entities.AddRange(batchEntities);
                }
            }

            // Scheduled publishing walks this sequence in order and needs a parent handled before its children.
            // Each batch above is hydrated and returned on its own, so the ordering is applied once, here.
            return entities
                .OrderBy(entity => entity.Level)
                .ThenBy(entity => entity.SortOrder);
        });

    /// <inheritdoc />
    public virtual Task<int> CountPublishedAsync(string? contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<NodeDto> query = PublishedNodes(db)
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && !node.Trashed);

            if (!string.IsNullOrWhiteSpace(contentTypeAlias))
            {
                query = FilterByContentTypeAlias(query, db, contentTypeAlias);
            }

            return await query.CountAsync(cancellationToken);
        });

    /// <summary>
    ///     Nodes of this repository's entity kind whose table-level "published" flag (<see cref="IPublishableContentDto.Published" />) is set.
    /// </summary>
    /// <remarks>
    ///     Callers still need to filter by <c>NodeObjectTypeKey</c>/<c>Trashed</c> themselves —
    ///     this only joins on the published flag. <c>protected</c> so <see cref="IsPathPublishedAsync" />
    ///     overrides in concrete repositories can reuse it.
    /// </remarks>
    protected static IQueryable<NodeDto> PublishedNodes(UmbracoDbContext db) =>
        db.Nodes.Join(db.Set<TEntityDto>().Where(entityDto => entityDto.Published), node => node.NodeId, entityDto => entityDto.NodeId, (node, entityDto) => node);

    /// <inheritdoc />
    public abstract Task<bool> IsPathPublishedAsync(TEntity? content, CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual Task<IDictionary<Guid, IEnumerable<ContentSchedule>>> GetContentSchedulesByKeysAsync(Guid[] contentKeys, CancellationToken cancellationToken)
    {
        if (contentKeys.Length == 0)
        {
            return Task.FromResult<IDictionary<Guid, IEnumerable<ContentSchedule>>>(new Dictionary<Guid, IEnumerable<ContentSchedule>>());
        }

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var result = new Dictionary<Guid, IEnumerable<ContentSchedule>>();

            foreach (IEnumerable<Guid> batch in contentKeys.Distinct().InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                List<Guid> batchKeys = [.. batch];

                List<(Guid Key, ContentScheduleDto Dto)> rows = await db.ContentSchedules
                    .Join(db.Nodes.Where(node => batchKeys.Contains(node.UniqueId)), contentSchedule => contentSchedule.NodeId, node => node.NodeId, (contentSchedule, node) => new { node.UniqueId, contentSchedule })
                    .Select(joined => new ValueTuple<Guid, ContentScheduleDto>(joined.UniqueId, joined.contentSchedule))
                    .ToListAsync(cancellationToken);

                foreach (IGrouping<Guid, (Guid Key, ContentScheduleDto Dto)> group in rows.GroupBy(row => row.Key))
                {
                    var schedules = new List<ContentSchedule>();
                    foreach ((Guid _, ContentScheduleDto dto) in group)
                    {
                        schedules.Add(await ToContentScheduleAsync(dto));
                    }

                    result[group.Key] = schedules;
                }
            }

            return (IDictionary<Guid, IEnumerable<ContentSchedule>>)result;
        });
    }

    /// <inheritdoc />
    public override Task<IEnumerable<TEntity>> GetAllVersionsAsync(Guid nodeKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // All versions for the node, no Current filter — mirrors NPoco GetBaseQuery(current: false).
            List<ContentRow<TEntityDto, TContentVersionDto>> rows = await BuildBaseQuery(
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
    public override Task<IEnumerable<TEntity>> GetAllVersionsSlimAsync(Guid nodeKey, int skip, int take, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<ContentRow<TEntityDto, TContentVersionDto>> rows = await BuildBaseQuery(
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
    public override Task<TEntity?> GetVersionAsync(int versionId, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // Filter by the version's own id; no Current filter — historical versions are valid targets.
            // The Nodes join guards against returning versions that belong to a different object type.
            ContentRow<TEntityDto, TContentVersionDto>? row = await BuildBaseQuery(
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
    public override Task<PagedModel<TEntity>> GetChildrenAsync(
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

            IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> baseQuery = BuildBaseQuery(
                db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentNodeId));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases, loadTemplates, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCustomFieldOrdered()
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

            Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchDefaultOrdered()
            {
                // Invariant name ordering falls through here: ApplyOrdering's default "name"
                // arm uses node.Text, equivalent to COALESCE(NULL, node.Text) in NPoco's invariant path.
                IOrderedQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering);

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
    public override Task<PagedModel<TEntity>> GetDescendantsAsync(
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

            IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> baseQuery = BuildBaseQuery(
                db,
                db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey
                    && EF.Functions.Like(node.Path, $"{pathMatch}%")
                    && (includeTrashed || node.Trashed == false)));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: loadTemplates, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCustomFieldOrdered()
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

            Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchDefaultOrdered()
            {
                // Invariant name ordering falls through here: ApplyOrdering's default "name"
                // arm uses node.Text, equivalent to COALESCE(NULL, node.Text) in NPoco's invariant path.
                IOrderedQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering);

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
    public override Task<IEnumerable<TEntity>> GetRootContentAsync(CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<IEnumerable<TEntity>>(async db =>
        {
            List<ContentRow<TEntityDto, TContentVersionDto>> rows = await BuildBaseQuery(
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
    public override Task<IEnumerable<TEntity>> GetRecycleBinAsync(CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<IEnumerable<TEntity>>(async db =>
        {
            // Every trashed node of this object type, regardless of tree depth — not just the direct
            // children of the recycle bin folder itself.
            List<ContentRow<TEntityDto, TContentVersionDto>> rows = await BuildBaseQuery(
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
    public override Task<PagedModel<TEntity>> GetPagedRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
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

            IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> baseQuery = BuildBaseQuery(
                db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCustomFieldOrdered()
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

            Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchDefaultOrdered()
            {
                IOrderedQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering);

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
            IQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> baseQuery = BuildBaseQuery(db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey))
                .Where(joined => contentTypeIdsList.Contains(joined.Content.ContentTypeId));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<TEntity> { Total = total, Items = [] };
            }

            List<TEntity> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCustomFieldOrdered()
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

            Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>>> FetchDefaultOrdered()
            {
                IOrderedQueryable<ContentJoinRow<TEntityDto, TContentVersionDto>> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering);

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
    public virtual async Task<IEnumerable<TEntity>> GetManyAsync(Guid[] keys, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken)
        => keys.Length == 0 ? [] : await PerformGetRangeAsync(keys, propertyAliases, loadTemplates, cancellationToken);

    private Task<List<TEntity>> PerformGetRangeAsync(Guid[]? keys, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<NodeDto> nodeQuery = db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey);
            if (keys is not null)
            {
                List<Guid> keyList = new(keys);
                nodeQuery = nodeQuery.Where(node => keyList.Contains(node.UniqueId));
            }

            List<ContentRow<TEntityDto, TContentVersionDto>> rows = await BuildBaseQuery(db, nodeQuery)
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
    ///     culture variations, the post-assembly hook, then a clean dirty state and the parent key.
    /// </summary>
    /// <param name="db">The EF Core context for the ambient scope.</param>
    /// <param name="rows">The rows to assemble, one per entity.</param>
    /// <param name="propertyAliases">
    ///     The property aliases to load, <see langword="null" /> for every property, or empty for none.
    /// </param>
    /// <param name="loadTemplates">Whether templates should be loaded, for entity kinds that have them.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The assembled entities, in row order.</returns>
    protected async Task<List<TEntity>> AssembleEntitiesAsync(UmbracoDbContext db, IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>> rows, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken)
    {
        int[] nodeIds = [.. rows.Select(row => row.Node.NodeId)];

        // All relevant version IDs (current + published, deduplicated).
        // Current and published are always different IDs after the first publish.
        var allVersionIds = rows
            .Select(row => row.ContentVersion.Id)
            .Concat(rows
                .Where(row => row.PublishedContentVersion is not null)
                .Select(row => row.PublishedContentVersion!.Id))
            .Distinct()
            .ToList();

        Dictionary<int, List<PropertyDataDto>> propertyDtosByVersionId =
            await LoadPropertyDataAsync(db, allVersionIds, propertyAliases, cancellationToken);

        // Pre-populate content type map. ContentTypeRepository caches, so no extra DB round-trips
        // after the first call per type — and we need the types up front to gate variation queries.
        var contentTypeMap = new Dictionary<int, IContentType?>();
        foreach (int contentTypeId in rows.Select(row => row.Content.ContentTypeId).Distinct())
        {
            contentTypeMap[contentTypeId] = await ContentTypeRepository.GetAsync(contentTypeId, cancellationToken);
        }

        (Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> contentVersionCultureVariationsByVersionId,
         Dictionary<int, IReadOnlyList<TContentCultureVariationDto>> entityCultureVariationsByNodeId,
         Dictionary<int, string?> isoCodeByLanguageId) =
            await LoadVariationsAsync(db, allVersionIds, nodeIds, contentTypeMap, cancellationToken);

        var entities = new List<TEntity>(rows.Count);

        foreach (ContentRow<TEntityDto, TContentVersionDto> row in rows)
        {
            contentTypeMap.TryGetValue(row.Content.ContentTypeId, out IContentType? contentType);

            TEntity entity = BuildEntity(row, contentType);

            // Deliberately not gated on the entity's own published flag: an unpublished entity keeps its
            // published property values and names so it can be republished in exactly its previous state.
            int publishedVersionId = row.PublishedContentVersion?.Id ?? 0;

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
                IPropertyType[] compositionProperties = contentType?.CompositionPropertyTypes.ToArray() ?? [];
                entity.Properties = new PropertyCollection(
                    await PropertyFactory.BuildEntities(
                        compositionProperties,
                        versionPropertyDtos,
                        publishedVersionId,
                        LanguageRepository));
            }

            ApplyVariations(
                entity,
                row.ContentVersion.Id,
                publishedVersionId,
                contentVersionCultureVariationsByVersionId,
                entityCultureVariationsByNodeId.GetValueOrDefault(row.Node.NodeId, []),
                isoCodeByLanguageId);

            entities.Add(entity);
        }

        await OnEntitiesAssembledAsync(db, entities, rows, loadTemplates, cancellationToken);

        for (var i = 0; i < entities.Count; i++)
        {
            TEntity entity = entities[i];
            ContentRow<TEntityDto, TContentVersionDto> row = rows[i];

            // Applying culture variations above, and anything the post-assembly hook set,
            // dirties the entity via CultureInfos/PublishCultureInfos collection-changed notifications, so a
            // freshly-assembled entity must be reset to a clean state before being handed to the caller.
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
        IReadOnlyList<ContentRow<TEntityDto, TContentVersionDto>> rows,
        bool loadTemplates,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    private async Task<(
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> ContentVersionCultureVariationsByVersionId,
        Dictionary<int, IReadOnlyList<TContentCultureVariationDto>> EntityCultureVariationsByNodeId,
        Dictionary<int, string?> IsoCodeByLanguageId)> LoadVariationsAsync(
            UmbracoDbContext db,
            List<int> versionIds,
            int[] nodeIds,
            Dictionary<int, IContentType?> contentTypeMap,
            CancellationToken cancellationToken)
    {
        // Skip variation queries entirely when no loaded content type varies by culture —
        // the common case for sites with only invariant content types.
        if (!contentTypeMap.Values.Any(contentType => contentType?.VariesByCulture() ?? false))
        {
            return (new(), new(), new());
        }

        // Content version culture variations — batched for the same 2100-parameter reason as property data.
        var allContentVersionCultureVariations = new List<ContentVersionCultureVariationDto>();
        foreach (IEnumerable<int> batch in versionIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            var batchIds = batch.ToList();
            allContentVersionCultureVariations.AddRange(await db.ContentVersionCultureVariations
                .Where(variation => batchIds.Contains(variation.VersionId))
                .ToListAsync(cancellationToken));
        }

        var contentVersionCultureVariationsByVersionId =
            allContentVersionCultureVariations
                .GroupBy(variation => variation.VersionId)
                .ToDictionary(group => group.Key, IReadOnlyList<ContentVersionCultureVariationDto> (group) => group.ToList());

        // Entity culture variations — nodeIds is unbounded for GetAll; batch accordingly.
        var allEntityCultureVariations = new List<TContentCultureVariationDto>();
        foreach (IEnumerable<int> batch in nodeIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            var batchIds = batch.ToList();
            allEntityCultureVariations.AddRange(await db.Set<TContentCultureVariationDto>()
                .Where(variation => batchIds.Contains(variation.NodeId))
                .ToListAsync(cancellationToken));
        }

        var entityCultureVariationsByNodeId =
            allEntityCultureVariations
                .GroupBy(variation => variation.NodeId)
                .ToDictionary(group => group.Key, IReadOnlyList<TContentCultureVariationDto> (group) => group.ToList());

        // The language table is tiny — one round trip resolves every LanguageId we might encounter below,
        // avoiding a per-culture-per-row lookup through the repository.
        Dictionary<int, string?> isoCodeByLanguageId = await db.Language
            .Select(language => new { language.Id, language.IsoCode })
            .ToDictionaryAsync(language => language.Id, language => language.IsoCode, cancellationToken);

        return (contentVersionCultureVariationsByVersionId, entityCultureVariationsByNodeId, isoCodeByLanguageId);
    }

    private static void ApplyVariations(
        TEntity entity,
        int currentVersionId,
        int publishedVersionId,
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> contentVersionCultureVariationsByVersionId,
        IReadOnlyList<TContentCultureVariationDto> entityCultureVariations,
        IReadOnlyDictionary<int, string?> isoCodeByLanguageId)
    {
        // Draft culture names
        if (contentVersionCultureVariationsByVersionId.TryGetValue(currentVersionId, out IReadOnlyList<ContentVersionCultureVariationDto>? draftVariations))
        {
            foreach (ContentVersionCultureVariationDto variation in draftVariations)
            {
                string? culture = ResolveIsoCode(variation.LanguageId, isoCodeByLanguageId);
                entity.SetCultureInfo(culture, variation.Name, variation.UpdateDate.EnsureUtc());
            }
        }

        // Published culture names
        if (entity.Published && publishedVersionId > 0 &&
            contentVersionCultureVariationsByVersionId.TryGetValue(publishedVersionId, out IReadOnlyList<ContentVersionCultureVariationDto>? publishedVariations))
        {
            foreach (ContentVersionCultureVariationDto variation in publishedVariations)
            {
                string? culture = ResolveIsoCode(variation.LanguageId, isoCodeByLanguageId);
                entity.SetPublishInfo(culture, variation.Name, variation.UpdateDate.EnsureUtc());
            }
        }

        // Edited cultures
        var editedCultures = new List<string?>();
        foreach (TContentCultureVariationDto variation in entityCultureVariations.Where(variation => variation.Edited))
        {
            editedCultures.Add(ResolveIsoCode(variation.LanguageId, isoCodeByLanguageId));
        }

        entity.SetCultureEdited(editedCultures);
    }

    // Matches the throw-on-not-found behavior of the obsolete ILanguageRepository.GetIsoCodeByIdAsync
    // bridge this class no longer calls — a LanguageId that isn't in the map indicates a dangling FK,
    // which should fail loudly rather than silently drop the culture (most consequential for the
    // edited-cultures list, which otherwise has no other error signal — SetCultureEdited silently
    // filters out null/blank entries).
    private static string? ResolveIsoCode(int languageId, IReadOnlyDictionary<int, string?> isoCodeByLanguageId)
    {
        if (!isoCodeByLanguageId.TryGetValue(languageId, out string? isoCode))
        {
            throw new ArgumentException($"Id {languageId} does not correspond to an existing language.", nameof(languageId));
        }

        return isoCode;
    }

    private async Task<ContentSchedule> ToContentScheduleAsync(ContentScheduleDto dto) =>
        new(
            dto.Id,
            await LanguageRepository.GetIsoCodeByIdAsync(dto.LanguageId) ?? Constants.System.InvariantCulture,
            dto.Date,
            dto.Action == ContentScheduleAction.Release.ToString() ? ContentScheduleAction.Release : ContentScheduleAction.Expire);

    /// <summary>
    ///     Gets a value indicating whether saving must keep names unique among siblings.
    /// </summary>
    protected virtual bool EnsureUniqueNaming => true;

    /// <summary>
    ///     Ensures the entity has a valid, sibling-unique invariant name and, for variant content, sibling-unique
    ///     names per culture.
    /// </summary>
    protected async Task SanitizeNamesAsync(UmbracoDbContext db, TEntity content, bool publishing)
    {
        await EnsureInvariantNameExistsAsync(content);
        await EnsureInvariantNameIsUniqueAsync(db, content);
        await EnsureVariantNamesAreUniqueAsync(db, content, publishing);
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
        List<SimilarNodeName> siblings = await db.Nodes
            .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentId)
            .Select(node => new SimilarNodeName { Id = node.NodeId, Name = node.Text })
            .ToListAsync();

        return (SimilarNodeName.GetUniqueName(siblings, id, nodeName), siblings);
    }

    private async Task EnsureVariantNamesAreUniqueAsync(UmbracoDbContext db, TEntity content, bool publishing)
    {
        if (!EnsureUniqueNaming || !content.ContentType.VariesByCulture() || content.CultureInfos?.Count == 0)
        {
            return;
        }

        // get names per culture, at same level (ie all siblings)
        var names = await db.ContentVersionCultureVariations
            .Join(db.ContentVersions.Where(contentVersion => contentVersion.Current), cultureVariation => cultureVariation.VersionId, contentVersion => contentVersion.Id, (cultureVariation, contentVersion) => new { cultureVariation, contentVersion })
            .Join(
                db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == content.ParentId && node.NodeId != content.Id),
                joined => joined.contentVersion.NodeId,
                node => node.NodeId,
                (joined, node) => new { joined.cultureVariation.Id, joined.cultureVariation.Name, joined.cultureVariation.LanguageId })
            .ToListAsync();

        if (names.Count == 0)
        {
            return;
        }

        // note: the code below means we are going to unique-ify every culture names, regardless
        // of whether the name has changed (ie the culture has been updated) - some saving culture
        // fr-FR could cause culture en-UK name to change - not sure that is clean
        ILookup<int, (int Id, string Name, int LanguageId)> namesByLanguage = names
            .Select(name => (name.Id, name.Name, name.LanguageId))
            .ToLookup(name => name.LanguageId);

        if (content.CultureInfos is null)
        {
            return;
        }

        foreach (ContentCultureInfos cultureInfo in content.CultureInfos)
        {
            int? langId = await LanguageRepository.GetIdByIsoCodeAsync(cultureInfo.Culture);
            if (!langId.HasValue)
            {
                continue;
            }

            IEnumerable<(int Id, string Name, int LanguageId)> cultureNames = namesByLanguage[langId.Value];
            if (!cultureNames.Any())
            {
                continue;
            }

            // get a unique name (literal duplicates first, then subclass-specific checks)
            List<SimilarNodeName> otherNames = [.. cultureNames.Select(cultureName => new SimilarNodeName { Id = cultureName.Id, Name = cultureName.Name })];
            var uniqueName = SimilarNodeName.GetUniqueName(otherNames, content.Id, cultureInfo.Name);
            uniqueName = await EnsureUniqueVariantNameAsync(uniqueName, content.Id, otherNames, cultureInfo.Culture);

            if (uniqueName == content.GetCultureName(cultureInfo.Culture))
            {
                continue;
            }

            // update the name, and the publish name if published
            content.SetCultureName(uniqueName, cultureInfo.Culture);
            if (publishing && (content.PublishCultureInfos?.ContainsKey(cultureInfo.Culture) ?? false))
            {
                content.SetPublishInfo(cultureInfo.Culture, uniqueName, DateTime.UtcNow);
            }
        }
    }

    /// <summary>
    ///     Called during variant name uniqueness to allow subclasses to apply additional uniqueness checks
    ///     (e.g. URL segment collision detection). The default implementation returns the name unchanged.
    /// </summary>
    protected virtual Task<string?> EnsureUniqueVariantNameAsync(string? nodeName, int nodeId, List<SimilarNodeName> siblings, string culture) =>
        Task.FromResult(nodeName);
}
