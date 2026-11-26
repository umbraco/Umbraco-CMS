using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Extensions;
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
///     Provides a base class for repositories managing <see cref="IPublishableContentBase" /> entities,
///     adding publishing and scheduling operations on top of
///     <see cref="AsyncContentRepositoryBase{TEntity,TRepository,TContentType,TVersionDto,TJoinRow,TRow}" />.
/// </summary>
/// <typeparam name="TEntity">The publishable content entity type.</typeparam>
/// <typeparam name="TRepository">The concrete repository type (self-referential, used for cache policy resolution).</typeparam>
/// <typeparam name="TEntityDto">The EF Core DTO type for the primary entity table.</typeparam>
/// <typeparam name="TContentVersionDto">The EF Core DTO type for content version rows.</typeparam>
/// <typeparam name="TContentCultureVariationDto">
///     The EF Core DTO type for culture variation rows; must implement <see cref="ICultureVariationDto" /> and
///     have a parameterless constructor.
/// </typeparam>
internal abstract class PublishableContentRepositoryBase<TEntity, TRepository, TEntityDto, TContentVersionDto, TContentCultureVariationDto>
    : AsyncContentRepositoryBase<
        TEntity,
        TRepository,
        IContentType,
        TContentVersionDto,
        PublishableContentJoinRow<TEntityDto, TContentVersionDto>,
        PublishableContentRow<TEntityDto, TContentVersionDto>>,
      IPublishableContentRepository<TEntity>
    where TEntity : class, IPublishableContentBase
    where TRepository : class, IRepository
    where TEntityDto : class, IPublishableContentDto
    where TContentVersionDto : class, IPublishableVersionDto
    where TContentCultureVariationDto : class, ICultureVariationDto, new()
{
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="PublishableContentRepositoryBase{TEntity,TRepository,TEntityDto,TContentVersionDto,TContentCultureVariationDto}" />
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
    protected PublishableContentRepositoryBase(
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
            contentTypeRepository,
            idKeyMap,
            tagRepository,
            jsonSerializer)
    {
    }

    /// <summary>
    ///     Builds the entity row (<typeparamref name="TEntityDto" />) inserted for a new domain entity, once its node id
    ///     and the published and edited flags being persisted are known. The node, content and shared version rows
    ///     are built by the base; this hook supplies only the kind-specific table row.
    /// </summary>
    /// <param name="entity">The domain entity being persisted, with its id assigned.</param>
    /// <param name="published">The published flag to persist; the entity still carries the value from before the save.</param>
    /// <param name="edited">The edited flag to persist.</param>
    /// <returns>The entity row ready to be inserted.</returns>
    protected abstract TEntityDto BuildEntityRow(TEntity entity, bool published, bool edited);

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

    /// <inheritdoc />
    // An explicitly set sort order is kept when no sibling holds it, so imports and copies can place an item.
    protected override async Task<int> ResolveNewItemSortOrderAsync(UmbracoDbContext db, TEntity item)
    {
        var calculateSortOrder = (item is { HasIdentity: false, SortOrder: 0 } && item.IsPropertyDirty(nameof(item.SortOrder)) is false)
                                  || await SortorderExistsAsync(db, item.ParentId, item.SortOrder);
        return calculateSortOrder ? await GetNewChildSortOrderAsync(db, item.ParentId, 0) : item.SortOrder;
    }

    /// <inheritdoc />
    protected override Task PersistIntermediateDeleteClausesAsync(UmbracoDbContext db, int nodeId) =>
        db.ContentSchedules.Where(contentSchedule => contentSchedule.NodeId == nodeId).ExecuteDeleteAsync();

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(TEntity item) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            item.AddingEntity();
            var publishing = item is { PublishedState: PublishedState.Publishing };

            AssignDefaultTemplateIfMissing(item);

            await SanitizeNamesAsync(db, item, publishing);

            item.SanitizeEntityPropertiesForXmlStorage();

            (NodeDto nodeDto, ContentDto contentDto, ContentVersionDto contentVersionDto) = BuildWriteRow(item, publishing);

            await PersistNewNodeAsync(db, item, nodeDto);
            await PersistNewContentAsync(db, item, contentDto);
            await PersistNewVersionsAsync(db, item, contentVersionDto, publishing);

            (bool edited, HashSet<string>? editedCultures) = await PersistNewPropertyDataAsync(db, item, item.PublishedVersionId);

            // if !publishing, we may have a new name != current publish name, also impacts 'edited'
            if (!publishing && item.PublishName != item.Name)
            {
                edited = true;
            }

            // at that point, when publishing, the entity still has its old Published value,
            // so the value being persisted is decided here rather than read from the entity
            bool published = publishing || item.Published;
            item.Edited = edited = !published || edited; // if not published, always edited
            db.Set<TEntityDto>().Add(BuildEntityRow(item, published, edited));
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

            ContentVersionDto version = await GetCurrentVersionRowAsync(db, item);

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
                await ApplyParentChangeAsync(db, item);
            }

            (NodeDto nodeDto, ContentDto contentDto, ContentVersionDto contentVersionDto) = BuildWriteRow(item, publishing);

            await UpdateNodeRowAsync(db, item, nodeDto);

            if (isMoving)
            {
                // Skip version/property/culture-variation/tag handling entirely, and skip the post-publish
                // flag flips below (a move never changes PublishedState).
                await OnUowRefreshedEntityAsync(item, CancellationToken.None);
                item.ResetDirtyProperties();
                IsolatedCache.Clear(EntityTypeCacheKeyPrefix + item.Key);
                return true;
            }

            await UpdateContentRowAsync(db, item, contentDto);
            await UpdateVersionRowsAsync(db, item, contentVersionDto, version.PreventCleanup, BuildEntityVersionRow(item, publishing));

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

            bool published = item.PublishedState switch
            {
                PublishedState.Publishing => true,
                PublishedState.Unpublishing => false,
                _ => item.Published,
            };

            item.Edited = edited = !published || edited; // if not published, always edited

            await db.Set<TEntityDto>().Where(entityDto => entityDto.NodeId == item.Id).ExecuteUpdateAsync(setters => setters
                .SetProperty(entityDto => entityDto.Published, published)
                .SetProperty(entityDto => entityDto.Edited, edited));

            // If entity is publishing, update tags; else leave tags there. This means that implicitly
            // unpublished, or trashed, entities *still* have tags in the database. Tags are set here (before
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

    private async Task PersistNewVersionsAsync(UmbracoDbContext db, TEntity item, ContentVersionDto contentVersionDto, bool publishing)
    {
        await PersistNewVersionAsync(db, item, contentVersionDto, publishing);

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

    // Shared culture-variation computation for PersistNewItemAsync/PersistUpdatedItemAsync: determines which
    // cultures have an edited name, updates SetCultureEdited/AdjustDates on the entity, and builds the DTOs
    // to persist. The DB-write strategy (plain AddRange vs. delete-then-reinsert) differs between callers and
    // stays at each call site. The isNew flag reflects an asymmetry between the two callers — PersistNewItemAsync never
    // feeds a culture-name mismatch into its own 'edited' flag, PersistUpdatedItemAsync does — so the
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
    // published cultures.
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

    // Builds one entity culture-variation row per culture the entity is available or published in. The row
    // carries only LanguageId, so no culture name is set here.
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
    // repository kind, so they follow the ITemplatedContent cast.
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
                    dto.IncludeInvariantForVariant = model.IncludeInvariantForVariant;
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
                        IncludeInvariantForVariant = model.IncludeInvariantForVariant,
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
                .ThenBy(entity => entity.SortOrder)
                .ThenBy(entity => entity.Id);
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
    public virtual async Task<IEnumerable<TEntity>> GetManyAsync(Guid[] keys, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken)
        => keys.Length == 0 ? [] : await PerformGetRangeAsync(keys, propertyAliases, loadTemplates, cancellationToken);

    /// <inheritdoc />
    protected override Expression<Func<PublishableContentJoinRow<TEntityDto, TContentVersionDto>, PublishableContentRow<TEntityDto, TContentVersionDto>>> ToContentRow { get; } =
        joined => new PublishableContentRow<TEntityDto, TContentVersionDto>(
            joined.Node,
            joined.Entity,
            joined.Content,
            joined.ContentVersion,
            joined.EntityVersion,
            joined.PublishedContentVersion,
            joined.PublishedEntityVersion,
            joined.ParentNode == null ? (Guid?)null : joined.ParentNode.UniqueId);

    /// <inheritdoc />
    // The entity's own published flag, not the current version's - a draft version of a
    // published entity carries Published = false on its version row.
    // TODO: when ordering.Culture is set, order variant content by whether a culture variation row
    // exists for that culture (the NPoco ApplySystemOrdering CASE), not by the table flag. [NL]
    protected override Expression<Func<PublishableContentJoinRow<TEntityDto, TContentVersionDto>, bool>>? PublishedOrderingSelector =>
        joined => joined.Entity.Published;

    /// <inheritdoc />
    // Deliberately not gated on the entity's own published flag: an unpublished entity keeps its
    // published property values and names so it can be republished in exactly its previous state.
    protected override int GetPublishedVersionId(PublishableContentRow<TEntityDto, TContentVersionDto> row) =>
        row.PublishedContentVersion?.Id ?? 0;

    /// <inheritdoc />
    protected override async Task<Action<TEntity, PublishableContentRow<TEntityDto, TContentVersionDto>>> PrepareVariationsAsync(
        UmbracoDbContext db,
        IReadOnlyList<PublishableContentRow<TEntityDto, TContentVersionDto>> rows,
        IReadOnlyList<int> versionIds,
        IReadOnlyDictionary<int, IContentType> contentTypeMap,
        CancellationToken cancellationToken)
    {
        int[] nodeIds = [.. rows.Select(row => row.Node.NodeId)];

        (Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> contentVersionCultureVariationsByVersionId,
         Dictionary<int, IReadOnlyList<TContentCultureVariationDto>> entityCultureVariationsByNodeId,
         Dictionary<int, string?> isoCodeByLanguageId) =
            await LoadVariationsAsync(db, versionIds, nodeIds, contentTypeMap, cancellationToken);

        return (entity, row) => ApplyVariations(
            entity,
            row.ContentVersion.Id,
            GetPublishedVersionId(row),
            contentVersionCultureVariationsByVersionId,
            entityCultureVariationsByNodeId.GetValueOrDefault(row.Node.NodeId, []),
            isoCodeByLanguageId);
    }

    private async Task<(
        Dictionary<int, IReadOnlyList<ContentVersionCultureVariationDto>> ContentVersionCultureVariationsByVersionId,
        Dictionary<int, IReadOnlyList<TContentCultureVariationDto>> EntityCultureVariationsByNodeId,
        Dictionary<int, string?> IsoCodeByLanguageId)> LoadVariationsAsync(
            UmbracoDbContext db,
            IReadOnlyList<int> versionIds,
            int[] nodeIds,
            IReadOnlyDictionary<int, IContentType> contentTypeMap,
            CancellationToken cancellationToken)
    {
        // Skip variation queries entirely when no loaded content type varies by culture —
        // the common case for sites with only invariant content types.
        if (!contentTypeMap.Values.Any(contentType => contentType.VariesByCulture()))
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
            dto.Action == ContentScheduleAction.Release.ToString() ? ContentScheduleAction.Release : ContentScheduleAction.Expire,
            dto.IncludeInvariantForVariant);

    /// <summary>
    ///     Ensures the entity has a valid, sibling-unique invariant name and, for variant content, sibling-unique
    ///     names per culture.
    /// </summary>
    protected async Task SanitizeNamesAsync(UmbracoDbContext db, TEntity content, bool publishing)
    {
        await SanitizeNamesAsync(db, content);
        await EnsureVariantNamesAreUniqueAsync(db, content, publishing);
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
