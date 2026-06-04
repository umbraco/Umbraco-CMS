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
        throw new NotImplementedException();

    /// <inheritdoc />
    public override Task<IContent?> GetVersionAsync(Guid versionKey, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

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
        throw new NotSupportedException(
            $"{nameof(AsyncDocumentRepository)} requires a full data projection to build entities. Use {nameof(PerformGetAsync)} instead.");

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

    private Task<List<IContent>> PerformGetRangeAsync(Guid[]? keys) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<NodeDto> nodeQuery = db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey);
            if (keys is not null)
            {
                nodeQuery = nodeQuery.Where(node => keys.Contains(node.UniqueId));
            }

            // Published version pairs: ContentVersion + DocumentVersion where Published = true.
            // Used as the inner side of the LEFT JOIN below so the published version is fetched
            // in the same round-trip as the current version (mirrors NPoco's nested LEFT JOIN in GetBaseQuery).
            var publishedSubquery =
                from pcv in db.ContentVersions
                join pdv in db.DocumentVersions.Where(x => x.Published) on pcv.Id equals pdv.Id
                select new { pcv, pdv };

            // Single round-trip: current version rows with the published version LEFT JOINed inline.
            // publishedContentVersion / publishedDocumentVersion are null for unpublished documents.
            var rows = await (
                from node in nodeQuery
                join document in db.Documents on node.NodeId equals document.NodeId
                join content in db.Content on node.NodeId equals content.NodeId
                join contentVersion in db.ContentVersions.Where(x => x.Current) on node.NodeId equals contentVersion.NodeId
                join documentVersion in db.DocumentVersions on contentVersion.Id equals documentVersion.Id
                join pub in publishedSubquery on node.NodeId equals pub.pcv.NodeId into pubGroup
                from pub in pubGroup.DefaultIfEmpty()
                select new
                {
                    node,
                    document,
                    content,
                    contentVersion,
                    documentVersion,
                    publishedContentVersion = pub.pcv,
                    publishedDocumentVersion = pub.pdv,
                })
            .ToListAsync();

            if (rows.Count == 0)
            {
                return [];
            }

            int[] nodeIds = rows.Select(row => row.node.NodeId).ToArray();

            // All relevant version IDs (current + published, deduplicated).
            // Current and published are always different IDs after the first publish.
            var allVersionIds = rows
                .Select(row => row.contentVersion.Id)
                .Concat(rows
                    .Where(row => row.publishedContentVersion is not null)
                    .Select(row => row.publishedContentVersion!.Id))
                .Distinct()
                .ToList();

            // TODO: batch allVersionIds/nodeIds IN queries when > Constants.Sql.MaxParameterCount

            // Property data grouped by version ID
            var propertyDtosByVersionId = (await db.PropertyData
                .Where(propertyData => allVersionIds.Contains(propertyData.VersionId))
                .ToListAsync())
                .GroupBy(propertyData => propertyData.VersionId)
                .ToDictionary(group => group.Key, group => group.ToList());

            // Pre-populate content type map. ContentTypeRepository caches, so no extra DB round-trips
            // after the first call per type — and we need the types up front to gate variation queries.
            var contentTypeMap = new Dictionary<int, IContentType?>();
            foreach (int contentTypeId in rows.Select(row => row.content.ContentTypeId).Distinct())
            {
                contentTypeMap[contentTypeId] = ContentTypeRepository.Get(contentTypeId);
            }

            // Skip variation queries entirely when no loaded content type varies by culture —
            // the common case for sites with only invariant content types.
            Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> cvVariationsByVersionId = new();
            Dictionary<int, IReadOnlyList<DocumentCultureVariationDto>> dcVariationsByNodeId = new();

            if (contentTypeMap.Values.Any(ct => ct?.VariesByCulture() ?? false))
            {
                // Content version culture variations (draft + published names per culture)
                cvVariationsByVersionId = (await db.ContentVersionCultureVariations
                    .Where(variation => allVersionIds.Contains(variation.VersionId))
                    .ToListAsync())
                    .GroupBy(variation => variation.VersionId)
                    .ToDictionary(group => group.Key, group => (IReadOnlyList<ContentVersionCultureVariationDto>)group.ToList());

                // Document culture variations (edited flag per culture per node)
                dcVariationsByNodeId = (await db.DocumentCultureVariations
                    .Where(variation => nodeIds.Contains(variation.NodeId))
                    .ToListAsync())
                    .GroupBy(variation => variation.NodeId)
                    .ToDictionary(group => group.Key, group => (IReadOnlyList<DocumentCultureVariationDto>)group.ToList());
            }

            var entities = new List<IContent>(rows.Count);

            foreach (var row in rows)
            {
                contentTypeMap.TryGetValue(row.content.ContentTypeId, out IContentType? contentType);

                // Wire nav properties (mirrors what NPoco [Reference] does automatically)
                row.content.NodeDto = row.node;
                row.documentVersion.ContentVersionDto = row.contentVersion;
                if (row.publishedDocumentVersion is not null)
                {
                    row.publishedDocumentVersion.ContentVersionDto = row.publishedContentVersion!;
                }

                row.document.ContentDto = row.content;
                row.document.CurrentVersion = row.documentVersion;
                row.document.PublishedVersion = row.publishedDocumentVersion;

                IContent entity = ContentBaseFactory.BuildEntity(row.document, contentType);

                var versionPropertyDtos = new List<PropertyDataDto>();
                if (propertyDtosByVersionId.TryGetValue(row.contentVersion.Id, out var currentProps))
                {
                    versionPropertyDtos.AddRange(currentProps);
                }

                if (row.publishedContentVersion is not null &&
                    propertyDtosByVersionId.TryGetValue(row.publishedContentVersion.Id, out var pubProps))
                {
                    versionPropertyDtos.AddRange(pubProps);
                }

                IPropertyType[] compositionProperties = contentType?.CompositionPropertyTypes.ToArray() ?? [];
                entity.Properties = new PropertyCollection(
                    await PropertyFactory.BuildEntities(
                        compositionProperties,
                        versionPropertyDtos,
                        row.publishedContentVersion?.Id ?? 0,
                        LanguageRepository));

                await ApplyVariationsAsync(
                    entity,
                    row.contentVersion.Id,
                    row.publishedContentVersion?.Id ?? 0,
                    cvVariationsByVersionId,
                    dcVariationsByNodeId.GetValueOrDefault(row.node.NodeId, []));

                entities.Add(entity);
            }

            return entities;
        });

    private async Task ApplyVariationsAsync(
        IContent entity,
        int currentVersionId,
        int publishedVersionId,
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> cvVariationsByVersionId,
        IReadOnlyList<DocumentCultureVariationDto> dcVariations)
    {
        // Draft culture names
        if (cvVariationsByVersionId.TryGetValue(currentVersionId, out IReadOnlyList<ContentVersionCultureVariationDto>? draftVariations))
        {
            foreach (ContentVersionCultureVariationDto variation in draftVariations)
            {
                string? culture = await LanguageRepository.GetIsoCodeByIdAsync(variation.LanguageId);
                entity.SetCultureInfo(culture, variation.Name, variation.UpdateDate.EnsureUtc());
            }
        }

        // Published culture names
        if (entity.Published && publishedVersionId > 0 &&
            cvVariationsByVersionId.TryGetValue(publishedVersionId, out IReadOnlyList<ContentVersionCultureVariationDto>? publishedVariations))
        {
            foreach (ContentVersionCultureVariationDto variation in publishedVariations)
            {
                string? culture = await LanguageRepository.GetIsoCodeByIdAsync(variation.LanguageId);
                entity.SetPublishInfo(culture, variation.Name, variation.UpdateDate.EnsureUtc());
            }
        }

        // Edited cultures
        var editedCultures = new List<string?>();
        foreach (DocumentCultureVariationDto variation in dcVariations.Where(v => v.Edited))
        {
            editedCultures.Add(await LanguageRepository.GetIsoCodeByIdAsync(variation.LanguageId));
        }

        entity.SetCultureEdited(editedCultures);
    }
}
