using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     Provides an EF Core backed async repository for <see cref="IContent" /> document entities.
/// </summary>
internal sealed class AsyncDocumentRepository
    : AsyncPublishableContentRepositoryBase<
        IContent,
        AsyncDocumentRepository,
        DocumentDto,
        DocumentVersionDto,
        DocumentCultureVariationDto>,
      IAsyncDocumentRepository
{
    private readonly ITemplateRepository _templateRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncDocumentRepository" /> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="languageRepository">The language repository.</param>
    /// <param name="relationRepository">The relation repository.</param>
    /// <param name="relationTypeRepository">The relation type repository.</param>
    /// <param name="propertyEditors">The property editor collection.</param>
    /// <param name="dataValueReferenceFactories">The data value reference factory collection.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="eventAggregator">The event aggregator for unit-of-work notifications.</param>
    /// <param name="repositoryCacheVersionService">The repository cache version service.</param>
    /// <param name="cacheSyncService">The cache synchronization service.</param>
    /// <param name="contentTypeRepository">The content type repository.</param>
    /// <param name="templateRepository">The template repository, used to validate template IDs on load.</param>
    internal AsyncDocumentRepository(
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
        ITemplateRepository templateRepository)
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
            contentTypeRepository)
    {
        _templateRepository = templateRepository;
    }

    /// <inheritdoc />
    public override Guid RecycleBinKey => Constants.System.RecycleBinContentKey;

    /// <inheritdoc />
    protected override Guid NodeObjectTypeKey => Constants.ObjectTypes.Document;

    /// <inheritdoc />
    protected override AsyncDocumentRepository This => this;

    // --- AsyncEntityRepositoryBase abstract overrides ---

    /// <inheritdoc />
    protected override async Task<IContent?> PerformGetAsync(Guid key)
    {
        List<IContent> results = await PerformGetRangeAsync([key]);
        return results.FirstOrDefault();
    }

    /// <inheritdoc />
    protected override async Task<IEnumerable<IContent>?> PerformGetAllAsync()
            => await PerformGetRangeAsync(null);

    /// <inheritdoc />
    protected override async Task<IEnumerable<IContent>?> PerformGetManyAsync(Guid[] keys)
        => await PerformGetRangeAsync(keys);

    /// <inheritdoc />
    protected override Task PersistNewItemAsync(IContent item) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    protected override Task PersistUpdatedItemAsync(IContent item) =>
        throw new NotImplementedException();

    // --- AsyncContentRepositoryBase abstract overrides ---

    /// <inheritdoc />
    protected override string RecycleBinCacheKey => CacheKeys.ContentRecycleBinCacheKey;

    /// <inheritdoc />
    public override Task<IEnumerable<IContent>> GetAllVersionsAsync(Guid nodeKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // All versions for the node, no Current filter — mirrors NPoco GetBaseQuery(current: false).
            // Published LEFT JOIN is keyed on NodeId so every version row carries the same published-version
            // context (which template is live, etc.), exactly as NPoco does.
            // Ordering must happen on the anonymous intermediate type before projecting into DocumentRow —
            // EF Core cannot translate member access on a record constructor call.
            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            List<DocumentRow> rows = await db.ContentVersions
                .Join(
                    db.Nodes.Where(node => node.UniqueId == nodeKey && node.NodeObjectType == NodeObjectTypeKey),
                    contentVersion => contentVersion.NodeId,
                    node => node.NodeId,
                    (contentVersion, node) => new { contentVersion, node })
                .Join(
                    db.DocumentVersions,
                    joined => joined.contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (joined, documentVersion) => new { joined.contentVersion, joined.node, documentVersion })
                .Join(
                    db.Content,
                    joined => joined.contentVersion.NodeId,
                    content => content.NodeId,
                    (joined, content) => new { joined.contentVersion, joined.node, joined.documentVersion, content })
                .Join(
                    db.Documents,
                    joined => joined.node.NodeId,
                    document => document.NodeId,
                    (joined, document) => new { joined.contentVersion, joined.node, joined.documentVersion, joined.content, document })
                .GroupJoin(
                    publishedSubquery,
                    joined => joined.node.NodeId,
                    pub => pub.contentVersion.NodeId,
                    (joined, pubGroup) => new { joined.contentVersion, joined.node, joined.documentVersion, joined.content, joined.document, pubGroup })
                .SelectMany(
                    joined => joined.pubGroup.DefaultIfEmpty(),
                    (joined, pub) => new { joined.contentVersion, joined.node, joined.documentVersion, joined.content, joined.document, pub })
                .OrderByDescending(joined => joined.contentVersion.Current)
                .ThenByDescending(joined => joined.contentVersion.VersionDate)
                .Select(joined => new DocumentRow(
                    joined.node,
                    joined.document,
                    joined.content,
                    joined.contentVersion,
                    joined.documentVersion,
                    joined.pub!.contentVersion,
                    joined.pub!.documentVersion))
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return Enumerable.Empty<IContent>();
            }

            return await AssembleEntitiesAsync(rows, db);
        });

    /// <inheritdoc />
    public override Task<IContent?> GetVersionAsync(Guid versionKey, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            // Filter by the version GUID key; no Current filter — historical versions are valid targets.
            // The Nodes join guards against returning versions that belong to a different object type.
            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            DocumentRow? row = await db.ContentVersions
                .Where(contentVersion => contentVersion.Key == versionKey)
                .Join(
                    db.DocumentVersions,
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion })
                .Join(
                    db.Content,
                    joined => joined.contentVersion.NodeId,
                    content => content.NodeId,
                    (joined, content) => new { joined.contentVersion, joined.documentVersion, content })
                .Join(
                    db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey),
                    joined => joined.contentVersion.NodeId,
                    node => node.NodeId,
                    (joined, node) => new { joined.contentVersion, joined.documentVersion, joined.content, node })
                .Join(
                    db.Documents,
                    joined => joined.node.NodeId,
                    document => document.NodeId,
                    (joined, document) => new { joined.contentVersion, joined.documentVersion, joined.content, joined.node, document })
                .GroupJoin(
                    publishedSubquery,
                    joined => joined.node.NodeId,
                    pub => pub.contentVersion.NodeId,
                    (joined, pubGroup) => new { joined.contentVersion, joined.documentVersion, joined.content, joined.node, joined.document, pubGroup })
                .SelectMany(
                    joined => joined.pubGroup.DefaultIfEmpty(),
                    (joined, pub) => new DocumentRow(
                        joined.node,
                        joined.document,
                        joined.content,
                        joined.contentVersion,
                        joined.documentVersion,
                        pub!.contentVersion,
                        pub!.documentVersion))
                .FirstOrDefaultAsync(cancellationToken);

            if (row is null)
            {
                return null;
            }

            List<IContent> entities = await AssembleEntitiesAsync([row], db);
            return entities.FirstOrDefault();
        });

    /// <inheritdoc />
    public override Task<PagedModel<IContent>> GetChildrenAsync(
        Guid parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken)
        => GetChildrenCoreAsync(parentKey, skip, take, propertyAliases, ordering, loadTemplates: true, cancellationToken);

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetChildrenWithoutTemplatesAsync(
        Guid parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken)
        => GetChildrenCoreAsync(parentKey, skip, take, propertyAliases, ordering, loadTemplates: false, cancellationToken);

    private Task<PagedModel<IContent>> GetChildrenCoreAsync(
        Guid parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, bool loadTemplates, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await ResolveNodeIdAsync(db, parentKey, cancellationToken);

            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentNodeId)
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<IContent> { Total = 0, Items = Enumerable.Empty<IContent>() };
            }

            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            // The ContentTypes JOIN is always included so contentTypeAlias is available for ordering.
            var baseQuery = db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentNodeId)
                .Join(
                    db.Documents,
                    node => node.NodeId,
                    document => document.NodeId,
                    (node, document) => new { node, document })
                .Join(
                    db.Content,
                    joined => joined.node.NodeId,
                    content => content.NodeId,
                    (joined, content) => new { joined.node, joined.document, content })
                .Join(
                    db.ContentVersions.Where(contentVersion => contentVersion.Current),
                    joined => joined.node.NodeId,
                    contentVersion => contentVersion.NodeId,
                    (joined, contentVersion) => new { joined.node, joined.document, joined.content, contentVersion })
                .Join(
                    db.DocumentVersions,
                    joined => joined.contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (joined, documentVersion) => new { joined.node, joined.document, joined.content, joined.contentVersion, documentVersion })
                .Join(
                    db.ContentTypes,
                    joined => joined.content.ContentTypeId,
                    contentType => contentType.NodeId,
                    (joined, contentType) => new { joined.node, joined.document, joined.content, joined.contentVersion, joined.documentVersion, contentType })
                .GroupJoin(
                    publishedSubquery,
                    joined => joined.node.NodeId,
                    pub => pub.contentVersion.NodeId,
                    (joined, pubGroup) => new { joined.node, joined.document, joined.content, joined.contentVersion, joined.documentVersion, joined.contentType, pubGroup })
                .SelectMany(
                    joined => joined.pubGroup.DefaultIfEmpty(),
                    (joined, pub) => new { joined.node, joined.document, joined.content, joined.contentVersion, joined.documentVersion, joined.contentType, pub });

            bool isCustomFieldOrdering = ordering?.IsCustomField == true;
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) == true
                && ordering?.IsInvariant == false;

            IReadOnlyList<DocumentRow> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<IContent> { Total = total, Items = Enumerable.Empty<IContent>() };
            }

            List<IContent> items = await AssembleEntitiesAsync(rows, db, propertyAliases, loadTemplates);

            async Task<IReadOnlyList<DocumentRow>> FetchCustomFieldOrdered()
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
                        .Where(joined => pageNodeIds.Contains(joined.node.NodeId))
                        .Select(joined => new DocumentRow(
                            joined.node,
                            joined.document,
                            joined.content,
                            joined.contentVersion,
                            joined.documentVersion,
                            joined.pub!.contentVersion,
                            joined.pub!.documentVersion))
                        .ToListAsync(cancellationToken),
                    cancellationToken);
            }

            async Task<IReadOnlyList<DocumentRow>> FetchCultureNameOrdered()
            {
                // Pre-fetch the language ID — the Language table is tiny (bounded by configured languages).
                // An unknown culture yields languageId = 0, which matches no CCV rows, so
                // variantName falls back to node.Text for every row (graceful degradation).
                int languageId = await db.Language
                    .Where(lang => lang.IsoCode == ordering!.Culture)
                    .Select(lang => lang.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                // LEFT JOIN ContentVersionCultureVariations filtered to the resolved language.
                // Join key is the current (draft) version ID, mirroring the NPoco CCV join condition.
                // variantName = COALESCE(ccv.Name, node.Text) — mirrors NPoco's VariantNameSqlExpression.
                var withVariantName = baseQuery
                    .GroupJoin(
                        db.ContentVersionCultureVariations.Where(ccv => ccv.LanguageId == languageId),
                        joined => joined.contentVersion.Id,
                        ccv => ccv.VersionId,
                        (joined, ccvGroup) => new
                        {
                            joined.node, joined.document, joined.content,
                            joined.contentVersion, joined.documentVersion,
                            joined.contentType, joined.pub, ccvGroup
                        })
                    .SelectMany(
                        joined => joined.ccvGroup.DefaultIfEmpty(),
                        (joined, ccv) => new
                        {
                            joined.node, joined.document, joined.content,
                            joined.contentVersion, joined.documentVersion,
                            joined.contentType, joined.pub,
                            variantName = ccv != null ? ccv.Name ?? joined.node.Text : joined.node.Text,
                        });

                bool descending = ordering!.Direction == Direction.Descending;
                var ordered = descending
                    ? withVariantName.OrderByDescending(joined => joined.variantName)
                    : withVariantName.OrderBy(joined => joined.variantName);

                return await ordered
                    .Skip(skip)
                    .Take(take)
                    .Select(joined => new DocumentRow(
                        joined.node,
                        joined.document,
                        joined.content,
                        joined.contentVersion,
                        joined.documentVersion,
                        joined.pub!.contentVersion,
                        joined.pub!.documentVersion))
                    .ToListAsync(cancellationToken);
            }

            async Task<IReadOnlyList<DocumentRow>> FetchDefaultOrdered()
            {
                // Invariant name ordering falls through here: textSelector = node.Text,
                // equivalent to COALESCE(NULL, node.Text) in NPoco's invariant path.
                var orderedQuery = ApplyDocumentOrdering(
                    baseQuery,
                    ordering,
                    sortOrderSelector: joined => joined.node.SortOrder,
                    textSelector: joined => joined.node.Text,
                    createDateSelector: joined => joined.node.CreateDate,
                    versionDateSelector: joined => joined.contentVersion.VersionDate,
                    idSelector: joined => joined.node.NodeId,
                    ownerSelector: joined => joined.node.UserId,
                    publishedSelector: joined => joined.documentVersion.Published,
                    contentTypeAliasSelector: joined => joined.contentType.Alias);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(joined => new DocumentRow(
                        joined.node,
                        joined.document,
                        joined.content,
                        joined.contentVersion,
                        joined.documentVersion,
                        joined.pub!.contentVersion,
                        joined.pub!.documentVersion))
                    .ToListAsync(cancellationToken);
            }
            return new PagedModel<IContent> { Total = total, Items = items };
        });

    /// <inheritdoc />
    public override Task<PagedModel<IContent>> GetDescendantsAsync(
        Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
        => GetDescendantsCoreAsync(ancestorKey, skip, take, ordering, loadTemplates: true, cancellationToken);

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetDescendantsWithoutTemplatesAsync(
        Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
        => GetDescendantsCoreAsync(ancestorKey, skip, take, ordering, loadTemplates: false, cancellationToken);

    private Task<PagedModel<IContent>> GetDescendantsCoreAsync(
        Guid ancestorKey, int skip, int take, Ordering? ordering, bool loadTemplates, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int parentNodeId = await ResolveNodeIdAsync(db, ancestorKey, cancellationToken);

            string pathMatch = parentNodeId == -1 ? "-1," : $",{parentNodeId},";

            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && EF.Functions.Like(node.Path, $"%{pathMatch}%"))
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<IContent> { Total = 0, Items = Enumerable.Empty<IContent>() };
            }

            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            var baseQuery = db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && EF.Functions.Like(node.Path, $"%{pathMatch}%"))
                .Join(
                    db.Documents,
                    node => node.NodeId,
                    document => document.NodeId,
                    (node, document) => new { node, document })
                .Join(
                    db.Content,
                    joined => joined.node.NodeId,
                    content => content.NodeId,
                    (joined, content) => new { joined.node, joined.document, content })
                .Join(
                    db.ContentVersions.Where(contentVersion => contentVersion.Current),
                    joined => joined.node.NodeId,
                    contentVersion => contentVersion.NodeId,
                    (joined, contentVersion) => new { joined.node, joined.document, joined.content, contentVersion })
                .Join(
                    db.DocumentVersions,
                    joined => joined.contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (joined, documentVersion) => new { joined.node, joined.document, joined.content, joined.contentVersion, documentVersion })
                .Join(
                    db.ContentTypes,
                    joined => joined.content.ContentTypeId,
                    contentType => contentType.NodeId,
                    (joined, contentType) => new { joined.node, joined.document, joined.content, joined.contentVersion, joined.documentVersion, contentType })
                .GroupJoin(
                    publishedSubquery,
                    joined => joined.node.NodeId,
                    pub => pub.contentVersion.NodeId,
                    (joined, pubGroup) => new { joined.node, joined.document, joined.content, joined.contentVersion, joined.documentVersion, joined.contentType, pubGroup })
                .SelectMany(
                    joined => joined.pubGroup.DefaultIfEmpty(),
                    (joined, pub) => new { joined.node, joined.document, joined.content, joined.contentVersion, joined.documentVersion, joined.contentType, pub });

            bool isCustomFieldOrdering = ordering?.IsCustomField == true;
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) == true
                && ordering?.IsInvariant == false;

            IReadOnlyList<DocumentRow> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<IContent> { Total = total, Items = Enumerable.Empty<IContent>() };
            }

            List<IContent> items = await AssembleEntitiesAsync(rows, db, loadTemplates: loadTemplates);

            async Task<IReadOnlyList<DocumentRow>> FetchCustomFieldOrdered()
            {
                List<int> candidateNodeIds = await db.Nodes
                    .Where(node => node.NodeObjectType == NodeObjectTypeKey && EF.Functions.Like(node.Path, $"%{pathMatch}%"))
                    .Select(node => node.NodeId)
                    .ToListAsync(cancellationToken);

                return await FetchCustomFieldOrderedPageAsync(
                    db,
                    candidateNodeIds,
                    ordering!,
                    skip,
                    take,
                    pageNodeIds => baseQuery
                        .Where(joined => pageNodeIds.Contains(joined.node.NodeId))
                        .Select(joined => new DocumentRow(
                            joined.node,
                            joined.document,
                            joined.content,
                            joined.contentVersion,
                            joined.documentVersion,
                            joined.pub!.contentVersion,
                            joined.pub!.documentVersion))
                        .ToListAsync(cancellationToken),
                    cancellationToken);
            }

            async Task<IReadOnlyList<DocumentRow>> FetchCultureNameOrdered()
            {
                // Pre-fetch the language ID — the Language table is tiny (bounded by configured languages).
                // An unknown culture yields languageId = 0, which matches no CCV rows, so
                // variantName falls back to node.Text for every row (graceful degradation).
                int languageId = await db.Language
                    .Where(lang => lang.IsoCode == ordering!.Culture)
                    .Select(lang => lang.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                // LEFT JOIN ContentVersionCultureVariations filtered to the resolved language.
                // Join key is the current (draft) version ID, mirroring the NPoco CCV join condition.
                // variantName = COALESCE(ccv.Name, node.Text) — mirrors NPoco's VariantNameSqlExpression.
                var withVariantName = baseQuery
                    .GroupJoin(
                        db.ContentVersionCultureVariations.Where(ccv => ccv.LanguageId == languageId),
                        joined => joined.contentVersion.Id,
                        ccv => ccv.VersionId,
                        (joined, ccvGroup) => new
                        {
                            joined.node, joined.document, joined.content,
                            joined.contentVersion, joined.documentVersion,
                            joined.contentType, joined.pub, ccvGroup
                        })
                    .SelectMany(
                        joined => joined.ccvGroup.DefaultIfEmpty(),
                        (joined, ccv) => new
                        {
                            joined.node, joined.document, joined.content,
                            joined.contentVersion, joined.documentVersion,
                            joined.contentType, joined.pub,
                            variantName = ccv != null ? ccv.Name ?? joined.node.Text : joined.node.Text,
                        });

                bool descending = ordering!.Direction == Direction.Descending;
                var ordered = descending
                    ? withVariantName.OrderByDescending(joined => joined.variantName)
                    : withVariantName.OrderBy(joined => joined.variantName);

                return await ordered
                    .Skip(skip)
                    .Take(take)
                    .Select(joined => new DocumentRow(
                        joined.node,
                        joined.document,
                        joined.content,
                        joined.contentVersion,
                        joined.documentVersion,
                        joined.pub!.contentVersion,
                        joined.pub!.documentVersion))
                    .ToListAsync(cancellationToken);
            }

            async Task<IReadOnlyList<DocumentRow>> FetchDefaultOrdered()
            {
                // Invariant name ordering falls through here: textSelector = node.Text,
                // equivalent to COALESCE(NULL, node.Text) in NPoco's invariant path.
                var orderedQuery = ApplyDocumentOrdering(
                    baseQuery,
                    ordering,
                    sortOrderSelector: joined => joined.node.SortOrder,
                    textSelector: joined => joined.node.Text,
                    createDateSelector: joined => joined.node.CreateDate,
                    versionDateSelector: joined => joined.contentVersion.VersionDate,
                    idSelector: joined => joined.node.NodeId,
                    ownerSelector: joined => joined.node.UserId,
                    publishedSelector: joined => joined.documentVersion.Published,
                    contentTypeAliasSelector: joined => joined.contentType.Alias);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(joined => new DocumentRow(
                        joined.node,
                        joined.document,
                        joined.content,
                        joined.contentVersion,
                        joined.documentVersion,
                        joined.pub!.contentVersion,
                        joined.pub!.documentVersion))
                    .ToListAsync(cancellationToken);
            }
            return new PagedModel<IContent> { Total = total, Items = items };
        });

    /// <inheritdoc />
    public override Task<IEnumerable<IContent>> GetRecycleBinAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Task<PagedModel<IContent>> GetPagedRecycleBinAsync(long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    protected override Task OnUowRefreshedEntityAsync(IContent entity, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    // --- AsyncPublishableContentRepositoryBase abstract overrides ---

    /// <inheritdoc />
    protected override IContent BuildEntity(DocumentDto entityDto, IContentType? contentType) =>
        ContentBaseFactory.BuildEntity(entityDto, contentType);

    /// <inheritdoc />
    protected override DocumentDto BuildEntityDto(IContent entity) =>
        throw new NotImplementedException();

    // --- IAsyncDocumentRepository: permissions ---

    /// <inheritdoc />
    public Task ReplaceContentPermissionsAsync(EntityPermissionSet permissionSet, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task AssignEntityPermissionAsync(IContent entity, string permission, IEnumerable<Guid> groupKeys, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<EntityPermissionCollection> GetPermissionsForEntityAsync(Guid entityKey, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task AddOrUpdatePermissionsAsync(ContentPermissionSet permission, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    // --- IAsyncDocumentRepository: document-specific ---

    /// <inheritdoc />
    public Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    // --- Private helpers ---

    // Projects a database row spanning Nodes/Documents/Content/ContentVersions/DocumentVersions
    // plus the optional published version (LEFT JOINed by NodeId) into a single typed record,
    // allowing entity assembly to be shared across PerformGetRangeAsync, GetVersionAsync and
    // GetAllVersionsAsync without anonymous-type boundaries.
    private sealed record DocumentRow(
        NodeDto Node,
        DocumentDto Document,
        ContentDto Content,
        ContentVersionDto ContentVersion,
        DocumentVersionDto DocumentVersion,
        ContentVersionDto? PublishedContentVersion,
        DocumentVersionDto? PublishedDocumentVersion);

    // Applies document ordering to any anonymous intermediate query type T before it is projected
    // to DocumentRow. Ordering must happen while the anonymous type is still live — EF Core cannot
    // translate member access on named record-type constructor calls in OrderBy key selectors.
    // The typed Expression<Func<T, TKey>> selectors allow EF Core to generate correct SQL ORDER BY
    // clauses through the anonymous type, which it can trace back to the original DTO columns.
    private static IOrderedQueryable<T> ApplyDocumentOrdering<T>(
        IQueryable<T> source,
        Ordering? ordering,
        Expression<Func<T, int>> sortOrderSelector,
        Expression<Func<T, string?>> textSelector,
        Expression<Func<T, DateTime>> createDateSelector,
        Expression<Func<T, DateTime>> versionDateSelector,
        Expression<Func<T, int>> idSelector,
        Expression<Func<T, int?>> ownerSelector,
        Expression<Func<T, bool>> publishedSelector,
        Expression<Func<T, string?>> contentTypeAliasSelector)
    {
        bool descending = ordering?.Direction == Direction.Descending;
        return ordering?.OrderBy?.ToLowerInvariant() switch
        {
            // Invariant name ordering (node.Text). Culture-specific name ordering is handled
            // by the callers via a ContentVersionCultureVariation JOIN before reaching this method.
            "name" => descending
                ? source.OrderByDescending(textSelector)
                : source.OrderBy(textSelector),
            "createdate" => descending
                ? source.OrderByDescending(createDateSelector)
                : source.OrderBy(createDateSelector),
            "versiondate" or "updatedate" => descending
                ? source.OrderByDescending(versionDateSelector)
                : source.OrderBy(versionDateSelector),
            "id" => descending
                ? source.OrderByDescending(idSelector)
                : source.OrderBy(idSelector),
            "owner" => descending
                ? source.OrderByDescending(ownerSelector)
                : source.OrderBy(ownerSelector),
            "published" => descending
                ? source.OrderByDescending(publishedSelector)
                : source.OrderBy(publishedSelector),
            "contenttypealias" => descending
                ? source.OrderByDescending(contentTypeAliasSelector)
                : source.OrderBy(contentTypeAliasSelector),
            // Custom-field ordering (ordering.IsCustomField) is intercepted by callers before reaching
            // this method — see ResolveCustomFieldOrderedNodeIdsAsync.
            _ => descending
                ? source.OrderByDescending(sortOrderSelector)
                : source.OrderBy(sortOrderSelector),
        };
    }

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

    // Resolves and sorts (but does not page) the full candidate node ID set by a custom property field.
    // Values are fetched via ordinary translatable LINQ (no raw SQL, no per-provider SQL fragments) —
    // the sort itself happens in-memory since no single SQL column can hold all four typed columns
    // (int/decimal/date/varchar) plus the string SortableValue override in one comparable form.
    private async Task<List<int>> ResolveCustomFieldOrderedNodeIdsAsync(
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

    // Re-sequences a set of already-fetched rows to match a previously computed node ID order. A node ID
    // present in orderedNodeIds is silently skipped if it has no corresponding row — the ordering-key
    // resolution and the row fetch are two separate round trips (unlike the other ordering paths, which
    // select/page within a single query), so a node concurrently deleted/moved between them would
    // otherwise fail the whole page with a KeyNotFoundException instead of just omitting that one node.
    private static List<DocumentRow> ReorderRowsByNodeIds(List<DocumentRow> rows, List<int> orderedNodeIds)
    {
        Dictionary<int, DocumentRow> rowsByNodeId = rows.ToDictionary(row => row.Node.NodeId);
        return orderedNodeIds
            .Where(rowsByNodeId.ContainsKey)
            .Select(nodeId => rowsByNodeId[nodeId])
            .ToList();
    }

    // Shared by both GetChildrenCoreAsync and GetDescendantsCoreAsync's custom-field ordering path: resolves
    // the full ordered candidate ID list, pages it, and fetches+re-sequences the page's rows. Callers differ
    // only in how candidateNodeIds is filtered (children vs. descendants) and how fetchRowsForPageNodeIds
    // queries baseQuery (a distinct anonymous-typed query per caller) — everything else is identical.
    private async Task<IReadOnlyList<DocumentRow>> FetchCustomFieldOrderedPageAsync(
        UmbracoDbContext db,
        List<int> candidateNodeIds,
        Ordering ordering,
        int skip,
        int take,
        Func<List<int>, Task<List<DocumentRow>>> fetchRowsForPageNodeIds,
        CancellationToken cancellationToken)
    {
        List<int> orderedNodeIds = await ResolveCustomFieldOrderedNodeIdsAsync(
            db, candidateNodeIds, ordering.OrderBy!, ordering.Culture ?? string.Empty, ordering.Direction, cancellationToken);

        List<int> pageNodeIds = orderedNodeIds.Skip(skip).Take(take).ToList();
        if (pageNodeIds.Count == 0)
        {
            return [];
        }

        List<DocumentRow> unorderedRows = await fetchRowsForPageNodeIds(pageNodeIds);
        return ReorderRowsByNodeIds(unorderedRows, pageNodeIds);
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
            return direction == Direction.Descending ? -missingFirst : missingFirst;
        }

        int comparison = CompareOrderingValues(left!, right!);
        if (comparison == 0)
        {
            return leftNodeId.CompareTo(rightNodeId);
        }

        return direction == Direction.Descending ? -comparison : comparison;
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

    private Task<List<IContent>> PerformGetRangeAsync(Guid[]? keys) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<NodeDto> nodeQuery = db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey);
            if (keys is not null)
            {
                List<Guid> keyList = new(keys);
                nodeQuery = nodeQuery.Where(node => keyList.Contains(node.UniqueId));
            }

            // Published version pairs: ContentVersion + DocumentVersion where Published = true.
            // Used as the inner side of the LEFT JOIN below so the published version is fetched
            // in the same round-trip as the current version (mirrors NPoco's nested LEFT JOIN in GetBaseQuery).
            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            // Single round-trip: current version rows with the published version LEFT JOINed inline.
            // publishedContentVersion / publishedDocumentVersion are null for unpublished documents.
            List<DocumentRow> rows = await nodeQuery
                .Join(
                    db.Documents,
                    node => node.NodeId,
                    document => document.NodeId,
                    (node, document) => new { node, document })
                .Join(
                    db.Content,
                    joined => joined.node.NodeId,
                    content => content.NodeId,
                    (joined, content) => new { joined.node, joined.document, content })
                .Join(
                    db.ContentVersions.Where(contentVersion => contentVersion.Current),
                    joined => joined.node.NodeId,
                    contentVersion => contentVersion.NodeId,
                    (joined, contentVersion) => new { joined.node, joined.document, joined.content, contentVersion })
                .Join(
                    db.DocumentVersions,
                    joined => joined.contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (joined, documentVersion) => new { joined.node, joined.document, joined.content, joined.contentVersion, documentVersion })
                .GroupJoin(
                    publishedSubquery,
                    joined => joined.node.NodeId,
                    pub => pub.contentVersion.NodeId,
                    (joined, pubGroup) => new { joined.node, joined.document, joined.content, joined.contentVersion, joined.documentVersion, pubGroup })
                .SelectMany(
                    joined => joined.pubGroup.DefaultIfEmpty(),
                    (joined, pub) => new DocumentRow(
                        joined.node,
                        joined.document,
                        joined.content,
                        joined.contentVersion,
                        joined.documentVersion,
                        pub!.contentVersion,
                        pub!.documentVersion))
                .ToListAsync();

            if (rows.Count == 0)
            {
                return [];
            }

            return await AssembleEntitiesAsync(rows, db);
        });

    private HashSet<int> ResolveValidTemplateIds(IReadOnlyList<DocumentRow> rows)
    {
        int[] templateIds = rows
            .SelectMany(row => new[] { row.DocumentVersion.TemplateId, row.PublishedDocumentVersion?.TemplateId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        return templateIds.Length > 0
            ? [.._templateRepository.GetMany(templateIds).Select(template => template.Id)]
            : [];
    }

    private async Task<List<IContent>> AssembleEntitiesAsync(IReadOnlyList<DocumentRow> rows, UmbracoDbContext db, string[]? propertyAliases = null, bool loadTemplates = true)
    {
        // Resolve valid template IDs once for the whole batch (mirrors NPoco AddAdditionalTempContentMapping).
        // null means "skip template assignment entirely".
        HashSet<int>? validTemplateIds = loadTemplates ? ResolveValidTemplateIds(rows) : null;

        int[] nodeIds = rows.Select(row => row.Node.NodeId).ToArray();

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
            await LoadPropertyDataAsync(db, allVersionIds, propertyAliases);

        // Pre-populate content type map. ContentTypeRepository caches, so no extra DB round-trips
        // after the first call per type — and we need the types up front to gate variation queries.
        var contentTypeMap = new Dictionary<int, IContentType?>();
        foreach (int contentTypeId in rows.Select(row => row.Content.ContentTypeId).Distinct())
        {
            contentTypeMap[contentTypeId] = await ContentTypeRepository.GetAsync(contentTypeId, CancellationToken.None);
        }

        (Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> contentVersionCultureVariationsByVersionId,
         Dictionary<int, IReadOnlyList<DocumentCultureVariationDto>> documentCultureVariationsByNodeId,
         Dictionary<int, string?> isoCodeByLanguageId) =
            await LoadVariationsAsync(db, allVersionIds, nodeIds, contentTypeMap);

        var entities = new List<IContent>(rows.Count);

        foreach (DocumentRow row in rows)
        {
            contentTypeMap.TryGetValue(row.Content.ContentTypeId, out IContentType? contentType);

            // Wire nav properties (mirrors what NPoco [Reference] does automatically)
            row.Content.NodeDto = row.Node;
            row.DocumentVersion.ContentVersionDto = row.ContentVersion;
            if (row.PublishedDocumentVersion is not null)
            {
                row.PublishedDocumentVersion.ContentVersionDto = row.PublishedContentVersion!;
            }

            row.Document.ContentDto = row.Content;
            row.Document.CurrentVersion = row.DocumentVersion;
            row.Document.PublishedVersion = row.PublishedDocumentVersion;

            IContent entity = BuildEntity(row.Document, contentType);

            if (validTemplateIds is not null)
            {
                int? draftTemplateId = row.DocumentVersion.TemplateId;
                if (draftTemplateId.HasValue && validTemplateIds.Contains(draftTemplateId.Value))
                {
                    entity.TemplateId = draftTemplateId;
                }

                int? publishedTemplateId = row.PublishedDocumentVersion?.TemplateId;
                if (publishedTemplateId.HasValue && validTemplateIds.Contains(publishedTemplateId.Value))
                {
                    entity.PublishTemplateId = publishedTemplateId;
                }
            }

            var versionPropertyDtos = new List<PropertyDataDto>();
            if (propertyDtosByVersionId.TryGetValue(row.ContentVersion.Id, out List<PropertyDataDto>? currentProps))
            {
                versionPropertyDtos.AddRange(currentProps);
            }

            if (row.PublishedContentVersion is not null &&
                propertyDtosByVersionId.TryGetValue(row.PublishedContentVersion.Id, out List<PropertyDataDto>? pubProps))
            {
                versionPropertyDtos.AddRange(pubProps);
            }

            IPropertyType[] compositionProperties = contentType?.CompositionPropertyTypes.ToArray() ?? [];
            entity.Properties = new PropertyCollection(
                await PropertyFactory.BuildEntities(
                    compositionProperties,
                    versionPropertyDtos,
                    row.PublishedContentVersion?.Id ?? 0,
                    LanguageRepository));

            ApplyVariations(
                entity,
                row.ContentVersion.Id,
                row.PublishedContentVersion?.Id ?? 0,
                contentVersionCultureVariationsByVersionId,
                documentCultureVariationsByNodeId.GetValueOrDefault(row.Node.NodeId, []),
                isoCodeByLanguageId);

            entities.Add(entity);
        }

        return entities;
    }

    private async Task<Dictionary<int, List<PropertyDataDto>>> LoadPropertyDataAsync(
        UmbracoDbContext db, List<int> versionIds, string[]? propertyAliases = null)
    {
        // Empty alias list means "no custom properties" — skip the query entirely.
        if (propertyAliases is { Length: 0 })
        {
            return new Dictionary<int, List<PropertyDataDto>>();
        }

        // Batched to stay within SQL Server's 2100-parameter limit.
        // allVersionIds can reach 2× the document count (current + published version per document).
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
                    .ToListAsync());
            }
            else
            {
                allPropertyData.AddRange(await db.PropertyData
                    .Where(propertyData => batchIds.Contains(propertyData.VersionId))
                    .ToListAsync());
            }
        }

        return allPropertyData
            .GroupBy(propertyData => propertyData.VersionId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    private async Task<(
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> ContentVersionCultureVariationsByVersionId,
        Dictionary<int, IReadOnlyList<DocumentCultureVariationDto>> DocumentCultureVariationsByNodeId,
        Dictionary<int, string?> IsoCodeByLanguageId)> LoadVariationsAsync(
            UmbracoDbContext db,
            List<int> versionIds,
            int[] nodeIds,
            Dictionary<int, IContentType?> contentTypeMap)
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
                .ToListAsync());
        }

        var contentVersionCultureVariationsByVersionId =
            allContentVersionCultureVariations
                .GroupBy(variation => variation.VersionId)
                .ToDictionary(group => group.Key, IReadOnlyList<ContentVersionCultureVariationDto> (group) => group.ToList());

        // Document culture variations — nodeIds is unbounded for GetAll; batch accordingly.
        var allDocumentCultureVariations = new List<DocumentCultureVariationDto>();
        foreach (IEnumerable<int> batch in nodeIds.InGroupsOf(Core.Constants.Sql.MaxParameterCount))
        {
            var batchIds = batch.ToList();
            allDocumentCultureVariations.AddRange(await db.DocumentCultureVariations
                .Where(variation => batchIds.Contains(variation.NodeId))
                .ToListAsync());
        }

        var documentCultureVariationsByNodeId =
            allDocumentCultureVariations
                .GroupBy(variation => variation.NodeId)
                .ToDictionary(group => group.Key, IReadOnlyList<DocumentCultureVariationDto> (group) => group.ToList());

        // The language table is tiny — one round trip resolves every LanguageId we might encounter below,
        // avoiding a per-culture-per-row lookup through the repository.
        Dictionary<int, string?> isoCodeByLanguageId = await db.Language
            .Select(language => new { language.Id, language.IsoCode })
            .ToDictionaryAsync(language => language.Id, language => language.IsoCode);

        return (contentVersionCultureVariationsByVersionId, documentCultureVariationsByNodeId, isoCodeByLanguageId);
    }

    private static void ApplyVariations(
        IContent entity,
        int currentVersionId,
        int publishedVersionId,
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> contentVersionCultureVariationsByVersionId,
        IReadOnlyList<DocumentCultureVariationDto> documentCultureVariations,
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
        foreach (DocumentCultureVariationDto variation in documentCultureVariations.Where(variation => variation.Edited))
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
}
