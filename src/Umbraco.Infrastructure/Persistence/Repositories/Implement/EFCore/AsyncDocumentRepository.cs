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
        IContentTypeRepository contentTypeRepository)
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
            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            // All versions for the node, no Current filter — mirrors NPoco GetBaseQuery(current: false).
            // Published LEFT JOIN is keyed on NodeId so every version row carries the same published-version
            // context (which template is live, etc.), exactly as NPoco does.
            // Ordering must happen on the anonymous intermediate type before projecting into DocumentRow —
            // EF Core cannot translate member access on a record constructor call.
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
            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            // Filter by the version GUID key; no Current filter — historical versions are valid targets.
            // The Nodes join guards against returning versions that belong to a different object type.
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
    public override Task<PagedModel<IContent>> GetChildrenAsync(Guid parentKey, long pageIndex, int pageSize, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Task<PagedModel<IContent>> GetDescendantsAsync(Guid ancestorKey, long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

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

    // --- IAsyncDocumentRepository: paged overloads with loadTemplates ---

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetChildrenAsync(Guid parentKey, long pageIndex, int pageSize, string[]? propertyAliases, Ordering? ordering, bool loadTemplates, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetDescendantsAsync(Guid ancestorKey, long pageIndex, int pageSize, Ordering? ordering, bool loadTemplates, CancellationToken cancellationToken) =>
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

    private async Task<List<IContent>> AssembleEntitiesAsync(IReadOnlyList<DocumentRow> rows, UmbracoDbContext db)
    {
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
            await LoadPropertyDataAsync(db, allVersionIds);

        // Pre-populate content type map. ContentTypeRepository caches, so no extra DB round-trips
        // after the first call per type — and we need the types up front to gate variation queries.
        var contentTypeMap = new Dictionary<int, IContentType?>();
        foreach (int contentTypeId in rows.Select(row => row.Content.ContentTypeId).Distinct())
        {
            contentTypeMap[contentTypeId] = ContentTypeRepository.Get(contentTypeId);
        }

        (Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> contentVersionCultureVariationsByVersionId,
         Dictionary<int, IReadOnlyList<DocumentCultureVariationDto>> documentCultureVariationsByNodeId) =
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

            await ApplyVariationsAsync(
                entity,
                row.ContentVersion.Id,
                row.PublishedContentVersion?.Id ?? 0,
                contentVersionCultureVariationsByVersionId,
                documentCultureVariationsByNodeId.GetValueOrDefault(row.Node.NodeId, []));

            entities.Add(entity);
        }

        return entities;
    }

    private async Task<Dictionary<int, List<PropertyDataDto>>> LoadPropertyDataAsync(
        UmbracoDbContext db, List<int> versionIds)
    {
        // Batched to stay within SQL Server's 2100-parameter limit.
        // allVersionIds can reach 2× the document count (current + published version per document).
        var allPropertyData = new List<PropertyDataDto>();
        foreach (IEnumerable<int> batch in versionIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            var batchIds = batch.ToList();
            allPropertyData.AddRange(await db.PropertyData
                .Where(propertyData => batchIds.Contains(propertyData.VersionId))
                .ToListAsync());
        }

        return allPropertyData
            .GroupBy(propertyData => propertyData.VersionId)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    private async Task<(
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> ContentVersionCultureVariationsByVersionId,
        Dictionary<int, IReadOnlyList<DocumentCultureVariationDto>> DocumentCultureVariationsByNodeId)> LoadVariationsAsync(
            UmbracoDbContext db,
            List<int> versionIds,
            int[] nodeIds,
            Dictionary<int, IContentType?> contentTypeMap)
    {
        // Skip variation queries entirely when no loaded content type varies by culture —
        // the common case for sites with only invariant content types.
        if (!contentTypeMap.Values.Any(contentType => contentType?.VariesByCulture() ?? false))
        {
            return (new(), new());
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

        return (contentVersionCultureVariationsByVersionId, documentCultureVariationsByNodeId);
    }

    private async Task ApplyVariationsAsync(
        IContent entity,
        int currentVersionId,
        int publishedVersionId,
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> contentVersionCultureVariationsByVersionId,
        IReadOnlyList<DocumentCultureVariationDto> documentCultureVariations)
    {
        // Draft culture names
        if (contentVersionCultureVariationsByVersionId.TryGetValue(currentVersionId, out IReadOnlyList<ContentVersionCultureVariationDto>? draftVariations))
        {
            foreach (ContentVersionCultureVariationDto variation in draftVariations)
            {
                // TODO: Look into adding language keys to variation to fix this obsolete.
                string? culture = await LanguageRepository.GetIsoCodeByIdAsync(variation.LanguageId);
                entity.SetCultureInfo(culture, variation.Name, variation.UpdateDate.EnsureUtc());
            }
        }

        // Published culture names
        if (entity.Published && publishedVersionId > 0 &&
            contentVersionCultureVariationsByVersionId.TryGetValue(publishedVersionId, out IReadOnlyList<ContentVersionCultureVariationDto>? publishedVariations))
        {
            foreach (ContentVersionCultureVariationDto variation in publishedVariations)
            {
                string? culture = await LanguageRepository.GetIsoCodeByIdAsync(variation.LanguageId);
                entity.SetPublishInfo(culture, variation.Name, variation.UpdateDate.EnsureUtc());
            }
        }

        // Edited cultures
        var editedCultures = new List<string?>();
        foreach (DocumentCultureVariationDto variation in documentCultureVariations.Where(variation => variation.Edited))
        {
            editedCultures.Add(await LanguageRepository.GetIsoCodeByIdAsync(variation.LanguageId));
        }

        entity.SetCultureEdited(editedCultures);
    }
}
