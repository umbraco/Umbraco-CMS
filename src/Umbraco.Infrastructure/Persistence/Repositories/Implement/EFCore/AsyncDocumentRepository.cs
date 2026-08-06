using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
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
    private readonly IIdKeyMap _idKeyMap;
    private readonly ITagRepository _tagRepository;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly AsyncPermissionRepository<IContent> _permissionRepository;

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
    /// <param name="idKeyMap">The ID/key map, used to resolve data type configuration for sortable property values.</param>
    /// <param name="tagRepository">The tag repository, used to persist tag values for tag-enabled properties on publish.</param>
    /// <param name="jsonSerializer">The JSON serializer, used to parse legacy JSON-stored tag values.</param>
    /// <param name="userGroupService">The user group service, used to resolve user group keys to IDs for permission storage.</param>
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
        ITemplateRepository templateRepository,
        IIdKeyMap idKeyMap,
        ITagRepository tagRepository,
        IJsonSerializer jsonSerializer,
        IUserGroupService userGroupService)
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
        _idKeyMap = idKeyMap;
        _tagRepository = tagRepository;
        _jsonSerializer = jsonSerializer;
        _permissionRepository = new AsyncPermissionRepository<IContent>(scopeAccessor, appCaches, userGroupService);
    }

    /// <inheritdoc />
    public override Guid RecycleBinKey => Constants.System.RecycleBinContentKey;

    /// <inheritdoc />
    protected override Guid NodeObjectTypeKey => Constants.ObjectTypes.Document;

    /// <inheritdoc />
    protected override AsyncDocumentRepository This => this;

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
    protected override async Task PersistNewItemAsync(IContent item) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            item.AddingEntity();

            var publishing = item.PublishedState == PublishedState.Publishing;

            AssignDefaultTemplateIfMissing(item);

            // TODO (EF Core): port sibling name-uniqueness (SimilarNodeName.GetUniqueName) and variant name
            // handling, see NPoco PublishableContentRepositoryBase.SanitizeNames.
            SanitizeNames(item);

            item.SanitizeEntityPropertiesForXmlStorage();

            DocumentDto dto = BuildEntityDto(item);

            await PersistNewNodeAsync(db, item, dto);
            await PersistNewContentAsync(db, item, dto);
            await PersistNewVersionsAsync(db, item, dto);

            (bool edited, HashSet<string>? editedCultures) = await PersistNewPropertyDataAsync(db, item);

            // if !publishing, we may have a new name != current publish name, also impacts 'edited'
            if (!publishing && item.PublishName != item.Name)
            {
                edited = true;
            }

            // at that point, when publishing, the entity still has its old Published value
            // so we need to explicitly update the dto to persist the correct value
            if (publishing)
            {
                dto.Published = true;
            }

            dto.NodeId = item.Id;
            item.Edited = dto.Edited = !dto.Published || edited; // if not published, always edited
            db.Documents.Add(dto);
            await db.SaveChangesAsync();

            if (item.ContentType.VariesByCulture())
            {
                (List<ContentVersionCultureVariationDto> contentVariations, List<DocumentCultureVariationDto> entityVariations, editedCultures, _) =
                    await ResolveCultureVariationChangesAsync(item, publishing, isNew: true, editedCultures, dto.CurrentVersion.ContentVersionDto.VersionDate);

                if (contentVariations.Count > 0)
                {
                    db.ContentVersionCultureVariations.AddRange(contentVariations);
                }

                if (entityVariations.Count > 0)
                {
                    db.DocumentCultureVariations.AddRange(entityVariations);
                }

                await db.SaveChangesAsync();
            }

            await OnUowRefreshedEntityAsync(item, CancellationToken.None);

            // Flip the entity's in-memory published state to match what was just persisted — mirrors
            // NPoco's PersistNewItem, and PersistUpdatedItemAsync's equivalent block below. Without this,
            // a caller inspecting the same IContent instance right after SaveAsync returns (rather than
            // re-fetching via GetAsync) would see stale Published/PublishDate/etc. values.
            await ApplyPostPublishFlagFlipsAsync(item);

            item.ResetDirtyProperties();

            return true;
        });

    /// <inheritdoc />
    protected override Task PersistUpdatedItemAsync(IContent item) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var isEntityDirty = item.IsDirty();
            var editedSnapshot = item.Edited;

            if ((item.PublishedState == PublishedState.Published || item.PublishedState == PublishedState.Unpublished)
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
            // we cannot roll a bulk move back anyway. Mirrors NPoco's
            // PublishableContentRepositoryBase.PersistUpdatedItem fast path.
            var isMoving = item.IsMoving();

            var publishing = item.PublishedState == PublishedState.Publishing;

            if (!isMoving)
            {
                if (publishing && item.PublishedVersionId > 0)
                {
                    // The published version is not published anymore — a new one is about to take its place.
                    await db.DocumentVersions.Where(documentVersion => documentVersion.Id == item.PublishedVersionId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(documentVersion => documentVersion.Published, false));
                }

                SanitizeNames(item);
                item.SanitizeEntityPropertiesForXmlStorage();

                if (item.IsPropertyDirty(nameof(item.ParentId)))
                {
                    NodeDto parent = await GetParentNodeDtoAsync(db, item.ParentId);
                    item.Path = string.Concat(parent.Path, ",", item.Id);
                    item.Level = parent.Level + 1;
                    item.SortOrder = await GetNewChildSortOrderAsync(db, item.ParentId, 0);
                }
            }

            DocumentDto dto = BuildEntityDto(item);

            NodeDto nodeDto = dto.ContentDto.NodeDto;
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
                IsolatedCache.Clear(RepositoryCacheKeys.GetGuidKey<IContent>(item.Key));
                return true;
            }

            await db.Content.Where(content => content.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(content => content.ContentTypeId, dto.ContentDto.ContentTypeId));

            ContentVersionDto contentVersionDto = dto.CurrentVersion.ContentVersionDto;
            DocumentVersionDto entityVersionDto = dto.CurrentVersion;

            // Preserve the existing flag — `version` is the ContentVersionDto fetched above for the
            // current-version assertion.
            contentVersionDto.PreventCleanup = version.PreventCleanup;

            await db.ContentVersions.Where(contentVersion => contentVersion.Id == item.VersionId).ExecuteUpdateAsync(setters => setters
                .SetProperty(contentVersion => contentVersion.VersionDate, contentVersionDto.VersionDate)
                .SetProperty(contentVersion => contentVersion.UserId, contentVersionDto.UserId)
                .SetProperty(contentVersion => contentVersion.Current, contentVersionDto.Current)
                .SetProperty(contentVersion => contentVersion.Text, contentVersionDto.Text)
                .SetProperty(contentVersion => contentVersion.PreventCleanup, contentVersionDto.PreventCleanup));

            await db.DocumentVersions.Where(documentVersion => documentVersion.Id == item.VersionId).ExecuteUpdateAsync(setters => setters
                .SetProperty(documentVersion => documentVersion.TemplateId, entityVersionDto.TemplateId)
                .SetProperty(documentVersion => documentVersion.Published, entityVersionDto.Published));

            if (publishing)
            {
                // The row just flipped above (now Current=false, Published=true) becomes the published
                // version; a new draft pair is inserted to take over as the current version. Built as a
                // genuinely new DTO instance (not a mutate-and-reinsert of the tracked one) so a fresh Key
                // can be assigned explicitly — see the equivalent New-path comment in PersistNewVersionsAsync.
                item.PublishedVersionId = item.VersionId;

                var newContentVersionDto = new ContentVersionDto
                {
                    NodeId = item.Id,
                    Key = Guid.NewGuid(),
                    VersionDate = contentVersionDto.VersionDate,
                    UserId = contentVersionDto.UserId,
                    Current = true,
                    Text = item.Name,
                    PreventCleanup = false, // new draft version disregards the existing prevent-cleanup flag
                };
                db.ContentVersions.Add(newContentVersionDto);
                await db.SaveChangesAsync();
                item.VersionId = newContentVersionDto.Id;

                var newDocumentVersionDto = new DocumentVersionDto
                {
                    Id = item.VersionId,
                    TemplateId = entityVersionDto.TemplateId,
                    Published = false,
                    ContentVersionDto = newContentVersionDto,
                };
                db.DocumentVersions.Add(newDocumentVersionDto);
                await db.SaveChangesAsync();

                dto.PublishedVersion = entityVersionDto;
                dto.CurrentVersion = newDocumentVersionDto;
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
                (List<ContentVersionCultureVariationDto> contentVariations, List<DocumentCultureVariationDto> entityVariations, editedCultures, bool cultureEdited) =
                    await ResolveCultureVariationChangesAsync(item, publishing, isNew: false, editedCultures, contentVersionDto.VersionDate);

                if (cultureEdited)
                {
                    edited = true;
                }

                // Replace (rather than update) the content version variations — only for versionToDelete,
                // and the entity variations — for the whole node, unconditionally. Mirrors NPoco's
                // delete-then-reinsert for these two tables (no diff-reconcile needed here, unlike PropertyData).
                await db.ContentVersionCultureVariations.Where(variation => variation.VersionId == versionToDelete).ExecuteDeleteAsync();
                await db.DocumentCultureVariations.Where(variation => variation.NodeId == item.Id).ExecuteDeleteAsync();

                if (contentVariations.Count > 0)
                {
                    db.ContentVersionCultureVariations.AddRange(contentVariations);
                }

                if (entityVariations.Count > 0)
                {
                    db.DocumentCultureVariations.AddRange(entityVariations);
                }

                await db.SaveChangesAsync();
            }

            if (item.PublishedState == PublishedState.Publishing)
            {
                dto.Published = true;
            }
            else if (item.PublishedState == PublishedState.Unpublishing)
            {
                dto.Published = false;
            }

            item.Edited = dto.Edited = !dto.Published || edited; // if not published, always edited

            await db.Documents.Where(document => document.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(document => document.Published, dto.Published)
                .SetProperty(document => document.Edited, dto.Edited));

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
            IsolatedCache.Clear(RepositoryCacheKeys.GetGuidKey<IContent>(item.Key));

            return true;
        });

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

                // ThenBy NodeId breaks ties in variantName (e.g. many nodes sharing a fallback node.Text)
                // so paged results stay stable/non-duplicated across separate fetches — mirrors NPoco's
                // ContentRepositoryBase.PreparePageSql, which unconditionally appends "ORDER BY
                // umbracoNode.id" after any user ordering (see http://issues.umbraco.org/issue/U4-8831).
                var ordered = descending
                    ? withVariantName.OrderByDescending(joined => joined.variantName).ThenBy(joined => joined.node.NodeId)
                    : withVariantName.OrderBy(joined => joined.variantName).ThenBy(joined => joined.node.NodeId);

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

                // ThenBy NodeId breaks ties in variantName (e.g. many nodes sharing a fallback node.Text)
                // so paged results stay stable/non-duplicated across separate fetches — mirrors NPoco's
                // ContentRepositoryBase.PreparePageSql, which unconditionally appends "ORDER BY
                // umbracoNode.id" after any user ordering (see http://issues.umbraco.org/issue/U4-8831).
                var ordered = descending
                    ? withVariantName.OrderByDescending(joined => joined.variantName).ThenBy(joined => joined.node.NodeId)
                    : withVariantName.OrderBy(joined => joined.variantName).ThenBy(joined => joined.node.NodeId);

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
        AmbientScope.ExecuteWithContextAsync<IEnumerable<IContent>>(async db =>
        {
            // Mirrors NPoco's ContentRepositoryBase.GetRecycleBin: every trashed node of this object type,
            // regardless of tree depth — not just the direct children of the recycle bin folder itself.
            var publishedSubquery = db.ContentVersions
                .Join(
                    db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                    contentVersion => contentVersion.Id,
                    documentVersion => documentVersion.Id,
                    (contentVersion, documentVersion) => new { contentVersion, documentVersion });

            List<DocumentRow> rows = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed)
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
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return Enumerable.Empty<IContent>();
            }

            return await AssembleEntitiesAsync(rows, db);
        });

    /// <inheritdoc />
    public override Task<PagedModel<IContent>> GetPagedRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed)
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

            // The ContentTypes JOIN is always included so contentTypeAlias is available for ordering,
            // mirroring GetChildrenCoreAsync/GetDescendantsCoreAsync — only the leading node predicate
            // (Trashed instead of ParentId/Path) differs from those two.
            var baseQuery = db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Trashed)
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

            List<IContent> items = await AssembleEntitiesAsync(rows, db);

            async Task<IReadOnlyList<DocumentRow>> FetchCustomFieldOrdered()
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

                // ThenBy NodeId breaks ties in variantName (e.g. many nodes sharing a fallback node.Text)
                // so paged results stay stable/non-duplicated across separate fetches — mirrors NPoco's
                // ContentRepositoryBase.PreparePageSql, which unconditionally appends "ORDER BY
                // umbracoNode.id" after any user ordering (see http://issues.umbraco.org/issue/U4-8831).
                var ordered = descending
                    ? withVariantName.OrderByDescending(joined => joined.variantName).ThenBy(joined => joined.node.NodeId)
                    : withVariantName.OrderBy(joined => joined.variantName).ThenBy(joined => joined.node.NodeId);

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
    protected override Task OnUowRefreshedEntityAsync(IContent entity, CancellationToken cancellationToken)
    {
        // ContentRefreshNotification is [Obsolete] ("use saved notifications instead") but is still the ONLY
        // live signal CacheRefreshingNotificationHandler (Umbraco.PublishedCache.HybridCache) listens for to
        // refresh the published-content cache — NPoco's DocumentRepository.OnUowRefreshedEntity fires the exact
        // same obsolete notification today, in production. Skipping it would silently stop HybridCache from
        // refreshing for EF-Core-saved documents. Mirror NPoco's current (also-obsolete) behavior faithfully.
#pragma warning disable CS0618 // Type or member is obsolete
        EventAggregator.Publish(new ContentRefreshNotification(entity, new EventMessages()));
#pragma warning restore CS0618
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override IContent BuildEntity(DocumentDto entityDto, IContentType? contentType) =>
        ContentBaseFactory.BuildEntity(entityDto, contentType);

    /// <inheritdoc />
    protected override DocumentDto BuildEntityDto(IContent entity) =>
        ContentBaseFactory.BuildDocumentDto(entity, NodeObjectTypeKey, entity.PublishedState == PublishedState.Publishing);

    /// <inheritdoc />
    public Task ReplaceContentPermissionsAsync(EntityPermissionSet permissionSet, CancellationToken cancellationToken) =>
        _permissionRepository.ReplaceEntityPermissionsAsync(permissionSet, cancellationToken);

    /// <inheritdoc />
    public Task AssignEntityPermissionAsync(IContent entity, string permission, IEnumerable<Guid> groupKeys, CancellationToken cancellationToken) =>
        _permissionRepository.AssignEntityPermissionAsync(entity, permission, groupKeys, cancellationToken);

    /// <inheritdoc />
    public Task<EntityPermissionCollection> GetPermissionsForEntityAsync(Guid entityKey, CancellationToken cancellationToken) =>
        _permissionRepository.GetPermissionsForEntityAsync(entityKey, cancellationToken);

    /// <inheritdoc />
    public Task AddOrUpdatePermissionsAsync(ContentPermissionSet permission, CancellationToken cancellationToken) =>
        _permissionRepository.AddOrUpdatePermissionsAsync(permission, cancellationToken);

    /// <inheritdoc />
    // Ported from NPoco's DocumentRepository.IsPathPublished, with one deliberate fix: that version has a
    // latent null-dereference for a null content parameter (the fast-fail/succeed-fast null-conditional
    // checks both fall through without returning, then it crashes unguarded on content.Path.Split) — this
    // version returns false for null up front instead of reproducing that bug.
    public override Task<bool> IsPathPublishedAsync(IContent? content, CancellationToken cancellationToken)
    {
        if (content is null || content.Path.StartsWith($"{Constants.System.Root},{Constants.System.RecycleBinContent},", StringComparison.Ordinal))
        {
            return Task.FromResult(false);
        }

        if (content.ParentId == Constants.System.Root)
        {
            return Task.FromResult(content.Published);
        }

        List<int> ancestorIds = content.Path.Split(',').Skip(1)
            .Select(s => int.Parse(s, CultureInfo.InvariantCulture)).ToList();

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int publishedAncestorCount = await PublishedNodes(db)
                .Where(node => ancestorIds.Contains(node.NodeId))
                .CountAsync(cancellationToken);

            return publishedAncestorCount == content.Level;
        });
    }

    /// <inheritdoc />
    // Checks for a direct child of the recycle bin folder node itself (Constants.System.RecycleBinContent),
    // not "any trashed node anywhere" — mirrors NPoco's PublishableContentRepositoryBase.RecycleBinSmells,
    // which is CountChildren(RecycleBinId) > 0. Deliberately does not replicate NPoco's IAppPolicyCache
    // wrapper: IAppPolicyCache.Get only accepts a synchronous factory, and wrapping this async query in one
    // would mean sync-over-async (GetAwaiter().GetResult()) — the exact anti-pattern already avoided
    // elsewhere in this file for language/tag lookups.
    public Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(db =>
            db.Nodes.AnyAsync(
                node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == Constants.System.RecycleBinContent,
                cancellationToken));

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
    /// <summary>
    ///     Applies document paging/ordering to a query, breaking ties on node id for stable paging.
    /// </summary>
    /// <remarks>
    ///     Internal (not private) so AsyncDocumentRepositoryOrderingTests can exercise the tiebreak logic
    ///     directly against an in-memory sequence, decoupling it from a real database's incidental row order.
    /// </remarks>
    internal static IOrderedQueryable<T> ApplyDocumentOrdering<T>(
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
        string? orderBy = ordering?.OrderBy?.ToLowerInvariant();
        IOrderedQueryable<T> ordered = orderBy switch
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

        // Break ties on node id so paged results stay stable/non-duplicated across separate fetches —
        // mirrors NPoco's ContentRepositoryBase.PreparePageSql, which unconditionally appends "ORDER BY
        // umbracoNode.id" after any user ordering (see http://issues.umbraco.org/issue/U4-8831). Skipped
        // when already ordering by id, since that's already unique.
        return orderBy == "id" ? ordered : ordered.ThenBy(idSelector);
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

    // Flips the entity's in-memory published state to match what was just persisted — shared by
    // PersistNewItemAsync and PersistUpdatedItemAsync. Mirrors NPoco's PersistNewItem/PersistUpdatedItem.
    private async Task ApplyPostPublishFlagFlipsAsync(IContent item)
    {
        if (item.PublishedState == PublishedState.Publishing)
        {
            item.Published = true;
            item.PublishTemplateId = item.TemplateId;
            item.PublisherId = item.WriterId;
            item.PublishName = item.Name;
            item.PublishDate = item.UpdateDate;

            await SetEntityTagsAsync(item);
        }
        else if (item.PublishedState == PublishedState.Unpublishing)
        {
            item.Published = false;
            item.PublishTemplateId = null;
            item.PublisherId = null;
            item.PublishName = null;
            item.PublishDate = null;

            ClearEntityTags(item);
        }
    }

    // Updates tags for an item. Ported from ContentRepositoryBase.SetEntityTags, with one behavioral
    // fix: the culture-to-language-id lookup is properly awaited here instead of using
    // GetAwaiter().GetResult() (safe in the NPoco base class only because callers there are synchronous).
    private async Task SetEntityTagsAsync(IContent entity)
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
                if (tagConfiguration == null)
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

    // Clears tags for an item. Ported from ContentRepositoryBase.ClearEntityTags — a plain synchronous
    // call, no scope/await concerns.
    private void ClearEntityTags(IContent entity) => _tagRepository.RemoveAll(entity.Id);

    private static void AssignDefaultTemplateIfMissing(IContent entity)
    {
        if (entity.TemplateId.HasValue is false)
        {
            entity.TemplateId = entity.ContentType.DefaultTemplate?.Id;
        }
    }

    // Scoped-down stand-in for NPoco's PublishableContentRepositoryBase.SanitizeNames: this phase is
    // invariant-only, so only the "must have a name at all" check applies here.
    private static void SanitizeNames(IContent entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new InvalidOperationException("Cannot save content with an empty name.");
        }
    }

    // Inserts (or converts a reserved placeholder into) the umbracoNode row, resolving Path/Level/SortOrder
    // from the parent, then patches the Path once the real NodeId is known. Mirrors NPoco's PersistNewItem
    // node-handling block.
    private async Task PersistNewNodeAsync(UmbracoDbContext db, IContent item, DocumentDto dto)
    {
        NodeDto parent = await GetParentNodeDtoAsync(db, item.ParentId);
        var level = parent.Level + 1;

        var calculateSortOrder = (item is { HasIdentity: false, SortOrder: 0 } && item.IsPropertyDirty(nameof(item.SortOrder)) is false)
                                  || await SortorderExistsAsync(db, item.ParentId, item.SortOrder);
        var sortOrder = calculateSortOrder ? await GetNewChildSortOrderAsync(db, item.ParentId, 0) : item.SortOrder;

        NodeDto nodeDto = dto.ContentDto.NodeDto;
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
    }

    private static async Task PersistNewContentAsync(UmbracoDbContext db, IContent item, DocumentDto dto)
    {
        ContentDto contentDto = dto.ContentDto;
        contentDto.NodeId = item.Id;
        db.Content.Add(contentDto);
        await db.SaveChangesAsync();
    }

    private static async Task PersistNewVersionsAsync(UmbracoDbContext db, IContent item, DocumentDto dto)
    {
        ContentVersionDto contentVersionDto = dto.CurrentVersion.ContentVersionDto;
        contentVersionDto.NodeId = item.Id;
        db.ContentVersions.Add(contentVersionDto);
        await db.SaveChangesAsync();
        item.VersionId = contentVersionDto.Id;

        DocumentVersionDto documentVersionDto = dto.CurrentVersion;
        documentVersionDto.Id = item.VersionId;
        db.DocumentVersions.Add(documentVersionDto);
        await db.SaveChangesAsync();

        if (item.PublishedState == PublishedState.Publishing)
        {
            // The pair just inserted (Current=false, Published=true) becomes the published version.
            // A second (Current=true, Published=false) pair is inserted for the new draft — built as a
            // genuinely new DTO instance (not a mutate-and-reinsert of the first) so a fresh Key can be
            // assigned explicitly. Unlike NPoco's ContentVersion table, the EF Core ContentVersionDto.Key
            // column has no DB-side default, so omitting this would silently duplicate the first row's Key.
            item.PublishedVersionId = item.VersionId;
            dto.PublishedVersion = documentVersionDto;

            var newContentVersionDto = new ContentVersionDto
            {
                NodeId = item.Id,
                Key = Guid.NewGuid(),
                VersionDate = contentVersionDto.VersionDate,
                UserId = contentVersionDto.UserId,
                Current = true,
                Text = item.Name,
            };
            db.ContentVersions.Add(newContentVersionDto);
            await db.SaveChangesAsync();
            item.VersionId = newContentVersionDto.Id;

            var newDocumentVersionDto = new DocumentVersionDto
            {
                Id = item.VersionId,
                TemplateId = documentVersionDto.TemplateId,
                Published = false,
                ContentVersionDto = newContentVersionDto,
            };
            db.DocumentVersions.Add(newDocumentVersionDto);
            await db.SaveChangesAsync();

            dto.CurrentVersion = newDocumentVersionDto;
        }
    }

    private async Task<(bool Edited, HashSet<string>? EditedCultures)> PersistNewPropertyDataAsync(UmbracoDbContext db, IContent item)
    {
        List<PropertyDataDto> propertyDataDtos = PropertyFactory.BuildEFCoreDtos(
            item.ContentType.Variations,
            item.VersionId,
            item.PublishedVersionId,
            item.Properties,
            LanguageRepository,
            out bool edited,
            out HashSet<string>? editedCultures).ToList();

        SetEntitySortableValues(item, propertyDataDtos);

        if (propertyDataDtos.Count > 0)
        {
            db.PropertyData.AddRange(propertyDataDtos);
            await db.SaveChangesAsync();
        }

        return (edited, editedCultures);
    }

    // Populates PropertyDataDto.SortableValue for any property whose editor implements IDataValueSortable,
    // so custom-field ordering (see ResolveCustomFieldOrderedNodeIdsAsync) can prioritize it over the raw
    // typed columns. Mirrors NPoco's ContentRepositoryBase.SetEntitySortableValues exactly.
    private void SetEntitySortableValues(IContentBase entity, IEnumerable<PropertyDataDto> propertyDtos)
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

    // Diff-reconciles PropertyData rows for the given version against the freshly built values for the
    // entity's current properties: matches existing rows by (PropertyTypeId, VersionId, LanguageId,
    // Segment), updates matches in place, inserts new rows for unmatched values, and deletes rows that no
    // longer have a corresponding value. Mirrors NPoco's ContentRepositoryBase.ReplacePropertyValues,
    // omitting NPoco's row-level pessimistic lock (ForUpdate()/UPDLOCK) on the fetch. This is safe because
    // every caller reaches this method through ContentService/PublishableContentServiceBase, which always
    // acquires the global Constants.Locks.ContentTree write lock before saving/publishing — a real
    // cross-server database lock (see IDistributedLockingMechanism.WriteLock), not just an in-process one.
    // EFCoreScope shares the ambient NPoco scope's physical connection/transaction
    // (_shareUmbracoConnection = true), so that lock already serializes this read-then-write sequence
    // across servers; a per-row UPDLOCK here would be redundant defense-in-depth, not a fix for a real race.
    // Note that NPoco's own ForUpdate() was itself narrow, not a strong guarantee being weakened here: per
    // its doc comment it "will not work for all queries, only simple ones" — it patches only the first
    // FROM-prefixed SQL fragment in the query (SqlServerSyntaxProvider.InsertForUpdateHint) and silently
    // no-ops if no such fragment is found, so it never handled joins, multiple FROMs, or aliased tables
    // robustly either.
    private async Task<(bool Edited, HashSet<string>? EditedCultures)> PersistUpdatedPropertyDataAsync(
        UmbracoDbContext db, IContent item, int versionId, int publishedVersionId)
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

        List<PropertyDataDto> propertyDataDtos = PropertyFactory.BuildEFCoreDtos(
            item.ContentType.Variations,
            item.VersionId,
            publishedVersionId,
            item.Properties,
            LanguageRepository,
            out bool edited,
            out HashSet<string>? editedCultures).ToList();

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
        // 2100-parameter convention (Infrastructure/CLAUDE.md §6) since a document with many
        // culture/segment-variant properties can plausibly exceed it.
        if (existingPropertyDataIds.Count > 0)
        {
            foreach (IEnumerable<int> batch in existingPropertyDataIds.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                List<int> batchIds = batch.ToList();
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
        List<DocumentCultureVariationDto> EntityVariations,
        HashSet<string>? EditedCultures,
        bool Edited)> ResolveCultureVariationChangesAsync(
            IContent item, bool publishing, bool isNew, HashSet<string>? editedCultures, DateTime versionDate)
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
        List<DocumentCultureVariationDto> entityVariations = await BuildEntityVariationDtosAsync(item, editedCultures!);

        return (contentVariations, entityVariations, editedCultures, edited);
    }

    // Builds ContentVersionCultureVariationDto rows for the 'current' (non-published) version, one per
    // culture with a name, plus — only when publishing — additional rows for the 'published' version's
    // published cultures. Mirrors PublishableContentRepositoryBase.GetContentVariationDtos, but awaits
    // LanguageRepository properly instead of that method's sync-over-async GetAwaiter().GetResult() calls,
    // since this helper is genuinely async (no non-async NPoco caller to match).
    private async Task<List<ContentVersionCultureVariationDto>> BuildContentVariationDtosAsync(IContent entity, bool publishing)
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
                    Name = cultureInfo.Name,
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
                    Name = cultureInfo.Name,
                    UpdateDate = entity.GetPublishDate(cultureInfo.Culture) ?? DateTime.MinValue,
                });
            }
        }

        return dtos;
    }

    // Builds one DocumentCultureVariationDto per culture the entity is available or published in. Mirrors
    // PublishableContentRepositoryBase.GetEntityVariationDtos field-by-field — the EF Core DTO carries only
    // LanguageId (no in-memory-only Culture convenience field like the NPoco DTO), so Culture is not set here.
    private async Task<List<DocumentCultureVariationDto>> BuildEntityVariationDtosAsync(IContent entity, HashSet<string>? editedCultures)
    {
        var dtos = new List<DocumentCultureVariationDto>();

        IEnumerable<string> allCultures = entity.AvailableCultures.Union(entity.PublishedCultures); // union = distinct
        foreach (string culture in allCultures)
        {
            int languageId = await LanguageRepository.GetIdByIsoCodeAsync(culture)
                ?? throw new InvalidOperationException("Not a valid culture.");

            dtos.Add(new DocumentCultureVariationDto
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

    private static Task<NodeDto> GetParentNodeDtoAsync(UmbracoDbContext db, int parentId) =>
        db.Nodes.FirstAsync(node => node.NodeId == parentId);

    private Task<bool> SortorderExistsAsync(UmbracoDbContext db, int parentId, int sortOrder) =>
        db.Nodes.AnyAsync(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentId && node.SortOrder == sortOrder);

    private async Task<int> GetNewChildSortOrderAsync(UmbracoDbContext db, int parentId, int first)
    {
        int? maxSortOrder = await db.Nodes
            .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == parentId)
            .Select(node => (int?)node.SortOrder)
            .MaxAsync();

        return maxSortOrder + 1 ?? first;
    }

    private static async Task<int> GetReservedIdAsync(UmbracoDbContext db, Guid uniqueId)
    {
        int? id = await db.Nodes
            .Where(node => node.UniqueId == uniqueId && node.NodeObjectType == Constants.ObjectTypes.IdReservation)
            .Select(node => (int?)node.NodeId)
            .FirstOrDefaultAsync();

        return id ?? 0;
    }

    // Quick sanity check that a freshly built Path is well-formed, mirroring NPoco's
    // NodeDto.ValidatePathWithException (Persistence/Models/PathValidationExtensions.cs), which is
    // defined against the NPoco NodeDto type and so cannot be reused directly against the EF Core one.
    private static void ValidatePath(NodeDto node)
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
