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
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
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
///     The EF Core DTO type for culture variation rows; must have a parameterless constructor.
/// </typeparam>
internal abstract class AsyncPublishableContentRepositoryBase<TEntity, TRepository, TEntityDto, TContentVersionDto, TContentCultureVariationDto>
    : AsyncContentRepositoryBase<TEntity, TRepository>, IAsyncPublishableContentRepository<TEntity>
    where TEntity : class, IPublishableContentBase
    where TRepository : class, IRepository
    where TEntityDto : class, IPublishableContentDto<TContentVersionDto>
    where TContentVersionDto : class, IContentVersionDto
    where TContentCultureVariationDto : class, new()
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
            cacheSyncService)
    {
        ContentTypeRepository = contentTypeRepository;
    }

    /// <summary>Gets the content type repository used to resolve content types by their integer ID.</summary>
    protected IContentTypeRepository ContentTypeRepository { get; }

    /// <summary>
    ///     Builds a domain entity from an EF Core <typeparamref name="TEntityDto" /> and its associated content type.
    /// </summary>
    /// <param name="entityDto">The primary entity DTO loaded from the database.</param>
    /// <param name="contentType">
    ///     The content type for the entity, or <see langword="null" /> if the content type could not be resolved.
    /// </param>
    /// <returns>The constructed domain entity.</returns>
    protected abstract TEntity BuildEntity(TEntityDto entityDto, IContentType? contentType);

    /// <summary>
    ///     Builds the EF Core <typeparamref name="TEntityDto" /> representation of a domain entity for persistence.
    /// </summary>
    /// <param name="entity">The domain entity to convert.</param>
    /// <returns>The DTO ready to be saved to the database.</returns>
    protected abstract TEntityDto BuildEntityDto(TEntity entity);

    /// <inheritdoc />
    protected override async Task PerformDeleteVersionAsync(int versionId, CancellationToken cancellationToken) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            await db.PropertyData
                .Where(x => x.VersionId == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            await db.ContentVersionCultureVariations
                .Where(x => x.VersionId == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            await db.Set<TContentVersionDto>()
                .Where(x => x.Id == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            await db.ContentVersions
                .Where(x => x.Id == versionId)
                .ExecuteDeleteAsync(cancellationToken);

            return true;
        });

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
                .Where(cs => cs.NodeId == nodeId)
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
                .Where(cs => cs.NodeId == content.Id)
                .ToDictionaryAsync(cs => cs.Id, cancellationToken);

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
                .Where(cs => cs.Date <= date)
                .Where(IsForThisObjectType(db))
                .ExecuteDeleteAsync(cancellationToken);
        });

    /// <inheritdoc />
    public virtual Task ClearScheduleAsync(DateTime date, ContentScheduleAction action, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync<object>(async db =>
        {
            string actionString = action.ToString();
            await db.ContentSchedules
                .Where(cs => cs.Date <= date && cs.Action == actionString)
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
                .Where(cs => cs.Action == actionString && cs.Date <= date)
                .AnyAsync(IsForThisObjectType(db), cancellationToken);
        });

    // Shared by ClearScheduleAsync (both overloads) and HasScheduledContentAsync - guards a
    // ContentScheduleDto against belonging to this repository's entity kind, since ContentSchedules
    // itself carries no object-type column of its own.
    private Expression<Func<ContentScheduleDto, bool>> IsForThisObjectType(UmbracoDbContext db) =>
        cs => db.Nodes.Any(n => n.NodeId == cs.NodeId && n.NodeObjectType == NodeObjectTypeKey);

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
                .Where(cs => cs.Action == actionString && cs.Date <= date)
                .Join(db.Nodes.Where(n => n.NodeObjectType == NodeObjectTypeKey), cs => cs.NodeId, n => n.NodeId, (cs, n) => n.UniqueId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (keys.Count == 0)
            {
                return Enumerable.Empty<TEntity>();
            }

            // PerformGetManyAsync is the same abstract hydration hook the public GetManyAsync uses
            // (declared on AsyncEntityRepositoryBase), reused directly here rather than duplicating the
            // concrete repository's multi-join entity-assembly logic.
            return await PerformGetManyAsync(keys.ToArray()) ?? Enumerable.Empty<TEntity>();
        });

    /// <inheritdoc />
    public virtual Task<int> CountPublishedAsync(string? contentTypeAlias, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<NodeDto> query = PublishedNodes(db)
                .Where(n => n.NodeObjectType == NodeObjectTypeKey && !n.Trashed);

            if (!string.IsNullOrWhiteSpace(contentTypeAlias))
            {
                query = FilterByContentTypeAlias(query, db, contentTypeAlias);
            }

            return await query.CountAsync(cancellationToken);
        });

    /// <summary>
    ///     Nodes of this repository's entity kind whose table-level "published" flag (<see cref="IPublishableContentDto{TVersionDto}.Published" />) is set.
    /// </summary>
    /// <remarks>
    ///     Callers still need to filter by <see cref="NodeObjectTypeKey" />/<c>Trashed</c> themselves —
    ///     this only joins on the published flag. <c>protected</c> so <see cref="IsPathPublishedAsync" />
    ///     overrides in concrete repositories can reuse it.
    /// </remarks>
    protected static IQueryable<NodeDto> PublishedNodes(UmbracoDbContext db) =>
        db.Nodes.Join(db.Set<TEntityDto>().Where(e => e.Published), n => n.NodeId, e => e.NodeId, (n, e) => n);

    /// <inheritdoc />
    public virtual Task<bool> IsPathPublishedAsync(TEntity? content, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

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
                List<Guid> batchKeys = batch.ToList();

                List<(Guid Key, ContentScheduleDto Dto)> rows = await db.ContentSchedules
                    .Join(db.Nodes.Where(n => batchKeys.Contains(n.UniqueId)), cs => cs.NodeId, n => n.NodeId, (cs, n) => new { n.UniqueId, cs })
                    .Select(joined => new ValueTuple<Guid, ContentScheduleDto>(joined.UniqueId, joined.cs))
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

    private async Task<ContentSchedule> ToContentScheduleAsync(ContentScheduleDto dto) =>
        new(
            dto.Id,
            await LanguageRepository.GetIsoCodeByIdAsync(dto.LanguageId) ?? Constants.System.InvariantCulture,
            dto.Date,
            dto.Action == ContentScheduleAction.Release.ToString() ? ContentScheduleAction.Release : ContentScheduleAction.Expire);

    /// <summary>
    ///     Gets a value indicating whether saving must keep names unique among siblings. Mirrors NPoco's
    ///     <c>PublishableContentRepositoryBase.EnsureUniqueNaming</c>.
    /// </summary>
    protected virtual bool EnsureUniqueNaming => true;

    /// <summary>
    ///     Ensures the entity has a valid, sibling-unique invariant name and, for variant content, sibling-unique
    ///     names per culture. Ported from NPoco's <c>PublishableContentRepositoryBase.SanitizeNames</c>.
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
            content.Name = defaultCulture != null &&
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
    ///     to add further uniqueness checks (e.g. URL segment collisions), as <c>AsyncDocumentRepository</c> does.
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
    ///     Fetches every sibling under <paramref name="parentId" /> (of this repository's <see cref="NodeObjectTypeKey" />)
    ///     and resolves a sibling-unique name for <paramref name="nodeName" />, returning both so overrides of
    ///     <see cref="EnsureUniqueNodeNameAsync" /> can run further checks against the same sibling list without
    ///     re-querying. Deliberately unfiltered (no name-prefix narrowing) — unlike NPoco's generic
    ///     Media/DataType-oriented sibling fetch, a URL-segment collision check (see <c>AsyncDocumentRepository</c>)
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
            .Join(db.ContentVersions.Where(cv => cv.Current), ccv => ccv.VersionId, cv => cv.Id, (ccv, cv) => new { ccv, cv })
            .Join(
                db.Nodes.Where(n => n.NodeObjectType == NodeObjectTypeKey && n.ParentId == content.ParentId && n.NodeId != content.Id),
                joined => joined.cv.NodeId,
                node => node.NodeId,
                (joined, node) => new { joined.ccv.Id, joined.ccv.Name, joined.ccv.LanguageId })
            .ToListAsync();

        if (names.Count == 0)
        {
            return;
        }

        // note: the code below means we are going to unique-ify every culture names, regardless
        // of whether the name has changed (ie the culture has been updated) - some saving culture
        // fr-FR could cause culture en-UK name to change - not sure that is clean
        ILookup<int, (int Id, string? Name, int LanguageId)> namesByLanguage = names
            .Select(n => (n.Id, n.Name, n.LanguageId))
            .ToLookup(n => n.LanguageId);

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

            IEnumerable<(int Id, string? Name, int LanguageId)> cultureNames = namesByLanguage[langId.Value];
            if (!cultureNames.Any())
            {
                continue;
            }

            // get a unique name (literal duplicates first, then subclass-specific checks)
            List<SimilarNodeName> otherNames = cultureNames.Select(n => new SimilarNodeName { Id = n.Id, Name = n.Name }).ToList();
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
