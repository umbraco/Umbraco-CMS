using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The EF Core repository for <see cref="IMedia" /> entities. Everything generic to content lives on
///     <see cref="AsyncContentRepositoryBase{TEntity,TRepository,TContentType,TVersionDto,TJoinRow,TRow}" />; this
///     class supplies the media version table, keys, rows, delete clauses and the lookup by file path.
/// </summary>
internal sealed partial class AsyncMediaRepository
    : AsyncContentRepositoryBase<
        IMedia,
        AsyncMediaRepository,
        IMediaType,
        MediaVersionDto,
        ContentJoinRow<MediaVersionDto>,
        ContentRow<MediaVersionDto>>,
      IAsyncMediaRepository
{
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncMediaRepository" /> class.
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
    /// <param name="mediaTypeRepository">The media type repository.</param>
    /// <param name="idKeyMap">The id/key map, used to resolve data types for tag and sortable-value handling.</param>
    /// <param name="tagRepository">The tag repository, used to persist tag values for tag-enabled properties on save.</param>
    /// <param name="jsonSerializer">The JSON serializer, used to parse legacy JSON-stored tag values.</param>
    /// <param name="mediaUrlGenerators">The generators that resolve the stored file path from the file property.</param>
    public AsyncMediaRepository(
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
        IMediaTypeRepository mediaTypeRepository,
        IIdKeyMap idKeyMap,
        ITagRepository tagRepository,
        IJsonSerializer jsonSerializer,
        MediaUrlGeneratorCollection mediaUrlGenerators)
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
            mediaTypeRepository,
            idKeyMap,
            tagRepository,
            jsonSerializer)
    {
        _mediaUrlGenerators = mediaUrlGenerators;
    }

    /// <inheritdoc />
    public override Guid RecycleBinKey => Constants.System.RecycleBinMediaKey;

    /// <inheritdoc />
    protected override Guid NodeObjectTypeKey => Constants.ObjectTypes.Media;

    /// <inheritdoc />
    protected override AsyncMediaRepository This => this;

    /// <inheritdoc />
    protected override string RecycleBinCacheKey => CacheKeys.MediaRecycleBinCacheKey;

    /// <inheritdoc />
    protected override Expression<Func<ContentJoinRow<MediaVersionDto>, ContentRow<MediaVersionDto>>> ToContentRow { get; } =
        joined => new ContentRow<MediaVersionDto>(
            joined.Node,
            joined.Content,
            joined.ContentVersion,
            joined.EntityVersion,
            joined.ParentNode == null ? (Guid?)null : joined.ParentNode.UniqueId);

    /// <inheritdoc />
    public Task<PagedModel<IMedia>> GetByLevelAsync(int level, int skip, int take, Ordering? ordering, CancellationToken cancellationToken) =>
        GetByLevelCoreAsync(level, skip, take, ordering, cancellationToken);

    /// <inheritdoc />
    public Task<PagedModel<IMedia>> GetAncestorsAsync(Guid key, int skip, int take, CancellationToken cancellationToken) =>
        GetAncestorsCoreAsync(key, skip, take, cancellationToken);

    /// <inheritdoc />
    public Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken) =>
        RecycleBinSmellsCoreAsync(cancellationToken);

    /// <inheritdoc />
    public Task<IMedia?> GetMediaByPathAsync(string mediaPath, CancellationToken cancellationToken)
    {
        // A resized rendition such as "/media/1024/koala_403x328.jpg" is stored as "/media/1024/koala.jpg".
        var storedPath = ResizeSuffixRegex().Replace(mediaPath, string.Empty, 1);

        return AmbientScope.ExecuteWithContextAsync(async db =>
        {
            ContentRow<MediaVersionDto>? row = await BuildBaseQuery(db, db.Nodes.Where(node => node.NodeObjectType == NodeObjectTypeKey))
                .Where(joined => joined.EntityVersion.Path == storedPath)
                .OrderBy(joined => joined.Node.NodeId)
                .Select(ToContentRow)
                .FirstOrDefaultAsync(cancellationToken);

            if (row is null)
            {
                return null;
            }

            List<IMedia> entities = await AssembleEntitiesAsync(db, [row], propertyAliases: null, loadTemplates: false, cancellationToken);
            return entities.FirstOrDefault();
        });
    }

    // The suffix directly before the final extension only, so an underscore or a dot elsewhere in the path is kept.
    [GeneratedRegex(@"_[0-9]+x[0-9]+(?=\.[^./]*$)")]
    private static partial Regex ResizeSuffixRegex();

    /// <inheritdoc />
    protected override Task PersistEntitySpecificDeleteClausesAsync(UmbracoDbContext db, int nodeId) =>
        db.UserGroups
            .Where(userGroup => userGroup.StartMediaId == nodeId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(userGroup => userGroup.StartMediaId, (int?)null));

    /// <inheritdoc />
    // A media item has exactly one version, which is its current one, so an existing version can only ever be
    // refused; the guard is here so a caller gets the same answer as from the other content kinds.
    protected override Task PerformDeleteVersionAsync(int versionId, CancellationToken cancellationToken) =>
        AmbientScope.ExecuteWithContextAsync(async db =>
        {
            bool? current = await db.ContentVersions
                .Where(contentVersion => contentVersion.Id == versionId)
                .Select(contentVersion => (bool?)contentVersion.Current)
                .SingleOrDefaultAsync(cancellationToken);

            if (current is true)
            {
                throw new InvalidOperationException("Cannot delete the current version.");
            }

            return true;
        });

    /// <inheritdoc />
    protected override Task OnUowRefreshedEntityAsync(IMedia entity, CancellationToken cancellationToken)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        EventAggregator.Publish(new MediaRefreshNotification(entity, new EventMessages()));
#pragma warning restore CS0618
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override IMedia BuildEntity(ContentRow<MediaVersionDto> row, IMediaType contentType) =>
        ContentBaseFactory.BuildEntity(row.Node, row.ContentVersion, contentType);

    /// <inheritdoc />
    protected override MediaVersionDto BuildEntityVersionRow(IMedia entity, bool publishing) =>
        ContentBaseFactory.BuildEFCoreMediaVersionDto(entity, _mediaUrlGenerators);

    /// <inheritdoc />
    protected override Task UpdateEntityVersionRowAsync(UmbracoDbContext db, MediaVersionDto entityVersion) =>
        db.MediaVersions.Where(mediaVersion => mediaVersion.Id == entityVersion.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(mediaVersion => mediaVersion.Path, entityVersion.Path));

    /// <inheritdoc />
    protected override IQueryable<ContentJoinRow<MediaVersionDto>> BuildBaseQuery(
        UmbracoDbContext db,
        IQueryable<NodeDto> nodes,
        IQueryable<ContentVersionDto> contentVersions) =>
        nodes
            .Join(contentVersions, node => node.NodeId, contentVersion => contentVersion.NodeId, (node, contentVersion) => new { node, contentVersion })
            .Join(db.Content, joined => joined.node.NodeId, content => content.NodeId, (joined, content) => new { joined.node, joined.contentVersion, content })
            .Join(db.MediaVersions, joined => joined.contentVersion.Id, mediaVersion => mediaVersion.Id, (joined, mediaVersion) => new { joined.node, joined.contentVersion, joined.content, mediaVersion })
            .GroupJoin(db.ContentTypes, joined => joined.content.ContentTypeId, contentType => contentType.NodeId, (joined, contentTypeGroup) => new { joined.node, joined.contentVersion, joined.content, joined.mediaVersion, contentTypeGroup })
            .SelectMany(joined => joined.contentTypeGroup.DefaultIfEmpty(), (joined, contentType) => new { joined.node, joined.contentVersion, joined.content, joined.mediaVersion, contentType })
            .GroupJoin(db.Nodes, joined => joined.node.ParentId, parentNode => parentNode.NodeId, (joined, parentNodeGroup) => new { joined.node, joined.contentVersion, joined.content, joined.mediaVersion, joined.contentType, parentNodeGroup })
            .SelectMany(joined => joined.parentNodeGroup.DefaultIfEmpty(), (joined, parentNode) => new { joined.node, joined.contentVersion, joined.content, joined.mediaVersion, joined.contentType, parentNode })
            .Select(joined => new ContentJoinRow<MediaVersionDto>
            {
                Node = joined.node,
                Content = joined.content,
                ContentVersion = joined.contentVersion,
                EntityVersion = joined.mediaVersion,
                ContentType = joined.contentType,
                ParentNode = joined.parentNode,
            });
}
