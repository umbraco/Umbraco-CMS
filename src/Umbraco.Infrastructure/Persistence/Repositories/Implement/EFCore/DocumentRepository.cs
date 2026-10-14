using System.Globalization;
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
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     Provides an EF Core backed async repository for <see cref="IContent" /> document entities.
/// </summary>
internal class DocumentRepository
    : AsyncPublishableContentRepositoryBase<
        IContent,
        DocumentRepository,
        DocumentDto,
        DocumentVersionDto,
        DocumentCultureVariationDto>,
      IDocumentRepository
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ITagRepository _tagRepository;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly AsyncPermissionRepository<IContent> _permissionRepository;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentRepository" /> class.
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
    /// <param name="shortStringHelper">The short string helper, used to detect URL segment collisions between sibling names.</param>
    public DocumentRepository(
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
        IShortStringHelper shortStringHelper)
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
        _permissionRepository = new AsyncPermissionRepository<IContent>(scopeAccessor, appCaches);
        _shortStringHelper = shortStringHelper;
    }

    /// <inheritdoc />
    public override Guid RecycleBinKey => Constants.System.RecycleBinContentKey;

    /// <inheritdoc />
    protected override Guid NodeObjectTypeKey => Constants.ObjectTypes.Document;

    /// <inheritdoc />
    protected override DocumentRepository This => this;

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IContent item) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            item.AddingEntity();
            var publishing = item is { PublishedState: PublishedState.Publishing };

            AssignDefaultTemplateIfMissing(item);

            await SanitizeNamesAsync(db, item, publishing);

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
                    await db.DocumentVersions.Where(documentVersion => documentVersion.Id == item.PublishedVersionId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(documentVersion => documentVersion.Published, false));
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
                IsolatedCache.Clear(EntityTypeCacheKeyPrefix + item.Key);
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
                // and the entity variations — for the whole node, unconditionally. These two tables are
                // deleted and reinserted rather than diff-reconciled, unlike PropertyData.
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

            if (item is { PublishedState: PublishedState.Publishing })
            {
                dto.Published = true;
            }
            else if (item is { PublishedState: PublishedState.Unpublishing })
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
            IsolatedCache.Clear(EntityTypeCacheKeyPrefix + item.Key);

            return true;
        });

    /// <inheritdoc />
    protected override Task PersistEntitySpecificDeleteClausesAsync(UmbracoDbContext db, int nodeId)
    {
        IQueryable<Guid> uniqueIdQuery = db.Nodes.Where(node => node.NodeId == nodeId).Select(node => node.UniqueId);

        return PersistEntitySpecificDeleteClausesCoreAsync(db, nodeId, uniqueIdQuery);
    }

    private static async Task PersistEntitySpecificDeleteClausesCoreAsync(UmbracoDbContext db, int nodeId, IQueryable<Guid> uniqueIdQuery)
    {
        await db.RedirectUrls.Where(redirectUrl => uniqueIdQuery.Contains(redirectUrl.ContentKey)).ExecuteDeleteAsync();

        await db.UserGroups
            .Where(userGroup => userGroup.StartContentId == nodeId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(userGroup => userGroup.StartContentId, (int?)null));

        await db.Domains.Where(domain => domain.RootStructureId == nodeId).ExecuteDeleteAsync();
        await db.Documents.Where(document => document.NodeId == nodeId).ExecuteDeleteAsync();
        await db.DocumentCultureVariations.Where(cultureVariation => cultureVariation.NodeId == nodeId).ExecuteDeleteAsync();

        IQueryable<int> versionIdQuery = db.ContentVersions.Where(contentVersion => contentVersion.NodeId == nodeId).Select(contentVersion => contentVersion.Id);
        await db.DocumentVersions.Where(documentVersion => versionIdQuery.Contains(documentVersion.Id)).ExecuteDeleteAsync();

        IQueryable<Guid> accessIdQuery = db.Access
            .Where(access => access.NodeId == nodeId || access.LoginNodeId == nodeId || access.NoAccessNodeId == nodeId)
            .Select(access => access.Id);
        await db.AccessRules.Where(accessRule => accessIdQuery.Contains(accessRule.AccessId)).ExecuteDeleteAsync();

        await db.Access.Where(access => access.NodeId == nodeId).ExecuteDeleteAsync();
        await db.Access.Where(access => access.LoginNodeId == nodeId).ExecuteDeleteAsync();
        await db.Access.Where(access => access.NoAccessNodeId == nodeId).ExecuteDeleteAsync();

        await db.DocumentUrls.Where(documentUrl => uniqueIdQuery.Contains(documentUrl.UniqueId)).ExecuteDeleteAsync();
        await db.DocumentUrlAliases.Where(documentUrlAlias => uniqueIdQuery.Contains(documentUrlAlias.UniqueId)).ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    protected override string RecycleBinCacheKey => CacheKeys.ContentRecycleBinCacheKey;

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetChildrenWithoutTemplatesAsync(
        Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken)
        => GetChildrenCoreAsync(parentKey, skip, take, propertyAliases, ordering, loadTemplates: false, cancellationToken);

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetDescendantsWithoutTemplatesAsync(
        Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true)
        => GetDescendantsCoreAsync(ancestorKey, skip, take, ordering, loadTemplates: false, includeTrashed, cancellationToken);

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ordering);

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int total = await db.Nodes
                .Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Level == level && !node.Trashed)
                .CountAsync(cancellationToken);

            if (total == 0)
            {
                return new PagedModel<IContent> { Total = 0, Items = [] };
            }

            // Mirrors GetPagedRecycleBinAsync — only the leading node predicate (Level == level plus the
            // same !Trashed exclusion the old sync ContentService.GetByLevel documented — "contrary to most
            // methods, this method filters out trashed content items") differs from that method.
            IQueryable<ContentJoinRow<DocumentDto, DocumentVersionDto>> baseQuery = BuildBaseQuery(
                db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && node.Level == level && !node.Trashed));

            bool isCustomFieldOrdering = ordering is { IsCustomField: true };
            bool isCultureNameOrdering =
                !isCustomFieldOrdering
                && ordering?.OrderBy?.Equals("name", StringComparison.OrdinalIgnoreCase) is true
                && ordering is { IsInvariant: false };

            IReadOnlyList<ContentRow<DocumentDto, DocumentVersionDto>> rows = isCustomFieldOrdering
                ? await FetchCustomFieldOrdered()
                : isCultureNameOrdering
                    ? await FetchCultureNameOrdered()
                    : await FetchDefaultOrdered();

            if (rows.Count == 0)
            {
                return new PagedModel<IContent> { Total = total, Items = [] };
            }

            List<IContent> items = await AssembleEntitiesAsync(db, rows, propertyAliases: null, loadTemplates: true, cancellationToken);

            async Task<IReadOnlyList<ContentRow<DocumentDto, DocumentVersionDto>>> FetchCustomFieldOrdered()
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

            Task<IReadOnlyList<ContentRow<DocumentDto, DocumentVersionDto>>> FetchCultureNameOrdered() =>
                FetchCultureNameOrderedAsync(db, baseQuery, ordering!, skip, take, cancellationToken);

            async Task<IReadOnlyList<ContentRow<DocumentDto, DocumentVersionDto>>> FetchDefaultOrdered()
            {
                IOrderedQueryable<ContentJoinRow<DocumentDto, DocumentVersionDto>> orderedQuery = ApplyOrdering(baseQuery, db.Users, ordering);

                return await orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(ToContentRow)
                    .ToListAsync(cancellationToken);
            }

            return new PagedModel<IContent> { Total = total, Items = items };
        });
    }

    /// <inheritdoc />
    public Task<PagedModel<IContent>> GetAncestorsAsync(Guid key, int skip, int take, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var self = await db.Nodes
                .Where(node => node.UniqueId == key)
                .Select(node => new { node.NodeId, node.Path })
                .FirstOrDefaultAsync(cancellationToken);

            if (self is null)
            {
                return new PagedModel<IContent> { Total = 0, Items = [] };
            }

            // Path is stored root-first (e.g. "-1,1063,1066,1092"), so splitting it already yields the
            // ancestors in root-first order — excluding the root node itself and the node whose ancestors
            // are being requested, mirroring ContentExtensions.GetAncestorIds().
            List<int> ancestorNodeIds = [.. self.Path.Split(',')
                .Select(segment => int.Parse(segment, CultureInfo.InvariantCulture))
                .Where(nodeId => nodeId != Constants.System.Root && nodeId != self.NodeId)];

            int total = ancestorNodeIds.Count;
            if (total == 0)
            {
                return new PagedModel<IContent> { Total = 0, Items = [] };
            }

            List<int> pageNodeIds = [.. ancestorNodeIds.Skip(skip).Take(take)];
            if (pageNodeIds.Count == 0)
            {
                return new PagedModel<IContent> { Total = total, Items = [] };
            }

            List<ContentRow<DocumentDto, DocumentVersionDto>> rows = await BuildBaseQuery(
                    db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey && pageNodeIds.Contains(node.NodeId)))
                .Select(ToContentRow)
                .ToListAsync(cancellationToken);

            // The Contains-filtered fetch above does not preserve pageNodeIds' root-first order, so
            // re-sort in memory to match it before assembling entities.
            Dictionary<int, int> position = pageNodeIds
                .Select((nodeId, index) => (nodeId, index))
                .ToDictionary(x => x.nodeId, x => x.index);
            List<ContentRow<DocumentDto, DocumentVersionDto>> orderedRows = [.. rows.OrderBy(row => position[row.Node.NodeId])];

            List<IContent> items = await AssembleEntitiesAsync(db, orderedRows, propertyAliases: null, loadTemplates: true, cancellationToken);
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
        ContentBaseFactory.BuildDocumentDto(entity, NodeObjectTypeKey, entity is { PublishedState: PublishedState.Publishing });

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

        List<int> ancestorIds = [.. content.Path.Split(',').Skip(1).Select(s => int.Parse(s, CultureInfo.InvariantCulture))];

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            int publishedAncestorCount = await PublishedNodes(db)
                .Where(node => ancestorIds.Contains(node.NodeId))
                .CountAsync(cancellationToken);

            return publishedAncestorCount == content.Level;
        });
    }

    /// <inheritdoc />
    // A direct child of the recycle bin node is what makes the bin non-empty; anything trashed deeper down
    // has such an ancestor. The cached answer is read and then inserted in two steps rather than through
    // IAppPolicyCache.Get, whose factory is synchronous and would force this query to block on its result.
    public async Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken)
    {
        // Answered on every backoffice tree render, so it is cached. ContentCacheRefresher clears this key
        // on refresh, so the answer cannot outlive a change to what is in the bin.
        var cached = AppCaches.RuntimeCache.GetCacheItem<bool?>(RecycleBinCacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var smells = await AmbientScope.ExecuteWithContextAsync(db =>
            db.Nodes.AnyAsync(
                node => node.NodeObjectType == NodeObjectTypeKey && node.ParentId == Constants.System.RecycleBinContent,
                cancellationToken));

        AppCaches.RuntimeCache.Insert(RecycleBinCacheKey, () => (bool?)smells);
        return smells;
    }

    /// <inheritdoc />
    protected override IQueryable<ContentJoinRow<DocumentDto, DocumentVersionDto>> BuildBaseQuery(
        UmbracoDbContext db,
        IQueryable<NodeDto> nodes,
        IQueryable<ContentVersionDto> contentVersions)
    {
        var publishedSubquery = db.ContentVersions
            .Join(
                db.DocumentVersions.Where(documentVersion => documentVersion.Published),
                contentVersion => contentVersion.Id,
                documentVersion => documentVersion.Id,
                (contentVersion, documentVersion) => new { contentVersion, documentVersion });

        return nodes
            .Join(contentVersions, node => node.NodeId, contentVersion => contentVersion.NodeId, (node, contentVersion) => new { node, contentVersion })
            .Join(db.Documents, joined => joined.node.NodeId, document => document.NodeId, (joined, document) => new { joined.node, joined.contentVersion, document })
            .Join(db.Content, joined => joined.node.NodeId, content => content.NodeId, (joined, content) => new { joined.node, joined.contentVersion, joined.document, content })
            .Join(db.DocumentVersions, joined => joined.contentVersion.Id, documentVersion => documentVersion.Id, (joined, documentVersion) => new { joined.node, joined.contentVersion, joined.document, joined.content, documentVersion })
            .GroupJoin(db.ContentTypes, joined => joined.content.ContentTypeId, contentType => contentType.NodeId, (joined, contentTypeGroup) => new { joined.node, joined.contentVersion, joined.document, joined.content, joined.documentVersion, contentTypeGroup })
            .SelectMany(joined => joined.contentTypeGroup.DefaultIfEmpty(), (joined, contentType) => new { joined.node, joined.contentVersion, joined.document, joined.content, joined.documentVersion, contentType })
            .GroupJoin(publishedSubquery, joined => joined.node.NodeId, publishedVersion => publishedVersion.contentVersion.NodeId, (joined, publishedVersionGroup) => new { joined.node, joined.contentVersion, joined.document, joined.content, joined.documentVersion, joined.contentType, publishedVersionGroup })
            .SelectMany(joined => joined.publishedVersionGroup.DefaultIfEmpty(), (joined, publishedVersion) => new { joined.node, joined.contentVersion, joined.document, joined.content, joined.documentVersion, joined.contentType, publishedVersion })
            .GroupJoin(db.Nodes, joined => joined.node.ParentId, parentNode => parentNode.NodeId, (joined, parentNodeGroup) => new { joined.node, joined.contentVersion, joined.document, joined.content, joined.documentVersion, joined.contentType, joined.publishedVersion, parentNodeGroup })
            .SelectMany(joined => joined.parentNodeGroup.DefaultIfEmpty(), (joined, parentNode) => new { joined.node, joined.contentVersion, joined.document, joined.content, joined.documentVersion, joined.contentType, joined.publishedVersion, parentNode })
            .Select(joined => new ContentJoinRow<DocumentDto, DocumentVersionDto>
            {
                Node = joined.node,
                Entity = joined.document,
                Content = joined.content,
                ContentVersion = joined.contentVersion,
                EntityVersion = joined.documentVersion,
                ContentType = joined.contentType,
                PublishedContentVersion = joined.publishedVersion != null ? joined.publishedVersion.contentVersion : null,
                PublishedEntityVersion = joined.publishedVersion != null ? joined.publishedVersion.documentVersion : null,
                ParentNode = joined.parentNode,
            });
    }

    /// <inheritdoc />
    protected override async Task OnEntitiesAssembledAsync(
        UmbracoDbContext db,
        IReadOnlyList<IContent> entities,
        IReadOnlyList<ContentRow<DocumentDto, DocumentVersionDto>> rows,
        bool loadTemplates,
        CancellationToken cancellationToken)
    {
        if (!loadTemplates)
        {
            return;
        }

        // Resolve valid template IDs once for the whole batch (mirrors NPoco AddAdditionalTempContentMapping).
        HashSet<int> validTemplateIds = await ResolveValidTemplateIdsAsync(rows, cancellationToken);

        for (var i = 0; i < entities.Count; i++)
        {
            IContent entity = entities[i];
            ContentRow<DocumentDto, DocumentVersionDto> row = rows[i];

            int? draftTemplateId = row.EntityVersion.TemplateId;
            if (draftTemplateId.HasValue && validTemplateIds.Contains(draftTemplateId.Value))
            {
                entity.TemplateId = draftTemplateId;
            }

            int? publishedTemplateId = row.PublishedEntityVersion?.TemplateId;
            if (publishedTemplateId.HasValue && validTemplateIds.Contains(publishedTemplateId.Value))
            {
                entity.PublishTemplateId = publishedTemplateId;
            }
        }
    }

    private async Task<HashSet<int>> ResolveValidTemplateIdsAsync(IReadOnlyList<ContentRow<DocumentDto, DocumentVersionDto>> rows, CancellationToken cancellationToken)
    {
        var templateIds = rows
            .SelectMany(row => new[] { row.EntityVersion.TemplateId, row.PublishedEntityVersion?.TemplateId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        if (templateIds.Count == 0)
        {
            return [];
        }

        // Templates are fully cached, so validating against all of them avoids a query per batch.
        IEnumerable<ITemplate> allTemplates = await _templateRepository.GetAllAsync(cancellationToken);
        return [.. allTemplates.Select(template => template.Id).Where(templateIds.Contains)];
    }

    // Flips the entity's in-memory published state to match what was just persisted — shared by
    // PersistNewItemAsync and PersistUpdatedItemAsync.
    private async Task ApplyPostPublishFlagFlipsAsync(IContent item)
    {
        if (item is { PublishedState: PublishedState.Publishing })
        {
            item.Published = true;
            item.PublishTemplateId = item.TemplateId;
            item.PublisherId = item.WriterId;
            item.PublishName = item.Name;
            item.PublishDate = item.UpdateDate;

            await SetEntityTagsAsync(item);
        }
        else if (item is { PublishedState: PublishedState.Unpublishing })
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

    // Clears tags for an item. Synchronous: there is no scope or await concern here.
    private void ClearEntityTags(IContent entity) => _tagRepository.RemoveAll(entity.Id);

    private static void AssignDefaultTemplateIfMissing(IContent entity)
    {
        if (entity.TemplateId.HasValue is false)
        {
            entity.TemplateId = entity.ContentType.DefaultTemplate?.Id;
        }
    }

    /// <inheritdoc />
    // Adds URL segment collision detection on top of the base's literal-duplicate-name check — resolves
    // https://github.com/umbraco/Umbraco-CMS/issues/22070 for the EF Core path too.
    protected override async Task<string?> EnsureUniqueNodeNameAsync(UmbracoDbContext db, int parentId, string? nodeName, int id)
    {
        if (!EnsureUniqueNaming)
        {
            return nodeName;
        }

        (string? uniqueName, List<SimilarNodeName> siblings) = await GetUniqueNodeNameAndSiblingsAsync(db, parentId, nodeName, id);
        return EnsureUniqueUrlSegment(uniqueName, id, siblings, _shortStringHelper);
    }

    /// <inheritdoc />
    protected override Task<string?> EnsureUniqueVariantNameAsync(string? nodeName, int nodeId, List<SimilarNodeName> siblings, string culture) =>
        Task.FromResult(EnsureUniqueUrlSegment(nodeName, nodeId, siblings, _shortStringHelper, culture));

    /// <summary>
    /// Ensures the proposed name produces a URL segment that is unique among sibling URL segments.
    /// If a collision is detected (e.g. "Title" and "Title." both produce segment "title"),
    /// a numeric suffix is appended to the name until uniqueness is achieved.
    /// </summary>
    internal static string? EnsureUniqueUrlSegment(
        string? nodeName,
        int nodeId,
        IEnumerable<SimilarNodeName> siblings,
        IShortStringHelper shortStringHelper,
        string? culture = null)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return nodeName;
        }

        var proposedSegment = shortStringHelper.CleanStringForUrlSegment(nodeName, culture);
        if (string.IsNullOrEmpty(proposedSegment))
        {
            return nodeName;
        }

        // Build a set of URL segments from siblings, excluding the current node.
        var siblingSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (SimilarNodeName sibling in siblings)
        {
            if (sibling.Id == nodeId || string.IsNullOrWhiteSpace(sibling.Name))
            {
                continue;
            }

            var segment = shortStringHelper.CleanStringForUrlSegment(sibling.Name, culture);
            if (string.IsNullOrEmpty(segment) is false)
            {
                siblingSegments.Add(segment);
            }
        }

        // If the proposed segment doesn't collide, return the name as-is.
        if (siblingSegments.Contains(proposedSegment) is false)
        {
            return nodeName;
        }

        // Increment a (N) suffix on the name until the resulting URL segment is unique.
        for (var i = 1; ; i++)
        {
            var candidateName = $"{nodeName} ({i})";
            var candidateSegment = shortStringHelper.CleanStringForUrlSegment(candidateName, culture);
            if (string.IsNullOrEmpty(candidateSegment) || siblingSegments.Contains(candidateSegment) is false)
            {
                return candidateName;
            }
        }
    }

    // Inserts (or converts a reserved placeholder into) the umbracoNode row, resolving Path/Level/SortOrder
    // from the parent, then patches the Path once the real NodeId is known.
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
        item.ParentKey = ResolveParentKey(item.ParentId, parent);
    }

    // Root's node row exists (umbracoNode id -1) but carries Constants.System.RootSystemKey, not the
    // semantic "no parent" value ParentKey contracts to, so its key must not be taken from the row. Every
    // other parent - the recycle bin included - carries the key the contract expects.
    private static Guid? ResolveParentKey(int parentId, NodeDto parent) =>
        parentId == Constants.System.Root ? null : parent.UniqueId;

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

        if (item is { PublishedState: PublishedState.Publishing })
        {
            // The pair just inserted (Current=false, Published=true) becomes the published version.
            // A second (Current=true, Published=false) pair is inserted for the new draft.
            item.PublishedVersionId = item.VersionId;
            dto.PublishedVersion = documentVersionDto;

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

    // Populates PropertyDataDto.SortableValue for any property whose editor implements IDataValueSortable,
    // so custom-field ordering (see ResolveCustomFieldOrderedNodeIdsAsync) can prioritize it over the raw
    // typed columns.
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
    // longer have a corresponding value. The fetch takes no row lock: every caller holds the ContentTree
    // write lock, and the scope shares one connection and transaction, so that lock already serializes this
    // read-then-write sequence.
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
        // 2100-parameter convention (Infrastructure/CLAUDE.md §6) since a document with many
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
