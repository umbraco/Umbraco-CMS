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
    /// <param name="idKeyMap">The id/key map, used to resolve data types for tag and sortable-value handling.</param>
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
            contentTypeRepository,
            idKeyMap,
            tagRepository,
            jsonSerializer)
    {
        _templateRepository = templateRepository;
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
    protected override IContent BuildEntity(ContentRow<DocumentDto, DocumentVersionDto> row, IContentType? contentType) =>
        ContentBaseFactory.BuildEntity(row.Entity, row.Node, row.ContentVersion, row.PublishedContentVersion, contentType);

    /// <inheritdoc />
    protected override DocumentDto BuildEntityRow(IContent entity, bool published, bool edited) =>
        ContentBaseFactory.BuildEFCoreDocumentDto(entity, published, edited);

    /// <inheritdoc />
    protected override DocumentVersionDto BuildEntityVersionRow(IContent entity, bool publishing) =>
        ContentBaseFactory.BuildEFCoreDocumentVersionDto(entity, publishing);

    /// <inheritdoc />
    protected override Task UpdateEntityVersionRowAsync(UmbracoDbContext db, DocumentVersionDto entityVersion) =>
        db.DocumentVersions.Where(documentVersion => documentVersion.Id == entityVersion.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(documentVersion => documentVersion.TemplateId, entityVersion.TemplateId)
            .SetProperty(documentVersion => documentVersion.Published, entityVersion.Published));

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
}
