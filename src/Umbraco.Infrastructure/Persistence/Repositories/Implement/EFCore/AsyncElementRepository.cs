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
///     The EF Core repository for <see cref="IElement" /> entities. Everything generic to publishable content lives
///     on <see cref="AsyncPublishableContentRepositoryBase{TEntity,TRepository,TEntityDto,TContentVersionDto,TContentCultureVariationDto}" />;
///     this class supplies the element tables, keys, rows and delete clauses.
/// </summary>
internal sealed class AsyncElementRepository
    : AsyncPublishableContentRepositoryBase<
        IElement,
        AsyncElementRepository,
        ElementDto,
        ElementVersionDto,
        ElementCultureVariationDto>,
      IAsyncElementRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncElementRepository" /> class.
    /// </summary>
    public AsyncElementRepository(
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

    /// <inheritdoc />
    public override Guid RecycleBinKey => Constants.System.RecycleBinElementKey;

    /// <inheritdoc />
    protected override Guid NodeObjectTypeKey => Constants.ObjectTypes.Element;

    /// <inheritdoc />
    protected override AsyncElementRepository This => this;

    /// <inheritdoc />
    protected override string RecycleBinCacheKey => CacheKeys.ElementRecycleBinCacheKey;

    /// <inheritdoc />
    // Elements are organised in containers, which carry no publish state, so an element is reachable as soon as it
    // is published and not trashed.
    public override Task<bool> IsPathPublishedAsync(IElement? content, CancellationToken cancellationToken) =>
        Task.FromResult(content is { Trashed: false, Published: true });

    /// <inheritdoc />
    protected override async Task PersistEntitySpecificDeleteClausesAsync(UmbracoDbContext db, int nodeId)
    {
        await db.UserGroups
            .Where(userGroup => userGroup.StartElementId == nodeId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(userGroup => userGroup.StartElementId, (int?)null));

        await db.Elements.Where(element => element.NodeId == nodeId).ExecuteDeleteAsync();
        await db.ElementCultureVariations.Where(cultureVariation => cultureVariation.NodeId == nodeId).ExecuteDeleteAsync();

        IQueryable<int> versionIdQuery = db.ContentVersions.Where(contentVersion => contentVersion.NodeId == nodeId).Select(contentVersion => contentVersion.Id);
        await db.ElementVersions.Where(elementVersion => versionIdQuery.Contains(elementVersion.Id)).ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    protected override Task OnUowRefreshedEntityAsync(IElement entity, CancellationToken cancellationToken)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        EventAggregator.Publish(new ElementRefreshNotification(entity, new EventMessages()));
#pragma warning restore CS0618
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override IElement BuildEntity(ContentRow<ElementDto, ElementVersionDto> row, IContentType? contentType) =>
        ContentBaseFactory.BuildEntity(row.Entity, row.Node, row.ContentVersion, row.PublishedContentVersion, contentType);

    /// <inheritdoc />
    protected override ElementDto BuildEntityRow(IElement entity, bool published, bool edited) =>
        ContentBaseFactory.BuildEFCoreElementDto(entity, published, edited);

    /// <inheritdoc />
    protected override ElementVersionDto BuildEntityVersionRow(IElement entity, bool publishing) =>
        ContentBaseFactory.BuildEFCoreElementVersionDto(entity, publishing);

    /// <inheritdoc />
    protected override Task UpdateEntityVersionRowAsync(UmbracoDbContext db, ElementVersionDto entityVersion) =>
        db.ElementVersions.Where(elementVersion => elementVersion.Id == entityVersion.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(elementVersion => elementVersion.Published, entityVersion.Published));

    /// <inheritdoc />
    protected override IQueryable<ContentJoinRow<ElementDto, ElementVersionDto>> BuildBaseQuery(
        UmbracoDbContext db,
        IQueryable<NodeDto> nodes,
        IQueryable<ContentVersionDto> contentVersions)
    {
        var publishedSubquery = db.ContentVersions
            .Join(
                db.ElementVersions.Where(elementVersion => elementVersion.Published),
                contentVersion => contentVersion.Id,
                elementVersion => elementVersion.Id,
                (contentVersion, elementVersion) => new { contentVersion, elementVersion });

        return nodes
            .Join(contentVersions, node => node.NodeId, contentVersion => contentVersion.NodeId, (node, contentVersion) => new { node, contentVersion })
            .Join(db.Elements, joined => joined.node.NodeId, element => element.NodeId, (joined, element) => new { joined.node, joined.contentVersion, element })
            .Join(db.Content, joined => joined.node.NodeId, content => content.NodeId, (joined, content) => new { joined.node, joined.contentVersion, joined.element, content })
            .Join(db.ElementVersions, joined => joined.contentVersion.Id, elementVersion => elementVersion.Id, (joined, elementVersion) => new { joined.node, joined.contentVersion, joined.element, joined.content, elementVersion })
            .GroupJoin(db.ContentTypes, joined => joined.content.ContentTypeId, contentType => contentType.NodeId, (joined, contentTypeGroup) => new { joined.node, joined.contentVersion, joined.element, joined.content, joined.elementVersion, contentTypeGroup })
            .SelectMany(joined => joined.contentTypeGroup.DefaultIfEmpty(), (joined, contentType) => new { joined.node, joined.contentVersion, joined.element, joined.content, joined.elementVersion, contentType })
            .GroupJoin(publishedSubquery, joined => joined.node.NodeId, publishedVersion => publishedVersion.contentVersion.NodeId, (joined, publishedVersionGroup) => new { joined.node, joined.contentVersion, joined.element, joined.content, joined.elementVersion, joined.contentType, publishedVersionGroup })
            .SelectMany(joined => joined.publishedVersionGroup.DefaultIfEmpty(), (joined, publishedVersion) => new { joined.node, joined.contentVersion, joined.element, joined.content, joined.elementVersion, joined.contentType, publishedVersion })
            .GroupJoin(db.Nodes, joined => joined.node.ParentId, parentNode => parentNode.NodeId, (joined, parentNodeGroup) => new { joined.node, joined.contentVersion, joined.element, joined.content, joined.elementVersion, joined.contentType, joined.publishedVersion, parentNodeGroup })
            .SelectMany(joined => joined.parentNodeGroup.DefaultIfEmpty(), (joined, parentNode) => new { joined.node, joined.contentVersion, joined.element, joined.content, joined.elementVersion, joined.contentType, joined.publishedVersion, parentNode })
            .Select(joined => new ContentJoinRow<ElementDto, ElementVersionDto>
            {
                Node = joined.node,
                Entity = joined.element,
                Content = joined.content,
                ContentVersion = joined.contentVersion,
                EntityVersion = joined.elementVersion,
                ContentType = joined.contentType,
                PublishedContentVersion = joined.publishedVersion != null ? joined.publishedVersion.contentVersion : null,
                PublishedEntityVersion = joined.publishedVersion != null ? joined.publishedVersion.elementVersion : null,
                ParentNode = joined.parentNode,
            });
    }
}
